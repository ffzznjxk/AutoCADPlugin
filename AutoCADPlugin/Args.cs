using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADPlugin
{
    public static class Args
    {
        //索引块名
        public static string indexBlockName = "INDEX";
        //标索引层名
        public static string indexLayer = "Dim";
        public static short indexColorIndex = 3;

        //车位块名
        public static string carBlockName = "CAR";
        //车位层名
        public static string carLayer = "Car";
        public static short carColorIndex = 2;

        //文件名
        public static string fileName = "Car.dwg";

    }
}
