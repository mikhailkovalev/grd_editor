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
            SetCountValue(_rowCountNumericUpDown, _map.RowCount);
            SetCountValue(_columnCountNumericUpDown, _map.ColumnCount);
            _textDataTextBox.Text = _map.TextData;
            SetMinMaxValue(_xMinNumericUpDown, Convert.ToDecimal(_map.XMin));
            SetMinMaxValue(_xMaxNumericUpDown, Convert.ToDecimal(_map.XMax));
            SetMinMaxValue(_yMinNumericUpDown, Convert.ToDecimal(_map.YMin));
            SetMinMaxValue(_yMaxNumericUpDown, Convert.ToDecimal(_map.YMax));
        }

        private void SetCountValue(NumericUpDown widget, Decimal value)
        {
            if (value > widget.Maximum)
            {
                widget.Maximum = value;
            }
            widget.Value = value;
        }

        private void SetMinMaxValue(NumericUpDown widget, Decimal value)
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

        private void test(object sender, EventArgs e)
        {
            
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
