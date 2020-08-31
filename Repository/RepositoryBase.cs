using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.Interfaces;
using SQLite3Repository.Data.POCO;

namespace SQLite3Repository.Data.Repository
{
	public abstract class RepositoryBase<TEntity>
	   where TEntity : class
	{

		private UnitOfWork unitOfWork;
		private readonly ILogger logger;
		private DBContext dbc;

        #region ctor
        //ctor with no unit of work necessary
        protected RepositoryBase(AppSettingsConfiguration s, ILogger l, DBContext d)
        {
            Settings = s;
            logger = l;
            dbc = d;
        }
        //ctor with unit of work
        protected RepositoryBase(AppSettingsConfiguration s, ILogger l, UnitOfWork uow, DBContext d)
        {
            Settings = s;
            logger = l;
            unitOfWork = uow;
            dbc = d;
        }
        #endregion

        public string OrderBy { get; set; }
        protected string CMDText { get; set; }
        protected AppSettingsConfiguration Settings { get; private set; }
        protected Constants.DBCommandType SqlCommandType { get; set; }
        protected MapToObjectBase<TEntity> MapToObject { get; set; }
        protected MapFromObjectBase<TEntity> MapFromObject { get; set; }

        #region FindAllCount
        protected int FindAllCount()
        {
            object cnt;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    //Returns an object, not an int
                    cnt = cmd.ExecuteScalar();
                    logger.LogInformation("FindAllCount complete.");
                    if (cnt != null)
                        return Convert.ToInt32(cnt);
                    else
                        return 0;
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
                return 0;
            }
        }
        #endregion

        #region FindAll
        public virtual ICollection<TEntity> FindAll()
        {
            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        ICollection<TEntity> entities = new List<TEntity>();
                        while (reader.Read())
                        {
                            entities.Add(MapToObject.Execute(reader));
                        }
                        logger.LogInformation($"FindAll complete for {typeof(TEntity)} entity.");
                        return entities;
                    }
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }
        #endregion

        #region FindAllPaged
        public virtual ICollection<TEntity> FindAllPaged(int offset, int pageSize)
        {
            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqliteParameter("@p1", offset));
                    cmd.Parameters.Add(new SqliteParameter("@p2", pageSize));

                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        ICollection<TEntity> entities = new List<TEntity>();
                        while (reader.Read())
                        {
                            entities.Add(MapToObject.Execute(reader));
                        }
                        logger.LogInformation($"FindAllPaged complete for {typeof(TEntity)} entity.");
                        return entities;
                    }
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }
        #endregion

        #region FindByPK
        public virtual TEntity FindByPK(IPrimaryKey pk)
        {
            int idx = 1;
            TEntity entity = null;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    if (pk.IsComposite)
                    {
                        foreach (int k in ((PrimaryKey)pk).CompositeKey)
                        {
                            cmd.Parameters.Add(new SqliteParameter("@pk" + idx.ToString(), k));
                            idx++;
                        }
                    }
                    else
                        cmd.Parameters.Add(new SqliteParameter("@pk", pk.Key));
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            entity = MapToObject.Execute(reader);
                        else
                            entity = null;
                        logger.LogInformation($"FindByPK complete for {typeof(TEntity)} entity.");
                    }
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
                return null;
            }

            return entity;
        }
        #endregion

        #region Add
        protected object Add(TEntity entity, PrimaryKey pk)
        {

            object result = null;
            int rows = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                if (unitOfWork != null) unitOfWork.Enlist();
                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    if (SqlCommandType == Constants.DBCommandType.SPROC)
                        cmd.CommandType = CommandType.StoredProcedure;
                    MapFromObject.Execute(entity, cmd);

                    //If Composite, then returns an array of objects
                    if (pk.IsComposite)
                    {
                        //returns CompositeKey
                        rows = cmd.ExecuteNonQuery();
                        result = rows;
                    }
                    //If Identity, then it's numeric
                    else if (pk.IsIdentity)
                    {
                        //returns PK
                        result = cmd.ExecuteScalar();
                    }
                    //Else it's a natural key
                    else
                    {
                        //returns rows updated and sets result to key
                        cmd.Parameters.Add(new SqliteParameter("@pk", pk.Key));
                        rows = cmd.ExecuteNonQuery();
                        result = rows;
                    }
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
            }
            logger.LogInformation($"Add complete for {typeof(TEntity)} entity.");
            return result;
        }
        #endregion

        #region Update
        protected int Update(TEntity entity, IPrimaryKey pk)
        {
            int rows = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                if (unitOfWork != null) unitOfWork.Enlist();
                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    if (SqlCommandType == Constants.DBCommandType.SPROC)
                        cmd.CommandType = CommandType.StoredProcedure;
                    MapFromObject.Execute(entity, cmd);
                    cmd.Parameters.Add(new SqliteParameter("@pk", pk.Key));
                    rows = cmd.ExecuteNonQuery();
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
            }
            logger.LogInformation($"Update complete for {typeof(TEntity)} entity.");
            return rows;
        }
        #endregion

        #region Delete
        protected int Delete(IPrimaryKey pk)
        {
            int idx = 1;
            int rows = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                if (unitOfWork != null) unitOfWork.Enlist();
                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    if (pk.IsComposite)
                    {
                        foreach (int k in ((PrimaryKey)pk).CompositeKey)
                        {
                            cmd.Parameters.Add(new SqliteParameter("@pk" + idx.ToString(), k));
                            idx++;
                        }
                    }
                    else
                        cmd.Parameters.Add(new SqliteParameter("@pk", pk.Key));

                    rows = cmd.ExecuteNonQuery();
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
            }
            logger.LogInformation($"Delete complete for {typeof(TEntity)} entity.");
            return rows;
        }
        #endregion

        #region ExecNonQuery
        protected int ExecNonQuery(IList<SqliteParameter> p)
        {
            int rows = 0;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    if (SqlCommandType == Constants.DBCommandType.SPROC)
                        cmd.CommandType = CommandType.StoredProcedure;

                    foreach (SqliteParameter s in p)
                        cmd.Parameters.Add(s);

                    //Returns an object, not an int
                    rows = cmd.ExecuteNonQuery();
                    logger.LogInformation("ExecNonQuery complete.");
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
                rows = 0;
            }

            return rows;
        }
        #endregion

        #region ExecJSONQuery
        protected string ExecJSONQuery(IList<SqliteParameter> parms)
        {
            string result = String.Empty;

            try
            {
                if (dbc.Connection.State != ConnectionState.Open)
                    dbc.Open();

                using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                {
                    if (SqlCommandType == Constants.DBCommandType.SPROC)
                        cmd.CommandType = CommandType.StoredProcedure;

                    foreach (SqliteParameter parm in parms)
                        cmd.Parameters.Add(parm);

                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        //Returns a string
                        while (reader.Read())
                            result += reader.GetString(0);
                    }
                    logger.LogInformation("ExecJSONQuery complete.");
                    return result;
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
                return "{}";
            }
        }
        #endregion

        #region ExecStoredProc
        protected int ExecStoredProc(IList<SqliteParameter> p)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
