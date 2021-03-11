using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AutoCADPlugin.Args;
using static AutoCADPlugin.Tools;

namespace AutoCADPlugin
{
    public partial class CarTool : Form
    {
        public CarTool()
        {
            InitializeComponent();
            comboBoxCarType.DataSource = new List<string> { "普通车位", "微型车位" };
            comboBoxOrderType.DataSource = new List<string> { "从上往下", "从下往上" };
        }

        private void buttonAddCar_Click(object sender, EventArgs e)
        {
            Hide();
            string filePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), fileName);
            double angle = Convert.ToDouble(numericUpDownAngle.Value) / 180 * Math.PI;
            AddCarBlock(filePath, comboBoxCarType.Text, angle);
        }

        private void buttonAddIndex_Click(object sender, EventArgs e)
        {
            Hide();
            OrderCarBlock(comboBoxOrderType.Text == "从上往下");
        }

        private void buttonSummaryCar_Click(object sender, EventArgs e)
        {
            Hide();
            AddCarTable();
        }
    }
}
