using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCADPlugin
{
    /// <summary>
    /// 信息类面板
    /// </summary>
    public partial class PaletteInfomation : UserControl
    {
        public PaletteInfomation(Dictionary<Bitmap, List<string>> cmdContent)
        {
            InitializeComponent();
            dicCmds = cmdContent;
        }

        Dictionary<Bitmap, List<string>> dicCmds = new Dictionary<Bitmap, List<string>>();

        private void PaletteInfomation_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < dicCmds.Count; i++)
            {
                var cmd = dicCmds.ElementAt(i);

                PictureBox pb = new PictureBox
                {
                    Size = new Size(32, 32),
                    Image = cmd.Key,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = new Point(4, i * 36 + 4),
                };
                Button bn = new Button
                {
                    Text = cmd.Value[0],
                    Size = new Size(100, 32),
                    Location = new Point(40, i * 36 + 4),
                    Tag = cmd.Value[1],
                };
                bn.Click += Bn_Click;
                Label lb = new Label
                {
                    Location = new Point(150, i * 36 + 14),
                    Text = cmd.Value[1],
                };

                Controls.Add(pb);
                Controls.Add(bn);
                Controls.Add(lb);
            }
        }

        private void Bn_Click(object sender, EventArgs e)
        {
            var bn = sender as Button;
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute($"{bn.Tag}\n", true, false, false);
        }
    }
}
