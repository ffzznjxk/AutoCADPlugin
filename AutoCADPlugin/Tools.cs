using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AutoCADPlugin.Args;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCADPlugin
{
    /// <summary>
    /// 工具集合
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// 添加车位块
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="carType">车位类型</param>
        /// <param name="angle">角度</param>
        public static void AddCarBlock(string filePath, string carType, double angle)
        {

            if (!File.Exists(filePath))
            {
                AcadApp.ShowAlertDialog($"{filePath} 文件不存在。");
                return;
            }

            //导入图形
            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                bool success = db.ImportBlockFromDwg(filePath, carBlockName, DuplicateRecordCloning.Replace);
                if (success)
                {
                    //设置图层
                    db.AddLayer(carLayer, "车位图层");
                    db.SetLayerColor(carLayer, carColorIndex);
                    tr.Commit();
                }
                else
                    return;
            }

            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
            //插入点
            var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
            while (ppr.Status == PromptStatus.OK)
            {
                var pt = ppr.Value;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    //插入图形
                    var id = db.CurrentSpaceId.InsertBlockReference(carLayer, carBlockName, pt, new Scale3d(1), angle);
                    //设置车位类型
                    if (id != ObjectId.Null)
                    {
                        id.SetDynBlockValue("类型", carType);
                        tr.Commit();
                        ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
                    }
                    else
                        break;
                }
            }
        }


        /// <summary>
        /// 添加索引块定义
        /// </summary>
        /// <returns></returns>
        public static bool AddIndexBtr()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                if (!bt.Has(indexBlockName))
                {
                    //圆
                    Circle c = new Circle
                    {
                        Center = Point3d.Origin,
                        Radius = 4
                    };
                    //字体样式
                    var tsId = db.AddTextStyle("index", "sceic.shx", "sceic.shx", 0.7);
                    //属性定义
                    AttributeDefinition att = new AttributeDefinition
                    {
                        Tag = "编号",
                        Height = 3.5,
                        HorizontalMode = TextHorizontalMode.TextCenter,
                        VerticalMode = TextVerticalMode.TextVerticalMid,
                        AlignmentPoint = Point3d.Origin,
                        TextStyleId = tsId,
                    };
                    //块定义
                    var btrId = db.AddBlockTableRecord(indexBlockName, c, att);
                    if (btrId == ObjectId.Null)
                        return false;
                }
                tr.Commit();
            }
            return true;
        }

        /// <summary>
        /// 车位排序
        /// </summary>
        /// <param name="isUpToDown">从上到下</param>
        public static void OrderCarBlock(bool isUpToDown = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                //选择车和索引
                var blocks = db.GetSelectionOfBlockRefs("\n选择车或索引", false,
                    carBlockName, indexBlockName);
                //删除原索引
                var indexs = blocks.Where(d => d.GetBlockName() == indexBlockName).ToList();
                if (indexs.Count > 0)
                {
                    foreach (var index in indexs)
                    {
                        index.UpgradeOpen();
                        index.Erase();
                    }
                }
                //选择的车
                var cars = blocks.Where(d => d.GetBlockName() == carBlockName).ToList();
                if (cars.Count != 0)
                {
                    //添加块定义
                    if (AddIndexBtr())
                    {
                        //从上到下
                        if (isUpToDown)
                        {
                            cars = cars.OrderBy(car => car.Position.X)
                                .ThenByDescending(d => d.Position.Y).ToList();
                        }
                        //从下到上
                        else
                        {
                            cars = cars.OrderBy(car => car.Position.X)
                                .ThenBy(d => d.Position.Y).ToList();
                        }

                        for (int i = 0; i < cars.Count; i++)
                        {
                            BlockReference car = cars[i];
                            Dictionary<string, string> atts = new Dictionary<string, string>
                            {
                                { "编号", $"{i + 1:D2}" }
                            };
                            //车位图形中点
                            var pt = new LineSegment3d(car.GeometricExtents.MinPoint,
                                car.GeometricExtents.MaxPoint).MidPoint;
                            var scale = new Scale3d(100);
                            //添加索引块
                            db.CurrentSpaceId.InsertBlockReference("0", indexBlockName, pt, scale, 0, atts);
                        }
                    }
                }
                else
                    AcadApp.ShowAlertDialog("选中数量为0");
                tr.Commit();
            }
        }

        /// <summary>
        /// 生成表单
        /// </summary>
        public static void AddCarTable()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                //选择车
                var cars = db.GetSelectionOfBlockRefs("\n选择车", false, carBlockName);

                if (cars.Count != 0)
                {
                    //车类型、数量字典
                    var gCar = cars.GroupBy(d => d.ObjectId.GetDynBlockValue("类型"))
                        .ToDictionary(d => d.Key, d => d.Count());
                    //文字样式
                    var tsId = db.AddTextStyle("index", "sceic.shx", "sceic.shx", 0.7);
                    //设置表样式
                    var tableStyleId = db.AddTableStyle("Summary", "index", 350, CellAlignment.BottomCenter, 100);
                    //表单
                    Table table = new Table();
                    table.TableStyle = tableStyleId;
                    //表单大小
                    table.SetSize(2 + gCar.Count, 2);
                    //设置列宽
                    foreach (var col in table.Columns)
                        col.Width = 2000;
                    //表头行
                    int row = 0;
                    table.Cells[row, 0].Value = "车位统计";
                    //标题行
                    row++;
                    table.Cells[row, 0].Value = "名称";
                    table.Cells[row, 1].Value = "数量";
                    //表单内容
                    for (int i = row + 1; i <= gCar.Count + row; i++)
                    {
                        var gc = gCar.ElementAt(i);
                        table.Cells[i, 0].Value = gc.Key;
                        table.Cells[i, 1].Value = gc.Value;
                    }

                    Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
                    //插入点
                    var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
                    //添加表单
                    if (ppr.Status == PromptStatus.OK)
                    {
                        table.Position = ppr.Value;
                        db.AddToCurrentSpace(table);
                    }

                }
                else
                    AcadApp.ShowAlertDialog("选中数量为0");
                tr.Commit();
            }
        }
    }
}
