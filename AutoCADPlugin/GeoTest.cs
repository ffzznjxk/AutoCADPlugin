using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetARX;
using Autodesk.AutoCAD.EditorInput;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCADPlugin
{
    public class GeoTest
    {

        [CommandMethod("DrawArrow")]
        public void DrawArrow()
        {

            Polyline pl = MakeArrow();

            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
            if (ppr.Status == PromptStatus.OK)
            {
                Point3d pt = ppr.Value;

                var pao = new PromptAngleOptions("\n请输入一个角度");
                pao.BasePoint = pt;
                pao.UseBasePoint = true;
                var par = ed.GetAngle(pao);
                if (par.Status == PromptStatus.OK)
                {
                    var angle = par.Value;

                    var pdr = ed.GetDouble(new PromptDoubleOptions("\n请输入比例:"));
                    if (pdr.Status == PromptStatus.OK)
                    {
                        var scale = pdr.Value;

                        var pko = new PromptKeywordOptions("\n是否反向");
                        pko.Keywords.Add("正向", "Y", "正向(Y)");
                        pko.Keywords.Add("反向", "N", "反向(N)");
                        pko.Keywords.Default = "正向";
                        var pkr = ed.GetKeywords(pko);

                        var mt = ed.CurrentUserCoordinateSystem;

                        if (pkr.Status == PromptStatus.OK)
                        {
                            if (pkr.StringResult == "反向")
                            {
                                var v = Vector3d.XAxis;
                                var currVector = v.RotateBy(angle + Math.PI / 2, Vector3d.ZAxis);
                                var l3d = new Line3d(Point3d.Origin, currVector);
                                mt = Matrix3d.Displacement(pt.GetAsVector())
                                    * Matrix3d.Scaling(scale, Point3d.Origin)
                                    * Matrix3d.Mirroring(l3d)
                                    * Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);

                            }
                            else
                                mt = Matrix3d.Displacement(pt.GetAsVector())
                                    * Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin)
                                    * Matrix3d.Scaling(scale, Point3d.Origin);
                        }
                        pl.TransformBy(mt);

                        Database db = HostApplicationServices.WorkingDatabase;

                        using (var tr = db.TransactionManager.StartTransaction())
                        {
                            var id = db.AddToCurrentSpace(pl);
                            tr.Commit();
                        }
                    }
                }
            }
        }


        [CommandMethod("CutawayViewTest")]
        public void CutawayViewTest()
        {

            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            var peo = new PromptEntityOptions("\n选择一条剖切线");
            peo.SetRejectMessage("选择直线");
            peo.AddAllowedClass(typeof(Line), true);
            var per = ed.GetEntity(peo);
            if (per.Status == PromptStatus.OK)
            {
                Database db = HostApplicationServices.WorkingDatabase;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var line = (Line)per.ObjectId.GetObject(OpenMode.ForRead);

                    var pso = new PromptSelectionOptions();
                    pso.MessageForAdding = "\n选择剖切对象";
                    var psr = ed.GetSelection(pso);
                    if (psr.Status == PromptStatus.OK)
                    {

                        List<ObjectId> ids = new List<ObjectId>();
                        foreach (var id in psr.Value.GetObjectIds())
                        {
                            var ent = (Entity)id.GetObject(OpenMode.ForRead);
                            if (ent is Curve c)
                            {
                                var plane = new Plane(line.StartPoint, line.Delta.RotateBy(Math.PI / 2, Vector3d.ZAxis));
                                var projectResult = c.GetOrthoProjectedCurve(plane);
                                if (projectResult != null)
                                {
                                    projectResult.ColorIndex = 1;
                                    ids.Add(db.AddToCurrentSpace(projectResult));
                                }
                            }
                        }
                        if (ids.Count > 0)
                        {

                            var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
                            if (ppr.Status == PromptStatus.OK)
                            {
                                var pt = ppr.Value;
                                ids.Add(line.ObjectId);
                                var copyIds = db.Copy(line.StartPoint, pt, ids.ToArray());
                                var mt = Matrix3d.Rotation(-line.Angle, Vector3d.ZAxis, pt);
                                foreach (var cid in copyIds)
                                {
                                    var ent = (Entity)cid.Value.GetObject(OpenMode.ForWrite);
                                    ent.TransformBy(mt);
                                    ent.DowngradeOpen();
                                }
                            }

                        }
                    }
                    tr.Commit();
                }
            }
        }


        private static Polyline MakeArrow()
        {
            var pt1 = new Point2d(0, 1);
            var pt2 = new Point2d(4, 0);
            var pt3 = new Point2d(0, -1);

            var pl = new Polyline();
            pl.CreatePolyline(pt1, pt2, pt3);
            pl.Closed = true;
            return pl;
        }
    }
}
