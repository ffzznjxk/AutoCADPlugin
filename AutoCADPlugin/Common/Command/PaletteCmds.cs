using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
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
    public partial class PaletteCmds : UserControl
    {
        public PaletteCmds(Dictionary<Bitmap, List<string>> cmdInfos)
        {
            InitializeComponent();
            dicCmdInfos = cmdInfos;
        }

        Dictionary<Bitmap, List<string>> dicCmdInfos = new Dictionary<Bitmap, List<string>>();

        private void PaletteCmds_Load(object sender, EventArgs e)
        {
            this.Controls.Clear();
            for (int i = 0; i < dicCmdInfos.Count; i++)
            {
                var infos = dicCmdInfos.ElementAt(i);
                PictureBox pb = new PictureBox()
                {
                    Location = new Point(3, 3 + 32 * i),
                    Size = new Size(32, 32),
                    Image = infos.Key,
                };
                Button bn = new Button()
                {
                    Location = new Point(36, 3 + 32 * i),
                    Size = new Size(100 + this.Size.Width - 200, 32),
                    Text = infos.Value[0],
                    Tag = infos.Value[1],
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                };
                bn.Click += Bn_Click;
                TextBox tb = new TextBox()
                {
                    Location = new Point(137 + this.Size.Width - 200, 8 + 32 * i),
                    Size = new Size(60, 23),
                    Text = infos.Value[1],
                    ReadOnly = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                };
                this.Controls.Add(pb);
                this.Controls.Add(bn);
                this.Controls.Add(tb);
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
