using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;

namespace SQLite3Repository.Data.Interfaces
{
    public enum Action { Add, Update, Delete };

    public interface IPrimaryKey
    {
        object Key { get; set; }
        bool IsIdentity { get; set; }
        bool IsComposite { get; set; }
    }

    public interface IDBContext : IDisposable
    {
        bool Open();
        void Close();
    }

    public interface IUOW
    {
        bool Save();
        bool Rollback();
    }

    public interface IRepository<TEntity>
        where TEntity : class
    {
        IPager<TEntity> FindAll(IPager<TEntity> pager);
        ICollection<TEntity> FindAll();
        TEntity FindByPK(IPrimaryKey pk);
        object Add(TEntity entity);
        int Update(TEntity entity);
        int Delete(PrimaryKey pk);
    }

    public interface IMapToObject<TEntity>
        where TEntity : class
    {
        TEntity Execute(IDataReader reader);
    }
    public interface IMapFromObject<TEntity>
        where TEntity : class
    {
        void Execute(TEntity entity, SqliteCommand cmd);
    }

}
