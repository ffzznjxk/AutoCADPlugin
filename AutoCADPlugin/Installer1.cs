using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoCADPlugin
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
            stateSaver.Add("TargetDir", Context.Parameters["DP_TargetDir"].ToString());
        }
        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);
            string installdir = savedState["TargetDir"].ToString();
            string installpath = Path.Combine(installdir, "AutoCADPlugin.dll").Replace("\\\\", "\\");//部署好的DLL的路径
            AddOrDeleteKey(true, @installpath);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            AddOrDeleteKey(false);
            base.Uninstall(savedState);
        }

        private static void AddOrDeleteKey(bool isAdd, string toolPath = "")
        {
            string toolName = "MyTool";
            string description = "我的工具";
            RegistryKey currUser = Registry.CurrentUser;
            RegistryKey autoCad = currUser.OpenSubKey("SOFTWARE\\Autodesk\\AutoCAD");
            try
            {
                foreach (string ver in autoCad.GetSubKeyNames())
                {
                    var verKey = autoCad.OpenSubKey(ver);
                    if (verKey == null) continue;
                    foreach (var lan in verKey.GetSubKeyNames())
                    {
                        //CAD语言键
                        var lanKey = verKey.OpenSubKey(lan);
                        if (lanKey == null) continue;

                        //Applications键
                        var appKey = lanKey.OpenSubKey("Applications", true);
                        if (appKey == null) continue;

                        if (isAdd)
                        {
                            RegistryKey MyPrograrm = appKey.CreateSubKey(toolName);
                            MyPrograrm.SetValue("DESCRIPTION", description, RegistryValueKind.String);
                            MyPrograrm.SetValue("LOADCTRLS", 2, RegistryValueKind.DWord);
                            MyPrograrm.SetValue("LOADER", toolPath, RegistryValueKind.String);
                            MyPrograrm.SetValue("MANAGED", 1, RegistryValueKind.DWord);
                        }
                        else if (appKey.GetSubKeyNames().Contains(toolName))
                        {
                            appKey.DeleteSubKey(toolName);
                        }

                        //Profiles键
                        var profilesKey = lanKey.OpenSubKey("Profiles");

                        if (profilesKey == null) continue;

                        foreach (var profile in profilesKey.GetSubKeyNames())
                        {
                            var profileKey = profilesKey.OpenSubKey(profile);
                            if (profileKey == null) continue;

                            var variablesKey = profileKey.OpenSubKey("Variables", true);
                            if (variablesKey == null) continue;

                            var currPaths = variablesKey.GetValue("TRUSTEDPATHS").ToString();
                            var paths = currPaths.Split(';').ToList();
                            var path = Path.GetDirectoryName(toolPath);
                            if (!paths.Contains(path) && isAdd)
                            {
                                paths.Add(path);
                                variablesKey.SetValue("TRUSTEDPATHS", string.Join(";", paths));
                            }
                            else if (!isAdd)
                            {
                                paths.Remove(path);
                                variablesKey.SetValue("TRUSTEDPATHS", string.Join(";", paths));
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}

