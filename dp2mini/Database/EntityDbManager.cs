using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2mini
{
    public class EntityDbManager
    {

        private EntityDbManager(string dir)
        {
            this.ConnectionDb(dir);
        }

        // 数据库对象
        EntityDB _dbclient = null;

        // 连接数据库
        public void ConnectionDb(string dir)
        {
            this._dbclient = new EntityDB(dir);
            //Create the database file at a path defined in SimpleDataStorage
            this._dbclient.Database.EnsureCreated();
            //Create the database tables defined in SimpleDataStorage
            this._dbclient.Database.Migrate();
        }




        // 给本地库增加一笔册记录
        public void AddEntity(Entity entity)
        {
            this._dbclient.Entities.Add(entity);
            this._dbclient.SaveChanges(true);
        }

        // 获取全部
        public List<Entity> GetEntityList()
        {
            return this._dbclient.Entities.ToList();
        }


    }


}
