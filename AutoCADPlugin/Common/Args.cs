using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADPlugin
{
    /// <summary>
    /// 参数集合
    /// </summary>
    public static class Args
    {
        ///索引块名
        public static string indexBlockName = "INDEX";
        ///标注层名
        public static string indexLayer = "Dim";
        ///标注颜色
        public static short indexColorIndex = 3;

        ///车位块名
        public static string carBlockName = "CAR";
        ///车位层名
        public static string carLayer = "Car";
        ///车位颜色
        public static short carColorIndex = 2;

        ///文件名
        public static string fileName = "Car.dwg";

    }
}
