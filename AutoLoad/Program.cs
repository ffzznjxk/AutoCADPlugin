using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoLoad
{
    static class Program
    {

        //
        static Dictionary<string, List<string>> dicToolInfos = new Dictionary<string, List<string>>()
        {
            { "索引工具", new List<string>{ "Index", "索引工具", @"C:\Program Files (x86)\Autodesk\ApplicationPlugins\Index.bundle\Contents\Windows\2016\Index.dll" } }
        };
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AutoLoad(dicToolInfos));
        }
    }
}
