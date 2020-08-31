using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data.Interfaces;

namespace SQLite3Repository.Data
{
    public class PrimaryKey : IPrimaryKey
    {
        private string tempString = String.Empty;
        public object Key { get; set; }
        public object[] CompositeKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComposite { get; set; }
        
        //ctor
        public PrimaryKey()
        {
            IsIdentity = false;
            IsComposite = false;
        }

        public override string ToString()
        {
            if (IsComposite)
            {
                foreach (object k in CompositeKey)
                    tempString += k.ToString() + "|";
                return $"|{tempString}{IsComposite}";
            }
            else
                return $"|{Key}|{IsIdentity}";
        }
    }

    public abstract class MapToObjectBase<TEntity> : IMapToObject<TEntity>
        where TEntity : class
    {
        protected readonly ILogger logger;

        protected MapToObjectBase(ILogger l)
        {
            logger = l;
        }

        public abstract TEntity Execute(IDataReader reader);
    }
    public abstract class MapFromObjectBase<TEntity> : IMapFromObject<TEntity>
        where TEntity : class
    {
        protected readonly ILogger logger;

        protected MapFromObjectBase(ILogger l)
        {
            logger = l;
        }

        public abstract void Execute(TEntity entity, SqliteCommand cmd);
    }

}