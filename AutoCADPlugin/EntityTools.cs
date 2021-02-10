using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADPlugin
{
    static class EntityTools
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entity">实体</param>
        /// <returns>对象Id</returns>
        public static ObjectId AddNewEntity(this Database db, Entity entity)
        {
            //开启事务处理
            Transaction tr = db.TransactionManager.StartTransaction();
            //打开块表
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            //打开块表记录
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            //添加对象到块表记录，返回Id
            var id = btr.AppendEntity(entity);
            //
            btr.DowngradeOpen();
            tr.AddNewlyCreatedDBObject(entity, true);
            tr.Commit();
            return id;
        }

        /// <summary>
        /// 添加实体组
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entitys">多个实体</param>
        /// <returns>对象Id</returns>
        public static ObjectId[] AddNewEntity(this Database db, params Entity[] entitys)
        {
            ObjectId[] ids = new ObjectId[entitys.Length];
            for (int i = 0; i < entitys.Length; i++)
            {
                ids[i] = db.AddNewEntity(entitys[i]);
            }
            return ids;
        }

        public static ObjectId AddNewPolyLine(this Database db, List<Point3d> pts, bool isClosed = true)
        {
            Polyline pl = new Polyline();
            for (int i = 0; i < pts.Count; i++)
            {
                Point3d pt = pts[i];
                pl.AddVertexAt(i, new Point2d(pt.X, pt.Y), 0, 0, 0);

            }
            pl.Closed = isClosed;


            return EntityTools.AddNewEntity(db, pl);
        }
    }
}
