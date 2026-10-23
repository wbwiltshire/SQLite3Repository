using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.Interfaces;
using SQLite3Repository.Data.POCO;

namespace SQLite3Repository.Data.Repository
{
	public abstract class RepositoryBase<TEntity>
	   where TEntity : class
	{
        private const string ORDERBY_TOKEN = "@orderBy";
        private const string DEFAULT_SORT_COLUMN = "Id";

		private readonly UnitOfWork unitOfWork;
		private readonly ILogger logger;
		private readonly DBContext dbc;

        #region ctor
        //ctor with no unit of work necessary
        protected RepositoryBase(AppSettingsConfiguration s, ILogger l, DBContext d)
        {
            Settings = s;
            logger = l;
            dbc = d ?? throw new ArgumentNullException(nameof(d));
        }
        //ctor with unit of work
        protected RepositoryBase(AppSettingsConfiguration s, ILogger l, UnitOfWork uow, DBContext d) :
            this(s, l, d)
        {
            unitOfWork = uow ?? throw new ArgumentNullException(nameof(uow));
            //The transaction belongs to the unit of work's connection
            if (!ReferenceEquals(uow.Context, d))
                throw new ArgumentException("The unit of work must use the same DBContext as the repository.", nameof(uow));
        }
        #endregion

        protected AppSettingsConfiguration Settings { get; private set; }

        //Columns a pager may sort by. Anything else falls back to Id, so column names are never taken from the caller.
        protected HashSet<string> SortColumns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DEFAULT_SORT_COLUMN };

        #region Sorting and Paging
        protected string BuildOrderBy(string sortColumn, SQLOrderBy direction)
        {
            string safeColumnName = DEFAULT_SORT_COLUMN;
            if (sortColumn is not null && SortColumns.TryGetValue(sortColumn, out string allowedColumn))
                safeColumnName = allowedColumn;

            //Id is a tie-breaker, so pages are deterministic for non-unique sort columns
            string sql = $"ORDER BY {safeColumnName} " + (direction == SQLOrderBy.DESC ? "DESC" : "ASC");
            if (safeColumnName != DEFAULT_SORT_COLUMN)
                sql += $", {DEFAULT_SORT_COLUMN} ASC";
            return sql;
        }

        //cmdText must contain the @orderBy token and the @pageSize and @offset parameters
        protected IPager<TEntity> FindAllPaged(string cmdText, string countCmdText, IMapToObject<TEntity> map, IPager<TEntity> pager)
        {
            ArgumentNullException.ThrowIfNull(pager);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pager.PageSize, nameof(pager.PageSize));
            ArgumentOutOfRangeException.ThrowIfNegative(pager.PageNbr, nameof(pager.PageNbr));

