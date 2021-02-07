using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADPlugin
{
    class EntityTools
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="entity">实体</param>
        /// <returns>对象Id</returns>
        public static ObjectId AddNewEntity(Database db, Entity entity)
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
        public static ObjectId[] AddNewEntity(Database db, params Entity[] entitys)
        {
            ObjectId[] ids = new ObjectId[entitys.Length];
            for (int i = 0; i < entitys.Length; i++)
            {
                ids[i] = AddNewEntity(db, entitys[i]);
            }
            return ids;
        }
    }
}
