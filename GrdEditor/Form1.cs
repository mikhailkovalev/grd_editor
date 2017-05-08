using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GrdApi;

namespace GrdEditor
{
    public partial class MainForm : Form
    {
        public MainForm(String[] argv)
        {
            try
            {
                InitializeComponent();
                LoadArgs(argv);
                CalculateBoundsUsingFactors();
                _tool = new HandTool(this);
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Source:\n{0}\n\nMessage:{1}", e.Source, e.Message));
            }
        }

        private void LoadArgs(String[] argv)
        {
            if (argv.Length > 0)
            {
                _map = new GrdMap(argv[0]);
            }
            else
            {
                _map = new GrdMap(10, 10);
            }
            // Test file
            _map = new GrdMap(@"C:\Users\Mixon\GRD\relief.grd");
            SynchronizeData();
        }

        private void SynchronizeData()
        {
            // Заполнение форм
            SetCountValue(_rowCountNumericUpDown, _map.RowCount);
            SetCountValue(_columnCountNumericUpDown, _map.ColumnCount);
            _textDataTextBox.Text = _map.TextData;
            SetValue(_defaultDataNumericUpDown, Convert.ToDecimal(_map.DefaultValue));
            SetValue(_xMinNumericUpDown, Convert.ToDecimal(_map.XMin));
            SetValue(_xMaxNumericUpDown, Convert.ToDecimal(_map.XMax));
            SetValue(_yMinNumericUpDown, Convert.ToDecimal(_map.YMin));
            SetValue(_yMaxNumericUpDown, Convert.ToDecimal(_map.YMax));

            // Пересчёт коэффициентов
            CalculateZTransformation();
        }

        private void SetCountValue(NumericUpDown widget, Decimal value)
        {
            if (value > widget.Maximum)
            {
                widget.Maximum = value;
            }
            widget.Value = value;
        }

        private void SetValue(NumericUpDown widget, Decimal value)
        {
            if (value > widget.Maximum)
            {
                widget.Maximum = value;
            }
            else if (value < widget.Minimum)
            {
                widget.Minimum = value;
            }
            widget.Value = value;
        }

        private void CalculateZTransformation()
        {
            Single z_diff = Convert.ToSingle(_map.ZMax - _map.ZMin);
            if (Math.Abs(z_diff) > _eps)
            {
                z_factor = 1.0f / z_diff;
                z_offset = -z_factor * Convert.ToSingle(_map.ZMin);
            }
            else
            {
                z_factor = 0.0f;
                z_offset = 0.5f;
            }
        }

        private bool ValidCell(int row, int col)
        {
            return row >= 0 && row < _map.RowCount &&
                   col >= 0 && col < _map.ColumnCount;
        }

