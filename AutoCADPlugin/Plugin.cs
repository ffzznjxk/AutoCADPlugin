using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;

namespace AutoCADPlugin
{
    public partial class Plugin
    {
        //public void Initialize()
        //{
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    ed.WriteMessage("Hello world!");
        //}

        //public void Terminate()
        //{
        //    throw new NotImplementedException();
        //}


        [CommandMethod("AutoLoad")]
        public void AutoLoad()
        {
            Load.AutoLoad("MyProgram", "CAD Plugin", @"G:\autocad\work\autocad plugin\autocad plugin\bin\Debug\autocad plugin.dll");
        }


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
            var ids = EntityTools.AddNewEntity(db, l, l2);
        }


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
                    var id = EntityTools.AddNewEntity(db, line);
                }
            }
        }

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
                    var id = EntityTools.AddNewEntity(db, line);

                    ptStart = ptEnd;
                    ppo = new PromptPointOptions("\n指定下一点");
                    ppr = ed.GetPoint(ppo);
                }
            }
        }

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
                db.AddNewPolyLine(pts);
            }
            else
            {
                ed.WriteMessage("\n输入点过少。");
            }
        }

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
                        //var vector = line.EndPoint - line.StartPoint;
                        var vector = line.Delta;

                        var pt4 = pt3 - vector;

                        List<Point3d> pts = new List<Point3d>()
                        {
                            line.StartPoint, line.EndPoint, pt3, pt4
                        };

                        db.AddNewPolyLine(pts);

                        line.UpgradeOpen();
                        line.Erase();

                        tr.Commit();
                    }
                }

            }
        }

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
                    var pt1 = new Point3d(minX, minY, 0);
                    var pt2 = new Point3d(maxX, minY, 0);
                    var pt3 = new Point3d(maxX, maxY, 0);
                    var pt4 = new Point3d(minX, maxY, 0);

                    List<Point3d> pts = new List<Point3d>()
                    {
                        pt1, pt2, pt3, pt4
                    };

                    Database db = HostApplicationServices.WorkingDatabase;

                    db.AddNewPolyLine(pts);

                }
            }
        }


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

                        List<Point3d> pts = new List<Point3d>();

                        for (int i = 0; i < sideNum; i++)
                        {
                            var vector = new Vector3d(Math.Cos(angle) * distance, Math.Sin(angle) * distance, 0);

                            var ptNext = ptCen + vector;
                            pts.Add(ptNext);
                            angle += Math.PI * 2 / sideNum;
                        }

                        Database db = HostApplicationServices.WorkingDatabase;

                        db.AddNewPolyLine(pts);
                    }
                }
            }

        }
    }
}
