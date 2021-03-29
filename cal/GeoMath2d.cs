using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoMath
{
    static partial class GeoMath
    {
        #region 几何参数

        //常量
        internal const double EpsL = 0.0001;
        internal const double Eps = 0.00005;
        internal const double EpsH = 0.00001;

        /// <summary>
        /// 向量相对关系
        /// </summary>
        internal enum VTV
        {
            //平行
            Parallel = -2,
            //左侧
            Left = -1,
            //共线
            Collinear = 0,
            //右侧
            Right = 1,
            //重合
            Coincide = 2,
        }

        /// <summary>
        /// 直线出入关系
        /// </summary>
        internal enum LTL
        {
            //入
            In = -1,
            //出
            Out = 1
        }

        /// <summary>
        /// 点与直线关系
        /// </summary>
        internal enum PTL
        {
            //左
            Left = -3,
            //终点
            End = -2,
            //起点
            Start = -1,
            //直线方程上
            OnLineEquation = 0,
            //直线上
            OnLine = 1,
            //右
            Right = 3,
        }

        /// <summary>
        /// 点与窗口关系
        /// </summary>
        internal enum PTB
        {
            //窗口边
            On = -1,
            //窗口外
            Outside = 0,
            //窗口内
            Inside = 1,
        }

        /// <summary>
        /// 点与三角形关系
        /// </summary>
        internal enum PTT
        {
            //窗口边
            On = -1,
            //窗口外
            Outside = 0,
            //窗口内
            Inside = 1,
            //顶点
            Vertex = -2,
            //延长线
            Extend = -3
        }

        /// <summary>
        /// 角度类型
        /// </summary>
        internal enum AT
        {
            //锐角
            AcuteAngle = 1,
            //钝角
            ObtuseAngle = -1,
            //直角
            RightAngle = 0
        }

        /// <summary>
        /// 坐标象限代号
        /// </summary>
        internal enum QC
        {
            First = 1,
            Second = 2,
            Third = 3,
            Fourth = 4,
        }

        /// <summary>
        /// 坐标象限代号
        /// </summary>
        internal enum AD
        {
            First = 0,
            Second = 1,
            Third = 2,
            Fourth = 3,
            False = 4,
        }

        /// <summary>
        /// 坐标八象限代号
        /// </summary>
        internal enum QC_8
        {
            QC1 = 1,
            QC2 = 2,
            QC3 = 3,
            QC4 = 4,
            QC5 = -4,
            QC6 = -3,
            QC7 = -2,
            QC8 = -1,
        }

        #endregion

        #region 几何结构

        /// <summary>
        /// 直线的解析方程 a*x+b*y+c=0  为统一表示，约定 a >= 0
        /// </summary>
        internal struct LineEquation
        {
            public double A;
            public double B;
            public double C;
            internal LineEquation(double d1 = 1, double d2 = -1, double d3 = 0)
            {
                A = d1;
                B = d2;
                C = d3;
            }
        };

        internal struct Point
        {
            public double X;
            public double Y;
            internal Point(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        #endregion

        #region 几何计算

        #region 一、导论

        /// <summary>
        /// 偏差范围
        /// </summary>
        /// <param name="value">测定值</param>
        /// <param name="target">目标值</param>
        /// <param name="error">偏差</param>
        /// <returns>是否范围内</returns>
        internal static bool GetError(this double value, double target, double error)
        {
            return value <= target + error && value >= target - error;
        }

        /// <summary>
        /// 正交（默认偏差值0.01）
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="parallel">指定水平</param>
        /// <returns>是否正交</returns>
        internal static bool GetOrthogonal(this double angle, object parallel = null)
        {
            angle = Math.Abs(angle) % Math.PI;
            bool IsParallel = angle.GetError(0, 0.01) || angle.GetError(Math.PI, 0.01);
            bool IsPerpendicular = angle.GetError(Math.PI / 2, 0.01);
            if (parallel == null)
                return IsParallel || IsPerpendicular;
            if ((bool)parallel)
                return IsParallel;
            else
                return IsPerpendicular;
        }

        ///// <summary>
        ///// 正交（默认偏差值0.01）
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="l2"></param>
        ///// <param name="parallel">指定水平</param>
        ///// <returns>是否正交</returns>
        //internal static bool GetOrthogonal(this LineSegment2d l1, LineSegment2d l2, object parallel = null)
        //{
        //    return l1.All(l2).GetOrthogonal(parallel);
        //}

        ///// <summary>
        ///// 正交（默认偏差值0.01）
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="le"></param>
        ///// <param name="parallel">指定水平</param>
        ///// <returns>是否正交</returns>
        //internal static bool GetOrthogonal(this LineSegment2d l1, LineEquation le, object parallel = null)
        //{
        //    l1.Lel(out LineEquation le1);
        //    return le1.All(le).GetOrthogonal(parallel);
        //}

        //#endregion

        ////计算方法
        //#region 二、数学基础
        ///// <summary>
        ///// 11.向量点积
        ///// </summary>
        ///// <param name="v1"></param>
        ///// <param name="v2"></param>
        ///// <returns></returns>
        //internal static double DotProduct(Vector2d v1, Vector2d v2)
        //{
        //    return v1.X * v2.X + v1.Y * v2.Y;
        //}

        ///// <summary>
        ///// 11.点线点积
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static double DotProduct(this Point2d p, LineSegment2d l)
        //{
        //    return DotProduct(p - l.StartPoint, l.EndPoint - l.StartPoint);
        //}

        ///// <summary>
        ///// 12.向量叉积
        ///// </summary>
        ///// <param name="v1"></param>
        ///// <param name="v2"></param>
        ///// <returns>result》0,v1在v2逆时针;result = 0,v1在v2共线;result《0,v1在v2顺时针</returns>
        //internal static double CrossProduct(Vector2d v1, Vector2d v2)
        //{
        //    return v1.X * v2.Y - v2.X * v1.Y;
        //}

        ///// <summary>
        ///// 12.向量叉积
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns>result》0,v1在v2逆时针;result = 0,v1在v2共线;result《0,v1在v2顺时针</returns>
        //internal static double CrossProduct(this Point2d p, LineSegment2d l)
        //{
        //    return CrossProduct(p, l.StartPoint, l.EndPoint);
        //}

        ///// <summary>
        ///// 12.向量叉积
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns>result》0,v1在v2逆时针;result = 0,v1在v2共线;result《0,v1在v2顺时针</returns>
        //internal static double CrossProduct(this Point2d p, Point2d p1, Point2d p2)
        //{
        //    return CrossProduct(p - p1, p2 - p1);
        //}

        ///// <summary>
        ///// 14。向量旋向
        ///// </summary>
        ///// <param name="v1"></param>
        ///// <param name="v2"></param>
        ///// <returns></returns>
        //internal static VTV Ivv(this Vector2d v1, Vector2d v2)
        //{
        //    return Isign(CrossProduct(v1, v2));
        //}

        ///// <summary>
        ///// 15.拐点判断
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <param name="p3"></param>
        ///// <returns>Right，p2凸点;Collinear，三点共线;Left，p2凹点</returns>
        //internal static VTV Ippp(this Point2d p3, Point2d p1, Point2d p2)
        //{
        //    return (p3 - p2).Ivv(p2 - p1);
        //}

        ///// <summary>
        ///// 15.拐点判断
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns>Right，p凸点;Collinear，三点共线;Left，p凹点</returns>
        //internal static VTV Ipl(this Point2d p, LineSegment2d l)
        //{
        //    return p.Ippp(l.StartPoint, l.EndPoint);
        //}

        ///// <summary>
        ///// 19.符号传递(d2符号传递d1)
        ///// </summary>
        ///// <param name="d1"></param>
        ///// <param name="d2"></param>
        ///// <returns></returns>
        //internal static double Sign(this double d1, double d2)
        //{
        //    return d2 >= 0
        //        ? Math.Abs(d1)
        //        : -Math.Abs(d1);
        //}

        ///// <summary>
        ///// 20.符号函数
        ///// </summary>
        ///// <param name="d"></param>
        ///// <returns></returns>
        //internal static VTV Isign(this double d)
        //{
        //    return Math.Abs(d) < Eps
        //        ? VTV.Collinear
        //        : d > 0
        //            ? VTV.Right
        //            : VTV.Left;
        //}

        ///// <summary>
        ///// 11-1.角度类型
        ///// </summary>
        ///// <param name="v1"></param>
        ///// <param name="v2"></param>
        ///// <returns></returns>
        //internal static AT GetAngleType(Vector2d v1, Vector2d v2)
        //{
        //    double d = DotProduct(v1, v2);
        //    return d == 0
        //        ? AT.RightAngle
        //        : d > 0
        //            ? AT.AcuteAngle
        //            : AT.ObtuseAngle;
        //}

        ///// <summary>
        ///// 11-1.角度类型
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static AT GetAngleType(this Point2d p, LineSegment2d l)
        //{
        //    return GetAngleType(p - l.StartPoint, l.EndPoint - l.StartPoint);
        //}

        ///// <summary>
        ///// 11-1.角度类型
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="l2"></param>
        ///// <returns></returns>
        //internal static AT GetAngleType(this LineSegment2d l1, LineSegment2d l2)
        //{
        //    return GetAngleType(l1.EndPoint - l1.StartPoint, l2.EndPoint - l2.StartPoint);
        //}

        ///// <summary>
        ///// 判断多边形方向（顺/逆时针）
        ///// </summary>
        ///// <param name="pts"></param>
        ///// <returns></returns>
        //internal static bool Direction(ref List<Point2d> pts)
        //{
        //    int minus = 0;
        //    if (pts.Count > 2)
        //    {
        //        for (int i = 0; i < pts.Count; i++)
        //        {
        //            Vector2d v1 = pts[(i + 1) % pts.Count] - pts[i];
        //            Vector2d v2 = pts[(i + 2) % pts.Count] - pts[(i + 1) % pts.Count];
        //            minus += (int)v2.Ivv(v1);
        //        }
        //    }
        //    if (minus > 0)
        //    {
        //        Point2d pt = pts.FirstOrDefault();
        //        pts.RemoveAt(0);
        //        pts.Reverse();
        //        pts.Insert(0, pt);
        //    }
        //    return minus < 0;
        //}
        //#endregion

        //#region 三、几何基础

        ///// <summary>
        ///// 30.向量的参数解及交点的几何数
        ///// </summary>
        ///// <param name="lp"></param>
        ///// <param name="lq"></param>
        ///// <param name="sp"></param>
        ///// <param name="sq"></param>
        ///// <param name="kp"></param>
        ///// <param name="kq"></param>
        ///// <returns></returns>
        //internal static bool Pvv(LineSegment2d lp, LineSegment2d lq, out double sp, out double sq, out LTL kp, out LTL kq)
        //{
        //    sp = 0; kp = 0; kq = 0;

        //    Vector2d vp = lp.StartPoint - lp.EndPoint;
        //    Vector2d vq = lq.StartPoint - lq.EndPoint;
        //    sq = CrossProduct(vp, vq);
        //    if (Math.Abs(sq) > Eps)
        //    {
        //        if (sq >= 0)
        //        {
        //            kp = LTL.Out;
        //            kq = LTL.In;
        //        }
        //        else
        //        {
        //            kp = LTL.In;
        //            kq = LTL.Out;
        //        }
        //        double ux = lq.StartPoint.X - lp.StartPoint.X;
        //        double vy = lq.StartPoint.Y - lp.StartPoint.Y;
        //        sp = (vq.Y * ux - vq.X * vy) / sq;
        //        sq = (vp.Y * ux - vp.X * vy) / sq;
        //        if ((sp < 1 + Eps && sp > -Eps) && (sq < 1 + Eps && sq > -Eps))
        //            return true;
        //    }
        //    return false;
        //}

        //#endregion

        //#region 四、几何变换

        ///// <summary>
        ///// 84-1.两点中点
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="c"></param>
        ///// <param name="angle"></param>
        ///// <returns></returns>
        //internal static Point2d Rot2d(this Point2d p, Point2d c, double angle)
        //{
        //    return new Point2d(c.X + (p.X - c.X) * Math.Cos(angle) - (p.Y - c.Y) * Math.Sin(angle),
        //        c.Y + (p.X - c.X) * Math.Sin(angle) + (p.Y - c.Y) * Math.Cos(angle));
        //}

        //#endregion

        //#region 五、二维几何

        ///// <summary>
        ///// 84.求分比点
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <param name="d"></param>
        ///// <param name="p"></param>
        ///// <returns>d》0 内分点，d《0 外分点;|d|《1 靠近p1,|d|》1 靠近p2;d = 1 p1-p2中点</returns>
        //internal static bool Ppp(Point2d p1, Point2d p2, double d, out Point2d p)
        //{
        //    p = Point2d.Origin;
        //    if (Math.Abs(d + 1) >= Eps)
        //    {
        //        p = new Point2d((p1.X + d * p2.X) / (1 + d), (p1.Y + d * p2.Y) / (1 + d));
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 84.求分比点
        ///// </summary>
        ///// <param name="l"></param>
        ///// <param name="d"></param>
        ///// <param name="p"></param>
        ///// <returns>d》0 内分点，d《0 外分点;|d|《1 靠近p1,|d|》1 靠近p2;d = 1 p1-p2中点</returns>
        //internal static bool Pl(this LineSegment2d l, double d, out Point2d p)
        //{
        //    return Ppp(l.StartPoint, l.EndPoint, d, out p);
        //}

        ///// <summary>
        ///// 84-1.两点中点
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns></returns>
        //internal static Point2d Ppm(Point2d p1, Point2d p2)
        //{
        //    Ppp(p1, p2, 1, out Point2d p);
        //    return p;
        //}

        ///// <summary>
        ///// 84-1.直线段中点
        ///// </summary>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static Point2d Plm(this LineSegment2d l)
        //{
        //    return Ppm(l.StartPoint, l.EndPoint);
        //}

        ///// <summary>
        ///// 85.求垂足
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <returns>p点到p1-p2线段垂足</returns>
        //internal static Point2d Ppln(this Point2d p, LineEquation le)
        //{
        //    double d = le.A * p.X + le.B * p.Y + le.C;
        //    return new Point2d(p.X - d * le.A, p.Y - d * le.B);
        //}

        ///// <summary>
        ///// 85.求垂足
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns>p点到p1-p2线段垂足</returns>
        //internal static Point2d Pppn(this Point2d p, Point2d p1, Point2d p2)
        //{
        //    Lpp(p1, p2, out LineEquation le);
        //    double d = le.A * p.X + le.B * p.Y + le.C;
        //    return new Point2d(p.X - d * le.A, p.Y - d * le.B);
        //}

        ///// <summary>
        ///// 85.求垂足
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns>p点到l垂足</returns>
        //internal static Point2d Ppln(this Point2d p, LineSegment2d l)
        //{
        //    return p.Pppn(l.StartPoint, l.EndPoint);
        //}

        ///// <summary>
        ///// 85-1.求投影向量
        ///// </summary>
        ///// <param name="v1">要投影</param>
        ///// <param name="v2">被投影</param>
        ///// <returns>投影向量</returns>
        //internal static Vector2d Vvv(this Vector2d v1, Vector2d v2)
        //{
        //    return v2 * DotProduct(v1, v2) / DotProduct(v2, v2);
        //}

        ///// <summary>
        ///// 86.求直线外定距离点
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <param name="d">d》0 前进方向右侧去点，d《0 前进方向左侧取点</param>
        ///// <returns>p点指定距离点</returns>
        //internal static Point2d Ppldn(this Point2d p, LineSegment2d l, double d)
        //{
        //    l.Lel(out LineEquation le);
        //    return p.Ppldn(le, d);
        //}

        ///// <summary>
        ///// 86.求直线外定距离点
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <param name="d">d》0 前进方向右侧去点，d《0 前进方向左侧取点</param>
        ///// <returns>p点指定距离点</returns>
        //internal static Point2d Ppldn(this Point2d p, LineEquation le, double d)
        //{
        //    return new Point2d(p.X + d * le.A, p.Y + d * le.B);
        //}

        ///// <summary>
        ///// 88.求对称点
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static Point2d Plp(this Point2d p, LineSegment2d l)
        //{
        //    l.Lel(out LineEquation le);
        //    return p.Plp(le);
        //}

        ///// <summary>
        ///// 88.求对称点
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static Point2d Plp(this Point2d p, LineEquation le)
        //{
        //    double d = 2 * (le.A * p.X + le.B * p.Y + le.C);
        //    return new Point2d(p.X - d * le.A, p.Y - d * le.B);
        //}

        ///// <summary>
        ///// 89.直线上定距离点
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <param name="d">d》0 前进方向，d《0 前进方向</param>
        ///// <returns></returns>
        //internal static Point2d Plpd(this Point2d p, LineSegment2d l, double d)
        //{
        //    l.Lel(out LineEquation le);
        //    return p.Plpd(le, d);
        //}

        ///// <summary>
        ///// 89.直线上定距离点
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <param name="d">d》0 前进方向，d《0 前进方向</param>
        ///// <returns></returns>
        //internal static Point2d Plpd(this Point2d p, LineEquation le, double d)
        //{
        //    return new Point2d(p.X - d * le.B, p.Y + d * le.A);
        //}

        ///// <summary>
        ///// 90.两直线交点
        ///// </summary>
        ///// <param name="le1"></param>
        ///// <param name="le2"></param>
        ///// <param name="p"></param>
        ///// <returns>false:平行或重合</returns>
        //internal static bool Pll(this LineEquation le1, LineEquation le2, out Point2d p)
        //{
        //    p = Point2d.Origin;
        //    double d = le1.A * le2.B - le1.B * le2.A;
        //    if (Math.Abs(d) > Eps)
        //    {
        //        p = new Point2d((le2.C * le1.B - le1.C * le2.B) / d, (le1.C * le2.A - le1.A * le2.C) / d);
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 90.两直线交点
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="l2"></param>
        ///// <param name="p"></param>
        ///// <returns>false:平行或重合</returns>
        //internal static bool Pll(this LineSegment2d l1, LineSegment2d l2, out Point2d p)
        //{
        //    return l1.GetLe().Pll(l2.GetLe(), out p);
        //}

        ///// <summary>
        ///// 92.两直线段交点
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="l2"></param>
        ///// <param name="p"></param>
        ///// <returns></returns>
        //internal static bool Plsls(this LineSegment2d l1, LineSegment2d l2, out Point2d p)
        //{
        //    p = Point2d.Origin;
        //    //
        //    if ((int)l1.StartPoint.Ipl(l2) * (int)l1.EndPoint.Ipl(l2) <= 0
        //        && (int)l2.StartPoint.Ipl(l1) * (int)l2.EndPoint.Ipl(l1) <= 0)
        //    {
        //        if (l2.Dls() > Eps)
        //            l1.GetLe().Pll(l2.GetLe(), out p);
        //        else if (l1.Dls() > Eps)
        //            l2.GetLe().Pll(l1.GetLe(), out p);
        //        else
        //            p = l1.StartPoint;
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 94.方向直线与向量求交
        ///// </summary>
        ///// <param name="le"></param>
        ///// <param name="l"></param>
        ///// <param name="p"></param>
        ///// <returns></returns>
        //internal static VTV Plv(LineEquation le, LineSegment2d l, out Point2d p)
        //{
        //    p = Point2d.Origin;

        //    if (!l.Lel(out LineEquation le1))
        //        return VTV.Collinear;

        //    if (!Pll(le, le1, out p))
        //        return Dpl(p, le) < Eps ? VTV.Coincide : VTV.Parallel;

        //    if (Math.Abs(l.EndPoint.X - l.StartPoint.X) > Math.Abs(l.EndPoint.Y - l.StartPoint.Y))
        //    {
        //        if (p.X < Math.Min(l.StartPoint.X, l.EndPoint.X) - Eps
        //            || p.X > Math.Max(l.StartPoint.X, l.EndPoint.X) + Eps)
        //            return VTV.Collinear;
        //    }
        //    else
        //    {
        //        if (p.Y < Math.Min(l.StartPoint.Y, l.EndPoint.Y) - Eps
        //            || p.Y > Math.Max(l.StartPoint.Y, l.EndPoint.Y) + Eps)
        //            return VTV.Collinear;
        //    }
        //    return Isign(le.A * le1.B - le1.A * le.B);
        //}

        /// <summary>
        /// 109.直线方程式
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="le"></param>
        /// <returns></returns>
        internal static bool Lpp(Point p1, Point p2, out LineEquation le)
        {
            le.A = p2.Y - p1.Y;
            le.B = p1.X - p2.X;
            le.C = Math.Sqrt(Math.Pow(le.A, 2) + Math.Pow(le.B, 2));
            if (le.C > Eps)
            {
                le.A /= le.C;
                le.B /= le.C;
                le.C = -le.A * p1.X - le.B * p1.Y;
                return true;
            }
            else
            {
                le.A = 1;
                le.B = 0;
                le.C = -p1.X;
                return false;
            }
        }

        ///// <summary>
        ///// 109.直线方程式
        ///// </summary>
        ///// <param name="l"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static bool Lel(this LineSegment2d l, out LineEquation le)
        //{
        //    return Lpp(l.StartPoint, l.EndPoint, out le);
        //}

        ///// <summary>
        ///// 109.直线方程式
        ///// </summary>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static LineEquation GetLe(this LineSegment2d l)
        //{
        //    Lpp(l.StartPoint, l.EndPoint, out LineEquation le);
        //    return le;
        //}

        ///// <summary>
        ///// 110.过已知点与X轴成angle夹角的直线（左侧为负）
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="angle"></param>
        ///// <returns></returns>
        //internal static LineEquation Lpax(this Point2d p, double angle)
        //{
        //    double a = Math.Sin(angle);
        //    double b = -Math.Cos(angle);
        //    double c = -(a * p.X + b * p.Y);
        //    return new LineEquation(a, b, c);
        //}

        ///// <summary>
        ///// 111.已知直线定距平行线
        ///// </summary>
        ///// <param name="l"></param>
        ///// <param name="d">d》0,l的右侧；d《0,l的左侧；</param>
        ///// <returns></returns>
        //internal static LineEquation Lld(this LineSegment2d l, double d)
        //{
        //    l.Lel(out LineEquation le);
        //    le.C -= d;
        //    return le;
        //}

        ///// <summary>
        ///// 111-1.已知直线定距平行直线段
        ///// </summary>
        ///// <param name="l"></param>
        ///// <param name="d">d》0,l的右侧；d《0,l的左侧；</param>
        ///// <returns></returns>
        //internal static LineSegment2d Lsld(this LineSegment2d l, double d)
        //{
        //    return new LineSegment2d(l.StartPoint.Ppldn(l, d), l.EndPoint.Ppldn(l, d));
        //}

        ///// <summary>
        ///// 111.已知直线定距平行线
        ///// </summary>
        ///// <param name="le"></param>
        ///// <param name="d">d》0,l的右侧；d《0,l的左侧；</param>
        ///// <returns></returns>
        //internal static LineEquation Lld(this LineEquation le, double d)
        //{
        //    le.C -= d;
        //    return new LineEquation(le.A, le.B, le.C);
        //}

        ///// <summary>
        ///// 112.过已知点与已知直线成angle夹角的直线
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <param name="angle"></param>
        ///// <returns></returns>
        //internal static LineEquation Lpla(this Point2d p, LineSegment2d l, double angle)
        //{
        //    l.Lel(out LineEquation le);
        //    double a = le.A * Math.Cos(angle) - le.B * Math.Sin(angle);
        //    double b = le.A * Math.Sin(angle) + le.B * Math.Cos(angle);
        //    double c = -(a * p.X + b * p.Y);
        //    return new LineEquation(a, b, c);
        //}

        ///// <summary>
        ///// 112.过已知点与已知直线成angle夹角的直线段终点
        ///// </summary>
        ///// <param name="l"></param>
        ///// <param name="angle"></param>
        ///// <returns></returns>
        //internal static Point2d Pla(this LineSegment2d l, double angle)
        //{
        //    return l.StartPoint.Plpd(l.StartPoint.Lpla(l, angle), l.Length);
        //}

        ///// <summary>
        ///// 112.过已知点与已知直线成angle夹角的直线段
        ///// </summary>
        ///// <param name="l"></param>
        ///// <param name="angle"></param>
        ///// <returns></returns>
        //internal static LineSegment2d Lla(this LineSegment2d l, double angle)
        //{
        //    return new LineSegment2d(l.StartPoint, l.Pla(angle));
        //}

        ///// <summary>
        ///// 112.过已知点与已知直线成angle夹角的直线
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <param name="angle"></param>
        ///// <returns></returns>
        //internal static LineEquation Lpla(this Point2d p, LineEquation le, double angle)
        //{
        //    double a = le.A * Math.Cos(angle) - le.B * Math.Sin(angle);
        //    double b = le.A * Math.Sin(angle) + le.B * Math.Cos(angle);
        //    double c = -(a * p.X + b * p.Y);
        //    return new LineEquation(a, b, c);
        //}

        ///// <summary>
        ///// 113.过已知点做已知直线垂线
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static LineEquation Lpln(this Point2d p, LineSegment2d l)
        //{
        //    l.Lel(out LineEquation le);
        //    double a = le.B;
        //    double b = -le.A;
        //    double c = -(a * p.X + b * p.Y);
        //    return new LineEquation(a, b, c);
        //}

        ///// <summary>
        ///// 113.过已知点做已知直线垂线
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static LineEquation Lpln(this Point2d p, LineEquation le)
        //{
        //    double a = le.B;
        //    double b = -le.A;
        //    double c = -(a * p.X + b * p.Y);
        //    return new LineEquation(a, b, c);
        //}

        ///// <summary>
        ///// 115.求已知两点的中垂线
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static bool Lppn(Point2d p1, Point2d p2, out LineEquation le)
        //{
        //    if (Lpp(p1, p2, out le))
        //    {
        //        le.C = -0.5 * (le.A * (p1.X + p2.X) + le.B * (p1.Y + p2.Y));
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 115.求已知两点的中垂线
        ///// </summary>
        ///// <param name="l"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static bool Lppn(this LineSegment2d l, out LineEquation le)
        //{
        //    return Lppn(l.StartPoint, l.EndPoint, out le);
        //}

        ///// <summary>
        ///// 116.过已知点已知直线做平行线
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static LineEquation Lplp(this Point2d p, LineSegment2d l)
        //{
        //    l.Lel(out LineEquation le);
        //    return new LineEquation(le.A, le.B, -(le.A * p.X + le.B * p.Y));
        //}

        ///// <summary>
        ///// 116.过已知点已知直线做平行线
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static LineEquation Lplp(this Point2d p, LineEquation le)
        //{
        //    return new LineEquation(le.A, le.B, -(le.A * p.X + le.B * p.Y));
        //}

        ///// <summary>
        ///// 119.求两直线正向角平分线
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="l2"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static bool Lll(LineSegment2d l1, LineSegment2d l2, out LineEquation le)
        //{
        //    le = new LineEquation();
        //    l1.Lel(out LineEquation le1);
        //    l2.Lel(out LineEquation le2);
        //    if (l1.Plsls(l2, out Point2d p))
        //    {
        //        Point2d p1 = p.Plpd(l1, 100);
        //        Point2d p2 = p.Plpd(l2, 100);
        //        LineEquation le11 = p1.Lplp(le1);
        //        LineEquation le22 = p2.Lplp(le2);

        //        if (Pll(le11, le22, out p1))
        //            return Lpp(p, p1, out le);
        //        return false;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 119.求两直线正向角平分线
        ///// </summary>
        ///// <param name="le1"></param>
        ///// <param name="le2"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static bool Lll(LineEquation le1, LineEquation le2, out LineEquation le)
        //{
        //    le = new LineEquation();
        //    if (Pll(le1, le2, out Point2d p))
        //    {
        //        Point2d p1 = p.Plpd(le1, 100);
        //        Point2d p2 = p.Plpd(le2, 100);
        //        LineEquation le11 = p1.Lplp(le1);
        //        LineEquation le22 = p2.Lplp(le2);

        //        if (Pll(le11, le22, out p1))
        //            return Lpp(p, p1, out le);
        //        return false;
        //    }
        //    return false;
        //}

        //#endregion

        //#region 六、二维计算

        ///// <summary>
        ///// 136.两点是否重合
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns></returns>
        //internal static bool Ispp(this Point2d p1, Point2d p2)
        //{
        //    return Math.Abs(p1.X - p2.X) <= Eps && Math.Abs(p1.Y - p2.Y) <= Eps;
        //}

        ///// <summary>
        ///// 137.点与直线关系
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static VTV IsPointOnLineEquation(this Point2d p, LineEquation le)
        //{
        //    return Isign(p.Dpl(le));
        //}

        ///// <summary>
        ///// 138.点与直线段关系
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static PTL IsPointOnLine(this Point2d p, LineSegment2d l)
        //{
        //    if (p.Ispp(l.StartPoint))
        //        return PTL.Start;
        //    if (p.Ispp(l.EndPoint))
        //        return PTL.End;
        //    l.Lel(out LineEquation le);

        //    int k = (int)p.IsPointOnLineEquation(le);
        //    if (k != 0)
        //        return (PTL)(k * 3);
        //    if (Math.Abs(le.A) < Math.Sqrt(2) / 2)
        //        k = (int)Isign(p.X - l.StartPoint.X) + (int)Isign(l.EndPoint.X - p.X);
        //    else
        //        k = (int)Isign(p.Y - l.StartPoint.Y) + (int)Isign(l.EndPoint.Y - p.Y);
        //    if (k == 0)
        //        return PTL.OnLineEquation;
        //    return PTL.OnLine;
        //}

        ///// <summary>
        ///// 140.点是否在直线上
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static bool Ipls(this Point2d p, LineSegment2d l)
        //{
        //    return p.Dpl_Length(l) < Eps;
        //}

        ///// <summary>
        ///// 146.点与窗口关系(常规)
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="ext"></param>
        ///// <returns></returns>
        //internal static PTB Is_p_to_box(this Point2d p, Extents2d ext)
        //{
        //    int n = Math.Abs(p.Code_p_to_box(ext));
        //    return n >= 4 && n <= 6 || n == 1
        //        ? PTB.On
        //        : n == 0
        //            ? PTB.Inside
        //            : PTB.Outside;
        //}

        ///// <summary>
        ///// 149.点与三角形（逆时针）关系
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <param name="p3"></param>
        ///// <returns></returns>
        //internal static PTT IsPointInTriangle(this Point2d p, Point2d p1, Point2d p2, Point2d p3)
        //{
        //    int k0 = (int)p.IsPointOnLine(new LineSegment2d(p1, p2));
        //    int k1 = (int)p.IsPointOnLine(new LineSegment2d(p2, p3));
        //    int k2 = (int)p.IsPointOnLine(new LineSegment2d(p3, p1));
        //    int kk = k0 + k1 + k2;
        //    if (kk == -9)
        //        return PTT.Inside;
        //    else if (k0 == 0 || k1 == 0 || k2 == 0)
        //        return Math.Abs(kk) == 6
        //            ? PTT.On
        //            : Math.Abs(kk) == 3
        //                ? PTT.Vertex
        //                : PTT.Extend;
        //    else
        //        return PTT.Outside;
        //}

        ///// <summary>
        ///// 150.返回点所在的坐标系象限(pi/2)
        ///// </summary>
        ///// <param name="p"></param>
        ///// <returns></returns>
        //internal static QC Quadrant_code(this Point2d p)
        //{
        //    return p.X >= 0
        //        ? p.Y >= 0
        //            ? QC.First
        //            : QC.Fourth
        //        : p.Y >= 0
        //            ? QC.Second
        //            : QC.Third;
        //}

        ///// <summary>
        ///// 150.返回向量所在的坐标系象限(pi/2)
        ///// </summary>
        ///// <param name="v"></param>
        ///// <returns></returns>
        //internal static QC Quadrant_code(this Vector2d v)
        //{
        //    return v.X >= 0
        //        ? v.Y >= 0
        //            ? QC.First
        //            : QC.Fourth
        //        : v.Y >= 0
        //            ? QC.Second
        //            : QC.Third;
        //}

        ///// <summary>
        ///// 150-1.返回点所在的坐标系象限(pi/4)
        ///// </summary>
        ///// <param name="p"></param>
        ///// <returns></returns>
        //internal static QC_8 Quadrant_8_code(this Point2d p)
        //{
        //    return p.Y >= 0
        //        ? p.X >= 0
        //            ? Math.Abs(p.X) >= Math.Abs(p.Y)
        //                ? QC_8.QC1
        //                : QC_8.QC2
        //            : Math.Abs(p.X) >= Math.Abs(p.Y)
        //                ? QC_8.QC4
        //                : QC_8.QC3
        //        : p.X >= 0
        //            ? Math.Abs(p.X) >= Math.Abs(p.Y)
        //                ? QC_8.QC8
        //                : QC_8.QC7
        //            : Math.Abs(p.X) >= Math.Abs(p.Y)
        //                ? QC_8.QC5
        //                : QC_8.QC6;
        //}

        ///// <summary>
        ///// 150-1.返回向量所在的坐标系象限(pi/4)
        ///// </summary>
        ///// <param name="v"></param>
        ///// <returns></returns>
        //internal static QC_8 Quadrant_8_code(this Vector2d v)
        //{
        //    return v.Y >= 0
        //        ? v.X >= 0
        //            ? Math.Abs(v.X) >= Math.Abs(v.Y)
        //                ? QC_8.QC1
        //                : QC_8.QC2
        //            : Math.Abs(v.X) >= Math.Abs(v.Y)
        //                ? QC_8.QC4
        //                : QC_8.QC3
        //        : v.X >= 0
        //            ? Math.Abs(v.X) >= Math.Abs(v.Y)
        //                ? QC_8.QC8
        //                : QC_8.QC7
        //            : Math.Abs(v.X) >= Math.Abs(v.Y)
        //                ? QC_8.QC5
        //                : QC_8.QC6;
        //}

        ///// <summary>
        ///// 151.点与窗口关系
        ///// +08 +09 +10 +11 +12
        ///// +03 +04 +05 +06 +07
        ///// -02 -01 +00 +01 +02
        ///// -07 -06 -05 -04 -03
        ///// -12 -11 -10 -09 -08
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="ext"></param>
        ///// <returns></returns>
        //internal static int Code_p_to_box(this Point2d p, Extents2d ext)
        //{
        //    return (int)Isign(p.X - ext.MinPoint.X) + (int)Isign(p.X - ext.MaxPoint.X)
        //        + 5 * ((int)Isign(p.Y - ext.MinPoint.Y) + (int)Isign(p.Y - ext.MaxPoint.Y));
        //}

        ///// <summary>
        ///// 164.计算两点距离
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns></returns>
        //internal static double Dpp(this Point2d p1, Point2d p2)
        //{
        //    return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        //}

        ///// <summary>
        ///// 165.计算线段长度
        ///// </summary>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static double Dls(this LineSegment2d l)
        //{
        //    return Dpp(l.StartPoint, l.EndPoint);
        //}

        ///// <summary>
        ///// 166.点到直线长度
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static double Dpl(this Point2d p, LineEquation le)
        //{
        //    return le.A * p.X + le.B * p.Y + le.C;
        //}

        ///// <summary>
        ///// 166.点到直线长度
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static double Dpl(this Point2d p, LineSegment2d l)
        //{
        //    l.Lel(out LineEquation le);
        //    return p.Dpl(le);
        //}

        ///// <summary>
        ///// 166-1.点到直线长度绝对值
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l"></param>
        ///// <returns></returns>
        //internal static double Dpl_Length(this Point2d p, LineSegment2d l)
        //{
        //    return Math.Abs(p.Dpl(l));
        //}

        ///// <summary>
        ///// 166-1.点到直线长度绝对值
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="le"></param>
        ///// <returns></returns>
        //internal static double Dpl_Length(this Point2d p, LineEquation le)
        //{
        //    return Math.Abs(p.Dpl(le));
        //}

        ///// <summary>
        ///// 166.点到两点长度绝对值
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns></returns>
        //internal static double Dppp_Length(this Point2d p, Point2d p1, Point2d p2)
        //{
        //    return Math.Abs(p.Dpl(new LineSegment2d(p1, p2)));
        //}

        ///// <summary>
        ///// 166.点到两点长度
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns></returns>
        //internal static double Dppp(this Point2d p, Point2d p1, Point2d p2)
        //{
        //    return p.Dpl(new LineSegment2d(p1, p2));
        //}

        ///// <summary>
        ///// 172.直线与X轴夹角
        ///// </summary>
        ///// <param name="l"></param>
        ///// <returns>0 《 a 《 2pi</returns>
        //internal static double Alx_2(this LineSegment2d l)
        //{
        //    return l.GetLe().Alx_2();
        //}

        ///// <summary>
        ///// 173.直线与X轴夹角
        ///// </summary>
        ///// <param name="l"></param>
        ///// <returns>-pi 《 a 《 pi</returns>
        //internal static double Alx(this LineSegment2d l)
        //{
        //    return l.GetLe().Alx();
        //}

        ///// <summary>
        ///// 173.直线与X轴夹角
        ///// </summary>
        ///// <param name="le"></param>
        ///// <returns>0 《 a 《 2pi</returns>
        //internal static double Alx_2(this LineEquation le)
        //{
        //    double a = le.Alx();
        //    if (a < 0)
        //        a += Math.PI * 2;
        //    if (Math.Abs(a - Math.PI * 2) <= Eps)
        //        a = 0;
        //    return a;
        //}

        ///// <summary>
        ///// 173.直线与X轴夹角
        ///// </summary>
        ///// <param name="le"></param>
        ///// <returns>-pi 《 a 《 pi</returns>
        //internal static double Alx(this LineEquation le)
        //{
        //    double q;
        //    if (Math.Abs(le.B) <= Eps)
        //        q = Sign(Math.PI / 2, le.A);
        //    else
        //    {
        //        q = Math.Abs(le.A / le.B);
        //        q = Math.Atan(q);
        //        if (le.B > 0)
        //            q = Math.PI - q;
        //        q = Sign(q, le.A);
        //    }
        //    return q;
        //}

        ///// <summary>
        ///// 172-1.两点与X轴夹角
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns>0 《 a 《 2pi</returns>
        //internal static double Appx_2(this Point2d p1, Point2d p2)
        //{
        //    return new LineSegment2d(p1, p2).Alx_2();
        //}

        ///// <summary>
        ///// 173-1.两点与X轴夹角
        ///// </summary>
        ///// <param name="p1"></param>
        ///// <param name="p2"></param>
        ///// <returns>-pi 《 a 《 pi</returns>
        //internal static double Appx(this Point2d p1, Point2d p2)
        //{
        //    return new LineSegment2d(p2, p1).Alx();
        //}

        ///// <summary>
        ///// 174.向量与X轴夹角
        ///// </summary>
        ///// <param name="v"></param>
        ///// <returns>0 《 a 《 2pi</returns>
        //internal static double Avx_2(this Vector2d v)
        //{
        //    return v.Avv_2(Vector2d.XAxis);
        //}

        ///// <summary>
        ///// 175.向量与X轴夹角
        ///// </summary>
        ///// <param name="v"></param>
        ///// <returns>-pi 《 a 《 pi</returns>
        //internal static double Avx(this Vector2d v)
        //{
        //    return v.Avv(Vector2d.XAxis);
        //}

        ///// <summary>
        ///// 176.两直线夹角
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="l2"></param>
        ///// <returns>0 《 a 《 2pi</returns>
        //internal static double All_2(this LineSegment2d l1, LineSegment2d l2)
        //{
        //    double a = l1.All(l2);
        //    if (a < -Eps)
        //        a += Math.PI * 2;
        //    return a;
        //}

        ///// <summary>
        ///// 176.两直线夹角
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="l2"></param>
        ///// <returns>-pi 《 a 《 pi</returns>
        //internal static double All(this LineSegment2d l1, LineSegment2d l2)
        //{
        //    return l1.GetLe().All(l2.GetLe());
        //}

        ///// <summary>
        ///// 176.两直线夹角
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l2"></param>
        ///// <returns>0 《 a 《 2pi</returns>
        //internal static double Apl_2(this Point2d p, LineSegment2d l2)
        //{
        //    double a = p.Apl(l2);
        //    if (a < -Eps)
        //        a += Math.PI * 2;
        //    return a;
        //}

        ///// <summary>
        ///// 176.两直线夹角
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="l2"></param>
        ///// <returns>-pi 《 a 《 pi</returns>
        //internal static double Apl(this Point2d p, LineSegment2d l2)
        //{
        //    new LineSegment2d(l2.StartPoint, p).Lel(out LineEquation le1);
        //    l2.Lel(out LineEquation le2);
        //    return le1.All(le2);
        //}

        ///// <summary>
        ///// 176.两直线夹角
        ///// </summary>
        ///// <param name="le1"></param>
        ///// <param name="le2"></param>
        ///// <returns>-pi 《 a 《 pi</returns>
        //internal static double All(this LineEquation le1, LineEquation le2)
        //{
        //    double a = le1.Alx() - le2.Alx();
        //    double an = le2.A * le1.B - le1.A * le2.B;
        //    if (an * a < 0)
        //        a = Math.PI * 2 - Math.Abs(a);
        //    return Sign(a, an);
        //}

        ///// <summary>
        ///// 177.两向量夹角
        ///// </summary>
        ///// <param name="v1"></param>
        ///// <param name="v2"></param>
        ///// <returns>0 《 a 《 2pi</returns>
        //internal static double Avv_2(this Vector2d v1, Vector2d v2)
        //{
        //    double a = v1.Avv(v2);
        //    if (a < -Eps)
        //        a += Math.PI * 2;
        //    return a;
        //}

        ///// <summary>
        ///// 178.两向量夹角
        ///// </summary>
        ///// <param name="v1"></param>
        ///// <param name="v2"></param>
        ///// <returns>-pi 《 a 《 pi</returns>
        //internal static double Avv(this Vector2d v1, Vector2d v2)
        //{
        //    return v1.GetAngleTo(v2);
        //}

        ///// <summary>
        ///// 平行线距离
        ///// </summary>
        ///// <param name="le1"></param>
        ///// <param name="le2"></param>
        ///// <returns></returns>
        //internal static double Dll(this LineEquation le1, LineEquation le2)
        //{
        //    return le1.A != 0
        //        ? le1.B != 0
        //            ? Math.Abs(le1.C - le1.A / le2.A * le2.C) / (le1.A * le1.A + le1.B * le1.B)
        //            : Math.Abs(le1.C - le1.A / le2.A * le2.C) / le1.A * le1.A
        //        : le1.B != 0
        //            ? Math.Abs(le1.C - le1.B / le2.B * le2.C) / le1.B * le1.B
        //            : default;
        //}

        ///// <summary>
        ///// 平行线距离
        ///// </summary>
        ///// <param name="l1"></param>
        ///// <param name="l2"></param>
        ///// <returns></returns>
        //internal static double Dll(this LineSegment2d l1, LineSegment2d l2)
        //{
        //    return l1.GetLe().Dll(l2.GetLe());
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="angle"></param>
        ///// <returns></returns>
        //internal static AD ADA(this double angle)
        //{
        //    angle %= Math.PI * 2;
        //    angle += Math.PI * 2;
        //    angle %= Math.PI * 2;
        //    return Math.Abs(angle) <= Eps
        //        ? AD.First
        //        : Math.Abs(angle - Math.PI / 2) <= Eps
        //            ? AD.Second
        //            : Math.Abs(angle - Math.PI) <= Eps
        //                ? AD.Third
        //                : Math.Abs(angle - Math.PI * 3 / 2) <= Eps
        //                    ? AD.Fourth
        //                    : AD.False;
        //}

        //internal static Point2d Pppp(this Point2d pt, Point2d pt1, Point2d pt2)
        //{
        //    pt2.Lplp(new LineSegment2d(pt, pt1)).Pll(pt1.Lplp(new LineSegment2d(pt, pt2)), out Point2d pt3);
        //    return pt3;
        //}

        //internal static Point2d Plplp(LineSegment2d l1, LineSegment2d l2)
        //{
        //    return l1.StartPoint.Pppp(l1.EndPoint, l2.EndPoint);
        //}

        //#endregion

        //#endregion

        //#region AutoCAD

        //internal static Vector2d GetDelta(this LineSegment2d line)
        //{
        //    return line.EndPoint - line.StartPoint;
        //}

        #endregion
    }
}
#endregion