using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADPlugin
{
    public class Plugin
    {
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

                Polyline pl = new Polyline();
                for (int i = 0; i < pts.Count; i++)
                {
                    Point3d pt = pts[i];
                    pl.AddVertexAt(i, new Point2d(pt.X, pt.Y), 0, 0, 0);
                }
                pl.Closed = true;

                Database db = HostApplicationServices.WorkingDatabase;
                var id = EntityTools.AddNewEntity(db, pl);
            }
            else
            {
                ed.WriteMessage("\n输入点过少。");
            }
        }
    }
}
