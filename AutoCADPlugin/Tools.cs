using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoCADPlugin.Args;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCADPlugin
{
    public static class Tools
    {
        public static void AddCarBlock(string filePath, string carType, double angle)
        {

            if (!File.Exists(filePath))
            {
                AcadApp.ShowAlertDialog($"{filePath} 文件不存在。");
                return;
            }

            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                bool success = db.ImportBlockFromDwg(filePath, carBlockName, DuplicateRecordCloning.Replace);
                if (success)
                {
                    db.AddLayer(carLayer, "车位图层");
                    db.SetLayerColor(carLayer, carColorIndex);
                    tr.Commit();
                }
                else
                    return;
            }

            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));
            while (ppr.Status == PromptStatus.OK)
            {
                var pt = ppr.Value;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var id = db.CurrentSpaceId.InsertBlockReference(carLayer, carBlockName, pt, new Scale3d(1), angle);

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



        public static bool AddIndexBtr()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                if (!bt.Has(indexBlockName))
                {
                    Circle c = new Circle
                    {
                        Center = Point3d.Origin,
                        Radius = 4
                    };
                    var tsId = db.AddTextStyle("test", "sceic.shx", "sceic.shx", 0.7);
                    AttributeDefinition att = new AttributeDefinition
                    {
                        Tag = "编号",
                        Height = 3.5,
                        HorizontalMode = TextHorizontalMode.TextCenter,
                        VerticalMode = TextVerticalMode.TextVerticalMid,
                        AlignmentPoint = Point3d.Origin,
                        TextStyleId = tsId,
                    };
                    var btrId = db.AddBlockTableRecord(indexBlockName, c, att);
                    if (btrId == ObjectId.Null)
                    {
                        return false;
                    }
                }
                tr.Commit();
            }
            return true;
        }

        public static void OrderCarBlock(bool isUpToDown = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var cars = db.GetSelectionOfBlockRefs("\n选择车", false, carBlockName);

                var indexs = db.GetBlockRefsInCurrentSpace(false, indexBlockName);
                if (indexs.Count > 0)
                {
                    foreach (var index in indexs)
                    {
                        index.UpgradeOpen();
                        index.Erase();
                    }
                }

                if (cars.Count == 0)
                {
                    AcadApp.ShowAlertDialog("选中数量为0");
                }
                else
                {
                    if (AddIndexBtr())
                    {
                        if (isUpToDown)
                        {
                            cars = cars.OrderBy(car => car.Position.X)
                                .ThenByDescending(d => d.Position.Y).ToList();
                        }
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
                            var pt = new LineSegment3d(car.GeometricExtents.MinPoint,
                                car.GeometricExtents.MaxPoint).MidPoint;
                            var scale = new Scale3d(100);
                            db.CurrentSpaceId.InsertBlockReference("0", indexBlockName, pt, scale, 0, atts);
                        }
                    }
                }
                tr.Commit();
            }
        }

        public static void AddCarTable()
        {
            Database db = HostApplicationServices.WorkingDatabase;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var cars = db.GetSelectionOfBlockRefs("\n选择车", false, carBlockName);

                if (cars.Count == 0)
                {
                    AcadApp.ShowAlertDialog("选中数量为0");
                }
                else
                {
                    Dictionary<string, int> gCar = cars.GroupBy(d => d.ObjectId.GetDynBlockValue("类型"))
                        .ToDictionary(d => d.Key, d => d.Count());


                    Table table = new Table();

                    table.SetSize(2 + gCar.Count, 2);

                    int row = 0;
                    table.Cells[row, 0].Value = "车位统计";

                    row++;
                    table.Cells[row, 0].Value = "名称";
                    table.Cells[row, 1].Value = "数量";

                    row++;
                    foreach (var gc in gCar)
                    {
                        table.Cells[row, 0].Value = gc.Key;
                        table.Cells[row, 1].Value = gc.Value;
                        row++;
                    }

                    Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

                    var ppr = ed.GetPoint(new PromptPointOptions("\n指定点"));

                    if (ppr.Status == PromptStatus.OK)
                    {
                        var pt = ppr.Value;
                        table.Position = pt;
                        db.AddToCurrentSpace(table);
                    }

                }
                tr.Commit();
            }
        }
    }
}
