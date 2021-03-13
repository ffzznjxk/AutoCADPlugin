using Autodesk.AutoCAD.Runtime;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Registry = Microsoft.Win32.Registry;
using RegistryKey = Microsoft.Win32.RegistryKey;

namespace AutoCADPlugin
{
    /// <summary>
    /// 程序加载
    /// </summary>
    public class Load
    {
        /// <summary>
        /// 自动加载
        /// </summary>
        /// <param name="dname">程序名称</param>
        /// <param name="desc">描述</param>
        /// <param name="dpath">路径</param>
        public static void AutoLoad(string dname, string desc, string dpath)
        {
            //
            RegistryKey curruser = Registry.CurrentUser;
            RegistryKey Applications = curruser.OpenSubKey("SOFTWARE\\Autodesk\\AutoCAD\\R20.1\\ACAD-F001:804\\Applications", true);
            RegistryKey MyPrograrm = Applications.CreateSubKey(dname);
            MyPrograrm.SetValue("DESCRIPTION", desc, RegistryValueKind.String);
            MyPrograrm.SetValue("LOADCTRLS", 2, RegistryValueKind.DWord);
            MyPrograrm.SetValue("LOADER", dpath, RegistryValueKind.String);
            MyPrograrm.SetValue("MANAGED", 1, RegistryValueKind.DWord);
        }
    }
}
