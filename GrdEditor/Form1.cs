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
            // _map = new GrdMap(@"C:\Users\Mixon\GRD\relief.grd");
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

        GrdMap _map = null;

        const Single[] _high_color =  { 1.0f, 0.753f, 0.0f };
        const Single[] _low_color = { 0.0f, 0.69f, 0.314f };
        Single z_factor, z_offset;

        private void CalculateZTransformation()
        {
            z_factor = 1.0f / Convert.ToSingle(_map.ZMax-_map.ZMin);
            z_offset = -z_factor * Convert.ToSingle(_map.ZMin);
        }

        private Color GetCellColor(int row, int col)
        {
            Int32 i;
            Int32[] color = new Int32[3];
            Single alpha = Convert.ToSingle(_map[row, col]) * z_factor + z_offset;
            Single beta = 1.0f - alpha;

            for (i = 0; i < 3; ++i)
            {
                color[i] = Convert.ToInt32(_low_color[i] * alpha + _high_color[i] * beta);
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
    }
}
