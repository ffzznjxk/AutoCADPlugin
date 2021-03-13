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
    /// <summary>
    /// 测试使用DNA
    /// </summary>
    public class UsingDNA
    {
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

    }
}
