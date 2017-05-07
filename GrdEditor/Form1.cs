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
            LoadArgs(argv);
            InitializeComponent();
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
        }

        GrdMap _map = null;
    }
}
