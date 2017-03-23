using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using AntData.ORM.Data;
using AntServiceStack.DbModel;

namespace AntServiceStack.Manager.Repository
{
    public abstract class BaseRepository<T> where T : class
    {
        protected BaseRepository()
        {

        }

        public MysqlDbContext<Entitys> DB
        {
            get
            {
                var db = new MysqlDbContext<Entitys>("antsoa");
#if DEBUG
                db.IsEnableLogTrace = true;
                db.OnLogTrace = OnCustomerTraceConnection;
#endif
                return db;
            }
        }

        /// <summary>
        /// DB里面所有的Entity
        /// </summary>
        public Entitys Entitys
        {
            get
            {
                return DB.Tables;
            }
        }

        /// <summary>
        /// 当前的Entity
        /// </summary>
        public IQueryable<T> Entity
        {
            get { return Entitys.Get<T>(); }
        }

        /// <summary>
        /// 显示sql
        /// </summary>
        /// <param name="customerTraceInfo"></param>
        protected void OnCustomerTraceConnection(CustomerTraceInfo customerTraceInfo)
        {
            try
            {
                string sql = customerTraceInfo.CustomerParams.Aggregate(customerTraceInfo.SqlText,
                       (current, item) => current.Replace(item.Key, item.Value.Value.ToString()));
                Trace.Write(sql);
            }
            catch (Exception)
            {
                //ignore
            }
        }
    }
}