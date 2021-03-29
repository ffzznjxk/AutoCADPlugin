using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GeoMath.GeoMath;

namespace cal
{
    class Program
    {
        static void Main(string[] args)
        {
            //Point E = new Point(0, 0);
            //Point F = new Point(1176, 0);
            //Point G = new Point(1176, 760);
            //Point H = new Point(0, 760);

            //Lpp(F, G, out LineEquation FG);
            //Lpp(G, H, out LineEquation GH);
            //起始长宽
            double width = 300;
            double height = 0;

            //外边长宽
            double recWidth = 1176;
            double recHeight = 760;

            //Point A = new Point(0, 0);
            //Point D = new Point(0, width);
            //精度
            double accuracy = 1;
            //移动值
            double fx = 0;

            List<double> results = new List<double>();

            while (fx <= 300 && height < recHeight)
            {
                var result = GetLength(fx, width, recWidth, out height);
                results.Add(result);
                Console.WriteLine($"长度{result}, 宽度{height}");
                fx += accuracy;
            }

            Console.WriteLine($"最大长度是 {results.Max()}计算次数{results.Count}");
            //Console.WriteLine($"结果内容是{string.Join("\n",results)}");
            Console.ReadKey();

        }

        private static double GetLength(double i, double width, double recWidth, out double height)
        {
            double angle = Math.Asin(i / width);
            height = Math.Tan(angle) * (recWidth - i);
            
            return (recWidth - i) / Math.Cos(angle);
        }
    }
}
