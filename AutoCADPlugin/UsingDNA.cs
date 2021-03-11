using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static AutoCADPlugin.Args;
using static AutoCADPlugin.Tools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCADPlugin
{
    public class UsingDNA
    {

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
                //ids = db.AddToModelSpace(l, c);
                //ids = db.AddToPaperSpace(l, c);
                
                ids = db.AddToCurrentSpace(l, c);
                ids.HighlightEntities();
                //ids.UnHighlightEntities();
                tr.Commit();
            }
            if (ids.Count > 0)
            {

                Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

                using (var tr = db.TransactionManager.StartTransaction())
                {

                    var peo = new PromptEntityOptions("\n选择一条直线");
                    peo.SetRejectMessage("选择直线");
                    peo.AddAllowedClass(typeof(Line), true);
                    var per = ed.GetEntity(peo);
                    if (per.Status == PromptStatus.OK)
                    {
                        var line = (Line)per.ObjectId.GetObject(OpenMode.ForRead);

                        //ids.Move(line.StartPoint, line.EndPoint);
                        //var copyIds = db.Copy(line.EndPoint, line.StartPoint, ids);
                        //foreach (var cid in copyIds)
                        //{
                        //    var ent = (Entity)cid.Value.GetObject(OpenMode.ForWrite);
                        //    ent.ColorIndex = 1;
                        //    ent.DowngradeOpen();
                        //}
                        //ids.Rotate(line.StartPoint, line.Angle);
                        //ids.Scale(line.StartPoint, 2);
                        EntTools.Mirror(line.EndPoint, line.StartPoint, false, ids.ToArray());
                    }
                    tr.Commit();
                }
            }

        }


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

                var tsId = db.AddTextStyle("test", "sceic.shx", "sceic.shx", 0.7);
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


        [CommandMethod("InsertBlock")]
        public void InsertBlock()
        {

            Database db = HostApplicationServices.WorkingDatabase;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                if (AddIndexBtr())
                {
                    Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
                    var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
                    if (ppr.Status == PromptStatus.OK)
                    {
                        var pt = ppr.Value;

                        var pir = ed.GetInteger(new PromptIntegerOptions("\n请输入序号"));
                        if (pir.Status == PromptStatus.OK)
                        {

                            Dictionary<string, string> atts = new Dictionary<string, string>
                            {
                                { "编号", pir.Value.ToString() }
                            };

                            db.AddLayer(indexLayer, "标注图层");

                            var brefId = db.CurrentSpaceId.InsertBlockReference(indexLayer, indexBlockName, pt,
                                new Scale3d(1), 0, atts);

                            if (brefId == ObjectId.Null)
                                AcadApp.ShowAlertDialog($"{indexBlockName} 块插入失败。");
                        }
                    }
                }
                else
                    AcadApp.ShowAlertDialog($"{indexBlockName} 创建块定义失败。");
                tr.Commit();
            }
        }


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


        [CommandMethod("OrderCar")]
        public void OrderCar()
        {
            OrderCarBlock();
        }

        [CommandMethod("CreateTable")]
        public void CreateTable()
        {
            AddCarTable();
        }

        [CommandMethod("ImportBlock")]
        public void ImportBlock()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), fileName);
            AddCarBlock(filePath, "微型车位", 0);
        }


        [CommandMethod("AddCar")]
        public void AddCar()
        {
            AcadApp.ShowModalDialog(new CarTool());
        }

    }
}
