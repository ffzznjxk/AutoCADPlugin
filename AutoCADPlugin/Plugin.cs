using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCADPlugin
{
    /// <summary>
    /// 测试插件
    /// </summary>
    public partial class Plugin : IExtensionApplication
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            //Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //ed.WriteMessage("Hello world!");
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Terminate()
        {

        }

        /// <summary>
        /// 自动加载
        /// </summary>
        [CommandMethod("AutoLoad")]
        public void AutoLoad()
        {
            Load.AutoLoad("MyProgram", "CAD Plugin", @"G:\autocad\work\autocad plugin\autocad plugin\bin\Debug\autocad plugin.dll");
        }

        /// <summary>
        /// 欢迎
        /// </summary>
        [CommandMethod("Hello")]
        public void HelloWorld()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Hello world!");
        }

        /// <summary>
        /// 画线
        /// </summary>
        [CommandMethod("DrwaLine")]
        public void DrawLine()
        {
            //
            Point3d pt1 = new Point3d(100, 100, 0);
            Point3d pt2 = new Point3d(200, 200, 0);
            Line l = new Line(pt1, pt2);
            var pt3 = new Point3d(200, 100, 0);
            var pt4 = new Point3d(300, 200, 0);
            Line l2 = new Line(pt3, pt4);
            Database db = HostApplicationServices.WorkingDatabase;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                db.AddToCurrentSpace(l, l2);
                tr.Commit();
            }

        }

        /// <summary>
        /// 提示画线
        /// </summary>
        [CommandMethod("PDL")]
        public void PromptDrawLine()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions ppo = new PromptPointOptions("\n指定起点");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status == PromptStatus.OK)
            {
                Point3d ptStart = ppr.Value;
                ppo = new PromptPointOptions("\n指定终点");
                ppr = ed.GetPoint(ppo);
                if (ppr.Status == PromptStatus.OK)
                {
                    Point3d ptEnd = ppr.Value;
                    Line line = new Line(ptStart, ptEnd);

                    Database db = HostApplicationServices.WorkingDatabase;

                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        db.AddToCurrentSpace(line);
                        tr.Commit();
                    }

                }
            }
        }

        /// <summary>
        /// 画多条线段
        /// </summary>
        [CommandMethod("PDML")]
        public void PromptDrawMultLine()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions ppo = new PromptPointOptions("\n指定起点");
            PromptPointResult ppr = ed.GetPoint(ppo);

            if (ppr.Status == PromptStatus.OK)
            {
                Point3d ptStart = ppr.Value;
                ppo = new PromptPointOptions("\n指定终点");
                ppr = ed.GetPoint(ppo);

                while (ppr.Status == PromptStatus.OK)
                {
                    Point3d ptEnd = ppr.Value;
                    Line line = new Line(ptStart, ptEnd);

                    Database db = HostApplicationServices.WorkingDatabase;

                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        db.AddToCurrentSpace(line);
                        tr.Commit();
                    }

                    ptStart = ptEnd;
                    ppo = new PromptPointOptions("\n指定下一点");
                    ppr = ed.GetPoint(ppo);
                }
            }
        }

        /// <summary>
        /// 画多线段
        /// </summary>
        [CommandMethod("PDPL")]
        public void PromptDrawPolyLine()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions ppo = new PromptPointOptions("\n指定起点");
            PromptPointResult ppr = ed.GetPoint(ppo);

            List<Point3d> pts = new List<Point3d>();
            while (ppr.Status == PromptStatus.OK)
            {
                Point3d pt = ppr.Value;
                pts.Add(pt);
                ppo = new PromptPointOptions("\n指定下一点");
                ppr = ed.GetPoint(ppo);
            }
            if (pts.Count > 1)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Polyline pl = new Polyline();
                var pts2d = pts.Select(d => new Point2d(d.X, d.Y)).ToArray();
                pl.CreatePolyline(pts2d);
                pl.Closed = true;
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    db.AddToCurrentSpace(pl);
                    tr.Commit();
                }

            }
            else
                ed.WriteMessage("\n输入点过少。");
        }

        /// <summary>
        /// 画平行四边形
        /// </summary>
        [CommandMethod("DPL")]
        public void DrawParallelogram()
        {

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            var peo = new PromptEntityOptions("\n选择一条直线");
            peo.SetRejectMessage("选择直线");
            peo.AddAllowedClass(typeof(Line), true);

            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status == PromptStatus.OK)
            {
                var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
                if (ppr.Status == PromptStatus.OK)
                {
                    var pt3 = ppr.Value;
                    var id = per.ObjectId;
                    Database db = HostApplicationServices.WorkingDatabase;
                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var ent = (Entity)id.GetObject(OpenMode.ForRead);
                        Line line = (Line)ent;
                        var vector = line.Delta;

                        var pt4 = pt3 - vector;

                        List<Point3d> pts = new List<Point3d>()
                        {
                            line.StartPoint, line.EndPoint, pt3, pt4
                        };

                        Polyline pl = new Polyline();
                        var pts2d = pts.Select(d => new Point2d(d.X, d.Y)).ToArray();

                        pl.CreatePolyline(pts2d);
                        pl.Closed = true;
                        db.AddToCurrentSpace(pl);

                        line.UpgradeOpen();
                        line.Erase();

                        tr.Commit();
                    }
                }

            }
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        [CommandMethod("DREC")]
        public void DrawRectangle()
        {
            Circle c = new Circle();
            c.CreateCircle(new Point3d(), new Point3d());

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions ppo = new PromptPointOptions("\n指定起点");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status == PromptStatus.OK)
            {
                Point3d ptStart = ppr.Value;
                var pcr = ed.GetCorner("\n指定角点", ptStart);
                if (pcr.Status == PromptStatus.OK)
                {
                    Point3d ptEnd = pcr.Value;

                    var maxX = Math.Max(ptStart.X, ptEnd.X);
                    var maxY = Math.Max(ptStart.Y, ptEnd.Y);
                    var minX = Math.Min(ptStart.X, ptEnd.X);
                    var minY = Math.Min(ptStart.Y, ptEnd.Y);
                    var pt1 = new Point2d(minX, minY);
                    var pt2 = new Point2d(maxX, minY);
                    var pt3 = new Point2d(maxX, maxY);
                    var pt4 = new Point2d(minX, maxY);

                    List<Point2d> pts = new List<Point2d>()
                    {
                        pt1, pt2, pt3, pt4
                    };

                    Database db = HostApplicationServices.WorkingDatabase;

                    using (var tr = db.TransactionManager.StartTransaction())
                    {
                        var pl = new Polyline();
                        pl.CreatePolyline(pts.ToArray());
                        pl.Closed = true;
                        db.AddToCurrentSpace(pl);
                        tr.Commit();
                    }

                }
            }
        }

        /// <summary>
        /// 绘制正多边形
        /// </summary>
        [CommandMethod("DRP")]
        public void DrawRegularPolygon()
        {

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var ppr = ed.GetPoint("\n指定圆心");
            if (ppr.Status == PromptStatus.OK)
            {
                var ptCen = ppr.Value;
                ppr = ed.GetPoint("\n指定角点");
                if (ppr.Status == PromptStatus.OK)
                {
                    var pt1 = ppr.Value;

                    var distance = pt1.DistanceTo(ptCen);
                    if (distance == 0) return;

                    var pir = ed.GetInteger("\n指定边数");
                    if (pir.Status == PromptStatus.OK)
                    {
                        int sideNum = pir.Value;

                        if (sideNum < 3) return;

                        var angle = (pt1 - ptCen).GetAngleTo(Vector3d.XAxis);

                        if (pt1.Y < ptCen.Y)
                            angle = -angle;

                        List<Point2d> pts = new List<Point2d>();

                        for (int i = 0; i < sideNum; i++)
                        {
                            var vector = new Vector2d(Math.Cos(angle), Math.Sin(angle)) * distance;
                            pts.Add(new Point2d(ptCen.X, ptCen.Y) + vector);
                            angle += Math.PI * 2 / sideNum;
                        }

                        Database db = HostApplicationServices.WorkingDatabase;

                        Polyline pl = new Polyline();
                        pl.CreatePolyline(pts.ToArray());
                        pl.Closed = true;
                        using (var tr = db.TransactionManager.StartTransaction())
                        {
                            db.AddToCurrentSpace(pl);
                            tr.Commit();
                        }
                    }
                }
            }
        }
    }
}