        private Color GetCellColor(int row, int col)
        {
            Int32 i;
            Int32[] color = new Int32[3];

            if (ValidCell(row, col))
            {

                Single alpha = Convert.ToSingle(_map[row, col]) * z_factor + z_offset;
                Single beta = 1.0f - alpha;

                for (i = 0; i < 3; ++i)
                {
                    color[i] = Convert.ToInt32(255.0f * (_low_color[i] * alpha + _high_color[i] * beta));
                }
            }
            else
            {
                bool first = ((row & 8) > 0) ^ ((col & 8) > 0);
                Single[] float_color = first ? _def_color1 : _def_color2;

                for (i = 0; i < 3; ++i)
                {
                    color[i] = Convert.ToInt32(255.0f * float_color[i]);
                }
            }
            return Color.FromArgb(color[0], color[1], color[2]);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            PictureBox box = sender as PictureBox;

            int x = 0, y = 0, w = box.Width, h = box.Height;

            // "Бюджетное" округление до ближайшего целого
            int r = GetRowFromY(y);
            int c;

            SolidBrush brush = new SolidBrush(Color.Black);

            while (y < h)
            {
                c = GetColumnFromX(x);
                
                brush.Color = GetCellColor(r, c);
                g.FillRectangle(brush, x, y, 1, 1);

                if (++x == w)
                {
                    x = 0;
                    ++y;
                    r = GetRowFromY(y);
                }
            }

            _tool.Paint(g);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _tool.MouseDownHandler(e);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _tool.MouseUpHandler(e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            _info_label.Text = String.Format("X = {0}, Y = {1}", e.X, e.Y);
            _tool.MouseMoveHandler(e);
        }

        public Single GetColumnFromXF(int x)
        {
            return rc_factor * x + c_offset;
        }

        public Single GetRowFromYF(int y)
        {
            return rc_factor * y + r_offset;
        }

        private int GetColumnFromX(int x)
        {
            int col = Convert.ToInt32(rc_factor * x + c_offset);
            return col;
        }

        private int GetRowFromY(int y)
        {
            int row = Convert.ToInt32(rc_factor * y + r_offset);
            return row;
        }

        private int GetXFromColumn(int column)
        {
            int x = Convert.ToInt32(xy_factor * column + x_offset);
            return x;
        }

        private int GetYFromRow(int row)
        {
            int y = Convert.ToInt32(xy_factor * row + x_offset);
            return y;
        }

        private void CalculateBoundsUsingFactors()
        {
            upBound = GetYFromRow(0);
            downBound = GetYFromRow(_map.RowCount - 1);
            leftBound = GetXFromColumn(0);
            rightBound = GetXFromColumn(_map.ColumnCount-1);
        }

        private void CalculateFactorsUsingBounds(Point CursorPos, PointF MapPos)
        {
            Single w = pictureBox1.Width;
            Single h = pictureBox1.Height;
            Single columnCount = Convert.ToSingle(_map.ColumnCount);
            Single rowCount = Convert.ToSingle(_map.RowCount);

            // Назовём рекомендуемой областью прямоугольник, задаваемый
            // полями leftBound и др.
            // Хотим, чтобы рекомендуемая область гарантированно
            // отображалась на экране; Рекомендуемая область не всегда
            // будет подобна прямоугольнику экрана, поэтому нужно
            // посчитать коэффициенты растяжения по обеим осям и
            // в качестве результирующего взять минимальный
            Single x_factor = (rightBound-leftBound) / (columnCount-1);
            Single y_factor = (downBound-upBound) / (rowCount-1);

            xy_factor = x_factor > y_factor ? y_factor : x_factor;

            // Теперь хотим, чтобы точка, по которой кликнули перешла в себя же
            x_offset = CursorPos.X - xy_factor * MapPos.X;
            y_offset = CursorPos.Y - xy_factor * MapPos.Y;
            /*x_offset = 0.5f * (leftBound + rightBound - xy_factor * columnCount);
            y_offset = 0.5f * (upBound + downBound - xy_factor * rowCount);*/

            //Считаем обратное преобразование:
            rc_factor = 1.0f / xy_factor;
            r_offset = -y_offset * rc_factor;
            c_offset = -x_offset * rc_factor;

            //Теперь вычислим "истинную" область, а не "рекомендуемую"
            CalculateBoundsUsingFactors();
        }

        public void Update(Point CursorPos, PointF MapPos)
        {
            CalculateFactorsUsingBounds(CursorPos, MapPos);
            pictureBox1.Invalidate();
            Refresh();
        }

        GrdMap _map = null;

        Single[] _high_color = { 1.0f, 0.753f, 0.0f };
        Single[] _low_color = { 0.0f, 0.69f, 0.314f };
        Single[] _def_color1 = { 0.0f, 0.0f, 0.0f };
        Single[] _def_color2 = { 0.5f, 0.0f, 1.0f };
        Single z_factor, z_offset;
        Single _eps = 1e-6f;

        // Преобразование координат из строк/столбцов в пиксели
        Single xy_factor = 1.0f, x_offset = 0.0f, y_offset = 0.0f;

        // Преобразование координат из пикселей в строки/столбцы
        Single rc_factor = 1.0f, r_offset = 0.0f, c_offset = 0.0f;

        // Границы, в которые переходят границы карты
        public Int32 upBound, downBound;
        public Int32 leftBound, rightBound;

        AbstractTool _tool;

        
    }
}
