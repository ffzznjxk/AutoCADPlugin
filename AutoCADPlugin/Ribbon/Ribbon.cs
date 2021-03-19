using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AutoCADPlugin
{
    /// <summary>
    /// 窗口界面
    /// </summary>
    public static class Ribbon
    {
        /// <summary>
        /// 添加界面
        /// </summary>
        [CommandMethod("AddRibbon")]
        public static bool AddRibbon()
        {
            RibbonControl rc = ComponentManager.Ribbon;

            string tabId = "MyTools";
            var tabs = rc.Tabs.Select(d => d.Id).ToList();
            if (tabs.Contains(tabId))
            {
                var tab = rc.Tabs.FirstOrDefault(d => d.Id == tabId);
                rc.Tabs.Remove(tab);
            }

            try
            {
                RibbonTab rt = new RibbonTab
                {
                    Title = "我的工具",
                    Id = tabId,
                    IsActive = true
                };
                rc.Tabs.Add(rt);

                RibbonPanel rp = new RibbonPanel();
                rt.Panels.Add(rp);

                RibbonPanelSource rps = new RibbonPanelSource();
                rps.Title = "车位工具";
                rp.Source = rps;

                string btnName = "添加车位";
                RibbonButton rb = new RibbonButton
                {
                    Name = btnName,
                    Text = btnName,
                    ShowText = true,
                    Image = Properties.Resources.AddCar.GetBitmapImage(16),
                    LargeImage = Properties.Resources.AddCar.GetBitmapImage(32),
                    Orientation = System.Windows.Controls.Orientation.Vertical,
                    CommandHandler = new RibbonCommandHandler(),
                    CommandParameter = "AddCar",
                    Size = RibbonItemSize.Large
                };

                rps.Items.Add(rb);

                RibbonToolTip rtt = new RibbonToolTip
                {
                    Title = "添加车位",
                    Content = "添加车位的功能",
                    Command = "AddCar",
                    ExpandedContent = "添加车位，排序及统计",
                    ExpandedImage = Properties.Resources.AddCarToolTip.GetBitmapImage(),
                };
                rb.ToolTip = rtt;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