            string pagedCmdText = cmdText.Replace(ORDERBY_TOKEN, BuildOrderBy(pager.SortColumn, pager.Direction));
            pager.Entities = FindAll(pagedCmdText, map,
                new SqliteParameter("@pageSize", pager.PageSize),
                new SqliteParameter("@offset", (long)pager.PageSize * pager.PageNbr));
            pager.RowCount = FindAllCount(countCmdText);
            return pager;
        }
        #endregion

        #region FindAllCount
        protected int FindAllCount(string cmdText)
        {
            return Execute("FindAllCount", cmdText, false, cmd =>
            {
                //Returns an object (long), not an int
                object cnt = cmd.ExecuteScalar();
                return cnt is null || cnt is DBNull ? 0 : Convert.ToInt32(cnt);
            });
        }
        #endregion

        #region FindAll
        protected ICollection<TEntity> FindAll(string cmdText, IMapToObject<TEntity> map, params SqliteParameter[] parms)
        {
            return Execute("FindAll", cmdText, false, cmd =>
            {
                cmd.Parameters.AddRange(parms);
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    ICollection<TEntity> entities = new List<TEntity>();
                    while (reader.Read())
                        entities.Add(map.Execute(reader));
                    return entities;
                }
            });
        }
        #endregion

        #region FindByPK
        protected TEntity FindByPK(string cmdText, IMapToObject<TEntity> map, IPrimaryKey pk)
        {
            return Execute("FindByPK", cmdText, false, cmd =>
            {
                AddKeyParameters(cmd, pk);
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? map.Execute(reader) : null;
                }
            });
        }
        #endregion

        #region Add
        protected object Add(string cmdText, IMapFromObject<TEntity> map, TEntity entity, PrimaryKey pk)
        {
            return Execute<object>("Add", cmdText, true, cmd =>
            {
                map.Execute(entity, cmd);

                //If Composite, then returns rows added
                if (pk.IsComposite)
                    return cmd.ExecuteNonQuery();
                //If Identity, then returns the new PK from SELECT last_insert_rowid()
                else if (pk.IsIdentity)
                    return cmd.ExecuteScalar();
                //Else it's a natural key, so returns rows added
                else
                {
                    cmd.Parameters.Add(new SqliteParameter("@pk", pk.Key));
                    return cmd.ExecuteNonQuery();
                }
            });
        }
        #endregion

        #region Update
        protected int Update(string cmdText, IMapFromObject<TEntity> map, TEntity entity, IPrimaryKey pk)
        {
            return Execute("Update", cmdText, true, cmd =>
            {
                map.Execute(entity, cmd);
                cmd.Parameters.Add(new SqliteParameter("@pk", pk.Key));
                return cmd.ExecuteNonQuery();
            });
        }
        #endregion

        #region Delete
        protected int Delete(string cmdText, IPrimaryKey pk)
        {
            return Execute("Delete", cmdText, true, cmd =>
            {
                AddKeyParameters(cmd, pk);
                return cmd.ExecuteNonQuery();
            });
        }
        #endregion

        #region ExecNonQuery
        protected int ExecNonQuery(string cmdText, params SqliteParameter[] parms)
        {
            return Execute("ExecNonQuery", cmdText, true, cmd =>
            {
                cmd.Parameters.AddRange(parms);
                return cmd.ExecuteNonQuery();
            });
        }
        #endregion

        #region ExecJSONQuery
        protected string ExecJSONQuery(string cmdText, params SqliteParameter[] parms)
        {
            return Execute("ExecJSONQuery", cmdText, false, cmd =>
            {
                cmd.Parameters.AddRange(parms);
                object json = cmd.ExecuteScalar();
                return json is null || json is DBNull ? "[]" : (string)json;
            });
        }
        #endregion

        #region Command execution
        //Writes start (or join) the unit of work transaction. Reads join it, if one has started, so they see uncommitted writes.
        private T Execute<T>(string operation, string cmdText, bool enlist, Func<SqliteCommand, T> execute)
        {
            try
            {
                if (enlist && unitOfWork is not null && !unitOfWork.Enlist())
                    throw new InvalidOperationException("Unable to enlist repository operation in unit of work.");
                if (dbc.Connection.State != ConnectionState.Open && !dbc.Open())
                    throw new InvalidOperationException("Unable to open the database connection.");

                using (SqliteCommand cmd = new SqliteCommand(cmdText, dbc.Connection, unitOfWork?.Transaction))
                {
                    T result = execute(cmd);
                    logger.LogInformation("{Operation} complete for {Entity} entity.", operation, typeof(TEntity).Name);
                    return result;
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex, "{Operation} failed for {Entity} entity.", operation, typeof(TEntity).Name);
                throw;
            }
        }

        private static void AddKeyParameters(SqliteCommand cmd, IPrimaryKey pk)
        {
            if (pk.IsComposite)
            {
                int idx = 1;
                foreach (object k in ((PrimaryKey)pk).CompositeKey)
                {
                    cmd.Parameters.Add(new SqliteParameter("@pk" + idx.ToString(), k));
                    idx++;
                }
            }
            else
                cmd.Parameters.Add(new SqliteParameter("@pk", pk.Key));
        }
        #endregion
    }
}
