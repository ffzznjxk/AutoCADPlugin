using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static AutoCADPlugin.Args;
using static AutoCADPlugin.Tools;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCADPlugin
{
    /// <summary>
    /// 测试插件
    /// </summary>
    public partial class Command : IExtensionApplication
    {
        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {

            var docs = AcadApp.DocumentManager;
            docs.DocumentActivated += Docs_DocumentActivated;
        }

        private void Docs_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            Ribbon.AddRibbon();
            var docs = AcadApp.DocumentManager;
            docs.DocumentActivated -= Docs_DocumentActivated;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Terminate()
        {

        }

        #endregion

        #region 相关参数


        #endregion

        #region 命令

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

        /// <summary>
        /// 添加实体
        /// </summary>
        [CommandMethod("EntityTools")]
        public void EntityTools()
        {

            Database db = HostApplicationServices.WorkingDatabase;

            Line l = new Line(Point3d.Origin, new Point3d(100, 100, 0));
            Circle c = new Circle();
            c.CreateCircle(new Point3d(50, 50, 0), new Point3d(50, 100, 0));

            ObjectIdList ids = new ObjectIdList();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                ////添加到模型空间
                //ids = db.AddToModelSpace(l, c);
                ////添加到布局空间
                //ids = db.AddToPaperSpace(l, c);
                //添加到当前空间
                ids = db.AddToCurrentSpace(l, c);
                //高亮
                ids.HighlightEntities();
                //ids.UnHighlightEntities();
                tr.Commit();
            }
            if (ids.Count > 0)
            {

                Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    //选择一条直线
                    var peo = new PromptEntityOptions("\n选择一条直线");
                    peo.SetRejectMessage("选择直线");
                    peo.AddAllowedClass(typeof(Line), true);
                    var per = ed.GetEntity(peo);
                    if (per.Status == PromptStatus.OK)
                    {
                        var line = (Line)per.ObjectId.GetObject(OpenMode.ForRead);
                        //移动
                        //ids.Move(line.StartPoint, line.EndPoint);
                        //拷贝
                        //var copyIds = db.Copy(line.EndPoint, line.StartPoint, ids);
                        //foreach (var cid in copyIds)
                        //{
                        //    var ent = (Entity)cid.Value.GetObject(OpenMode.ForWrite);
                        //    ent.ColorIndex = 1;
                        //    ent.DowngradeOpen();
                        //}
                        //旋转
                        //ids.Rotate(line.StartPoint, line.Angle);
                        //缩放
                        //ids.Scale(line.StartPoint, 2);
                        //镜像
                        EntTools.Mirror(line.EndPoint, line.StartPoint, false, ids.ToArray());
                    }
                    tr.Commit();
                }
            }
        }

        /// <summary>
        /// 添加文字
        /// </summary>
        [CommandMethod("PLT")]
        public void PolylineHatch()
        {

            Database db = HostApplicationServices.WorkingDatabase;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                //Polyline pl = new Polyline();
                ////pl.CreatePolyline(Point2d.Origin, new Point2d(0, 50), new Point2d(50, 50));
                ////pl.Closed = true;
                ////pl.CreateRectangle(Point2d.Origin, new Point2d(50, 50));
                //pl.CreatePolygon(Point2d.Origin, 6, 50);
                //db.AddToCurrentSpace(pl);

                var tsId = db.AddTextStyle("test", "sceie.shx", "sceic.shx", 0.7);
                if (tsId != ObjectId.Null)
                {

                    DBText text = new DBText()
                    {
                        TextStyleId = tsId,
                        //Position = new Point3d(25, 25, 0),
                        Height = 20,
                        TextString = "文字",
                        HorizontalMode = TextHorizontalMode.TextCenter,
                        VerticalMode = TextVerticalMode.TextVerticalMid,
                        AlignmentPoint = new Point3d(25, 25, 0),
                    };
                    db.AddToCurrentSpace(text);
                }
                else
                    AcadApp.ShowAlertDialog("字体样式添加失败。");
                tr.Commit();
            }
        }

        /// <summary>
        /// 插入图块
        /// </summary>
        [CommandMethod("InsertBlock")]
        public void InsertBlock()
        {

            Database db = HostApplicationServices.WorkingDatabase;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                //块定义
                if (!AddIndexBtr())
                {
                    AcadApp.ShowAlertDialog($"{indexBlockName} 创建块定义失败。");
                    return;
                }
                Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
                //插入点
                var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
                if (ppr.Status != PromptStatus.OK) return;
                //序号值
                var pir = ed.GetInteger(new PromptIntegerOptions("\n请输入序号"));
                if (pir.Status != PromptStatus.OK) return;
                //属性
                Dictionary<string, string> atts = new Dictionary<string, string>
                {
                    { "编号", pir.Value.ToString() }
                };
                //添加图层
                db.AddLayer(indexLayer, "标注图层");
                //插入块
                var brefId = db.CurrentSpaceId.InsertBlockReference(indexLayer, indexBlockName,
                    ppr.Value, new Scale3d(1), 0, atts);

                if (brefId == ObjectId.Null)
                    AcadApp.ShowAlertDialog($"{indexBlockName} 块插入失败。");
                tr.Commit();
            }
        }

        /// <summary>
        /// 序号加1
        /// </summary>
        [CommandMethod("PlusIndex")]
        public void PlusIndex()
        {

            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            var peo = new PromptEntityOptions("\n选择一个编号");
            peo.SetRejectMessage("选择块");
            peo.AddAllowedClass(typeof(BlockReference), true);
            var per = ed.GetEntity(peo);
            if (per.Status == PromptStatus.OK)
            {

                Database db = HostApplicationServices.WorkingDatabase;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var indexBlock = (BlockReference)per.ObjectId.GetObject(OpenMode.ForRead);

                    string currIndex = indexBlock.ObjectId.GetAttributeInBlockReference("编号");

                    try
                    {
                        indexBlock.ObjectId.UpdateAttributesInBlock("编号",
                            (Convert.ToInt32(currIndex) + 1).ToString());
                    }
                    catch (System.Exception)
                    {
                        AcadApp.ShowAlertDialog("\n当前内容不支持序号加 1");
                    }
                    tr.Commit();
                }

            }

        }

        /// <summary>
        /// 切换车类型
        /// </summary>
        [CommandMethod("ChangeCarType")]
        public void ChangeCarType()
        {

            Database db = HostApplicationServices.WorkingDatabase;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                List<BlockReference> cars = db.GetSelectionOfBlockRefs("\n选择车", false, carBlockName);

                if (cars.Count == 0)
                {
                    AcadApp.ShowAlertDialog("选中数量为0");
                }
                else
                {
                    foreach (var car in cars)
                    {
                        var currCarType = car.ObjectId.GetDynBlockValue("类型");

                        switch (currCarType)
                        {
                            case "微型车位":
                                car.ObjectId.SetDynBlockValue("类型", "普通车位");
                                break;
                            case "普通车位":
                                car.ObjectId.SetDynBlockValue("类型", "微型车位");
                                break;
                        }
                    }
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// 车位排序
        /// </summary>
        [CommandMethod("OrderCar")]
        public void OrderCar()
        {
            OrderCarBlock();
        }

        /// <summary>
        /// 创建表格
        /// </summary>
        [CommandMethod("CreateTable")]
        public void CreateTable()
        {
            AddCarTable();
        }

        /// <summary>
        /// 导入块，添加块
        /// </summary>
        [CommandMethod("ImportBlock")]
        public void ImportBlock()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), fileName);
            AddCarBlock(filePath, "微型车位", 0);
        }

        /// <summary>
        /// 添加车
        /// </summary>
        [CommandMethod("AddCar")]
        public void AddCar()
        {
            AcadApp.ShowModalDialog(new CarTool());
        }

        public static PaletteSet ps = null;

        /// <summary>
        /// 命令面板集
        /// </summary>
        [CommandMethod("Cmds")]
        public void Cmds()
        {
            if (ps == null)
            {
                ps = new PaletteSet("我的工具");
                ps.Visible = true;
                ps.Dock = DockSides.Left;
                ps.MinimumSize = new Size(200, 500);

                var dicCmds = new Dictionary<string, Dictionary<Bitmap, List<string>>>
                {
                    { "命令集", new Dictionary<Bitmap, List<string>>
                        {
                            { Properties.Resources.AddCar, new List<string>{ "添加车位", "AddCar"} },
                            { Properties.Resources.OrderCar, new List<string>{ "车位排序", "OrderCar"} },
                            { Properties.Resources.CreateTable, new List<string>{ "统计车位", "CreateTable"} },
                        }
                    },
                    {
                        "命令集1", new Dictionary<Bitmap, List<string>>
                        {
                            { Properties.Resources.AddCar, new List<string>{ "添加车位", "AddCar"} },
                            { Properties.Resources.OrderCar, new List<string>{ "车位排序", "OrderCar"} },
                        }
                    }
                };

                foreach (var cmd in dicCmds)
                    ps.Add(cmd.Key, new PaletteCmds(cmd.Value));
            }
            else
                ps.Visible = !ps.Visible;

            //PaletteSet psMyTool = null;
            //if (psMyTool == null)
            //{
            //    psMyTool = new PaletteSet("我的工具")
            //    {
            //        Visible = true,
            //        Dock = DockSides.Left,
            //        MinimumSize = new Size(200, 500)
            //    };

            //    var dicCmds = new Dictionary<string, Dictionary<Bitmap, List<string>>>();

            //    var dicInfos = new Dictionary<Bitmap, List<string>>
            //    {
            //        { Properties.Resources.AddCar,
            //            new List<string> { "添加车位", "AddCar"}},
            //        { Properties.Resources.OrderCar,
            //            new List<string> { "车位排序", "OrderCar"}},
            //        { Properties.Resources.CreateTable,
            //            new List<string> { "统计车位", "CreateTable"}},
            //    };
            //    dicCmds.Add("信息类", dicInfos);
            //    foreach (var cmds in dicCmds)
            //        psMyTool.Add(cmds.Key, new PaletteInfomation(cmds.Value));
            //}
            //else
            //    psMyTool.Visible = !psMyTool.Visible;
        }

        /// <summary>
        /// 添加网格
        /// </summary>
        [CommandMethod("CarGird")]
        public void CarGird()
        {
            var pl = DrawRandomQuadrilateral();
            if (pl != null)
            {
                var ls3ds = new List<LineSegment3d>();
                for (int i = 0; i < pl.NumberOfVertices; i++)
                    ls3ds.Add(pl.GetLineSegmentAt(i));
                ls3ds = ls3ds.OrderBy(d => d.MidPoint.Y).ToList();
                double length = 100;
                var pt1 = Point2d.Origin;
                var pt3 = (pt1 + Vector2d.XAxis - Vector2d.YAxis) * length;
                Polyline grid = new Polyline();
                grid.CreateRectangle(pt1, pt3);

                var ls3d = ls3ds.FirstOrDefault();

                var pts = new List<Point3d> { ls3d.StartPoint, ls3d.EndPoint };
                var pt = new Point3d(pts.Min(d => d.X), pts.Min(d => d.Y), 0);

                var l3d = new Line3d(pt, Vector3d.XAxis);
                var pl3d = pl.GetGeCurve();

                //var inter = pl3d.();
            }
        }

        private static Polyline DrawRandomQuadrilateral()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
            if (ppr.Status == PromptStatus.OK)
            {
                var startPt = ppr.Value;

                ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
                if (ppr.Status == PromptStatus.OK)
                {
                    var endPt = ppr.Value;

                    //var ls3d = new LineSegment3d(startPt, endPt);

                    Random rd = new Random();

                    var v1 = startPt.GetVectorTo(endPt).OrthoProjectTo(Vector3d.YAxis);

                    var value1 = Convert.ToDouble(rd.Next(100));
                    var value2 = Convert.ToDouble(rd.Next(100));

                    var pt2 = startPt + v1 * ( 1 + value1 / 100);
                    var pt4 = endPt - v1 * value2 / 100;

                    Polyline pl = new Polyline();
                    pl.CreatePolyline(new Point3dCollection { startPt, pt2, endPt, pt4 });
                    pl.Closed = true;
                    return pl;
                }
            }
            return null;
        }

        #endregion
    }
}
