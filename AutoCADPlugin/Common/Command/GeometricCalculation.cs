using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCADPlugin
{
    /// <summary>
    /// 集合测试
    /// </summary>
    public class GeometricCalculation
    {
        /// <summary>
        /// 绘制箭头
        /// </summary>
        [CommandMethod("DrawArrow")]
        public void DrawArrow()
        {
            //箭头图形
            Polyline pl = MakeArrow();

            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            //插入点
            var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
            if (ppr.Status == PromptStatus.OK)
            {
                Point3d pt = ppr.Value;
                
                //插入角度
                var pao = new PromptAngleOptions("\n请输入一个角度");
                pao.BasePoint = pt;
                pao.UseBasePoint = true;
                var par = ed.GetAngle(pao);
                if (par.Status == PromptStatus.OK)
                {
                    var angle = par.Value;

                    //缩放比例
                    var pdr = ed.GetDouble(new PromptDoubleOptions("\n请输入比例:"));
                    if (pdr.Status == PromptStatus.OK)
                    {
                        var scale = pdr.Value;

                        //是否镜像
                        var pko = new PromptKeywordOptions("\n是否反向");
                        pko.Keywords.Add("正向", "Y", "正向(Y)");
                        pko.Keywords.Add("反向", "N", "反向(N)");
                        pko.Keywords.Default = "正向";
                        var pkr = ed.GetKeywords(pko);

                        //旋转、缩放、平移矩阵
                        var mt = Matrix3d.Displacement(pt.GetAsVector())
                            * Matrix3d.Scaling(scale, Point3d.Origin)
                            * Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);

                        if (pkr.Status == PromptStatus.OK)
                        {
                            if (pkr.StringResult == "反向")
                            {
                                var v = Vector3d.XAxis.RotateBy(angle + Math.PI / 2, Vector3d.ZAxis);
                                var l3d = new Line3d(Point3d.Origin, v);
                                //旋转、进行、缩放、平移矩阵
                                mt = Matrix3d.Displacement(pt.GetAsVector())
                                    * Matrix3d.Scaling(scale, Point3d.Origin)
                                    * Matrix3d.Mirroring(l3d)
                                    * Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);
                            }
                        }
                        //几何变换
                        pl.TransformBy(mt);

                        Database db = HostApplicationServices.WorkingDatabase;
                        //添加图形
                        using (var tr = db.TransactionManager.StartTransaction())
                        {
                            var id = db.AddToCurrentSpace(pl);
                            tr.Commit();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 剖立面测试
        /// </summary>
        [CommandMethod("CutawayViewTest")]
        public void CutawayViewTest()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            //选择剖切线
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
                    //选择剖切图形
                    var pso = new PromptSelectionOptions();
                    pso.MessageForAdding = "\n选择剖切对象";
                    var psr = ed.GetSelection(pso);
                    if (psr.Status == PromptStatus.OK)
                    {
                        //投影对象
                        List<ObjectId> ids = new List<ObjectId>();
                        foreach (var id in psr.Value.GetObjectIds())
                        {
                            var ent = (Entity)id.GetObject(OpenMode.ForRead);
                            if (ent is Curve c)
                            {
                                //剖切线转平面
                                var plane = new Plane(line.StartPoint, line.Delta.RotateBy(Math.PI / 2, Vector3d.ZAxis));
                                //投影图形
                                var curveResult = c.GetOrthoProjectedCurve(plane);
                                if (curveResult != null)
                                {
                                    //变色(红)
                                    curveResult.ColorIndex = 1;
                                    ids.Add(db.AddToCurrentSpace(curveResult));
                                }
                            }
                        }
                        if (ids.Count > 0)
                        {
                            //剖立面插入点
                            var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
                            if (ppr.Status == PromptStatus.OK)
                            {
                                var pt = ppr.Value;
                                //添加原线
                                ids.Add(line.ObjectId);
                                //复制剖立面内容
                                var copyIds = db.Copy(line.StartPoint, pt, ids.ToArray());
                                //旋转矩阵（水平）
                                var mt = Matrix3d.Rotation(-line.Angle, Vector3d.ZAxis, pt);
                                //几何变换
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

        /// <summary>
        /// 箭头图形
        /// </summary>
        /// <returns></returns>
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
