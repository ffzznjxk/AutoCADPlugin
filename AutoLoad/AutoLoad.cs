using Microsoft.Win32;
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

namespace AutoLoad
{
    public partial class AutoLoad : Form
    {
        public AutoLoad(Dictionary<string, List<string>> toolInfos)
        {
            InitializeComponent();
            dicToolInfos = toolInfos;
        }
        Dictionary<string, List<string>> dicToolInfos = new Dictionary<string, List<string>>();

        Dictionary<string, string> dicCadVersion = new Dictionary<string, string>
        {
            { "R18.0", "AutoCAD 2010" },
            { "R19.1", "AutoCAD 2014" },
            { "R20.1", "AutoCAD 2016" },
            { "R23.1", "AutoCAD 2020" },
        };

        static RegistryKey currUser = Registry.CurrentUser;
        RegistryKey autoCad = currUser.OpenSubKey("SOFTWARE\\Autodesk\\AutoCAD");

        private void AutoLoad_Load(object sender, EventArgs e)
        {
            foreach (var dic in dicToolInfos)
                checkedListBoxTools.Items.Add(dic.Key, true);

            foreach (var ver in autoCad.GetSubKeyNames())
                if (dicCadVersion.ContainsKey(ver))
                    listBoxCadVersion.Items.Add(dicCadVersion[ver]);
                else
                    listBoxCadVersion.Items.Add(ver);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            AddOrDeleteKey(true);
        }

        private void AddOrDeleteKey(bool isAdd)
        {
            if (checkedListBoxTools.CheckedItems.Count == 0)
            {
                string info = "请选择工具。";
                MessageBox.Show(info, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (listBoxCadVersion.Items.Count == 0)
            {
                string info = "尚未安装AutoCAD。";
                MessageBox.Show(info, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int count = 0;
            try
            {
                foreach (string ci in checkedListBoxTools.CheckedItems)
                {
                    var infos = dicToolInfos[ci];
                    if (!File.Exists(infos[2]) && isAdd)
                    {
                        MessageBox.Show($"{infos[2]} 文件不存在，请检查。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;
                    }
                    foreach (string ver in listBoxCadVersion.Items)
                    {
                        //CAD版本键
                        RegistryKey verKey = null;
                        if (dicCadVersion.Values.Contains(ver))
                        {
                            var key = dicCadVersion.FirstOrDefault(d => d.Value == ver).Key;
                            verKey = autoCad.OpenSubKey(key);
                        }
                        else
                            verKey = autoCad.OpenSubKey(ver);
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
                                RegistryKey MyPrograrm = appKey.CreateSubKey(infos[0]);
                                MyPrograrm.SetValue("DESCRIPTION", infos[1], RegistryValueKind.String);
                                MyPrograrm.SetValue("LOADCTRLS", 2, RegistryValueKind.DWord);
                                MyPrograrm.SetValue("LOADER", infos[2], RegistryValueKind.String);
                                MyPrograrm.SetValue("MANAGED", 1, RegistryValueKind.DWord);
                            }
                            else if (appKey.GetSubKeyNames().Contains(infos[0]))
                            {
                                appKey.DeleteSubKey(infos[0]);
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
                                var path = Path.GetDirectoryName(infos[2]);
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

                            count++;
                        }
                    }
                }

                string message = isAdd ? "加载" : "卸载";
                if (count == 0)
                {
                    string info = $"工具{message}失败。";
                    MessageBox.Show(info, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string info = $"工具{message}成功。";
                    MessageBox.Show(info, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            AddOrDeleteKey(false);
        }

        private void buttonCancle_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
