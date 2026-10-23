using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.Interfaces;
using SQLite3Repository.Data.POCO;

namespace SQLite3Repository.Data.Repository
{
	public class StateRepository : RepositoryBase<State>, IRepository<State>
	{
		private const string FINDALLCOUNT_STMT = "SELECT COUNT(Id) FROM State WHERE Active=1;";
		private const string FINDALL_STMT = "SELECT Id,Name,Active,ModifiedDt,CreateDt FROM State WHERE Active=1 ORDER BY Id;";
        private const string FINDALLPAGER_STMT = "SELECT Id,Name,Active,ModifiedDt,CreateDt FROM State WHERE Active=1 @orderBy LIMIT @pageSize OFFSET @offset;";
        private const string FINDBYPK_STMT = "SELECT Id, Name, Active, ModifiedDt, CreateDt FROM State WHERE Id =@pk AND Active=1;";
        private const string ADD_STMT = "INSERT INTO State (Id,[Name],Active,ModifiedDt,CreateDt) VALUES(@pk, @p1, 1, strftime('%s','now'), strftime('%s','now'));";
        private const string UPDATE_STMT = "UPDATE State SET Name=@p1, ModifiedDt=strftime('%s','now') WHERE Id =@pk AND Active=1";
        private const string DELETE_STMT = "UPDATE State SET Active=0, ModifiedDt=strftime('%s','now') WHERE Id =@pk";
        //ISO 8601 UTC dates, so they deserialize directly to DateTime
        private const string JSON_STMT = "SELECT json_group_array(json_object('Id', Id, 'Name', [Name], 'Active', Active, 'ModifiedDt', strftime('%Y-%m-%dT%H:%M:%SZ',ModifiedDt,'unixepoch'), 'CreateDt', strftime('%Y-%m-%dT%H:%M:%SZ',CreateDt,'unixepoch'))) AS json_result FROM (SELECT Id,[Name],Active,ModifiedDt,CreateDt FROM State WHERE Active=1 ORDER BY Id);";

        private ILogger logger;

        #region ctor
        //Default constructor calls the base ctor
        public StateRepository(AppSettingsConfiguration s, ILogger l, DBContext d) :
            base(s, l, d)
        { Init(l); }
        public StateRepository(AppSettingsConfiguration s, ILogger l, UnitOfWork uow, DBContext d) :
            base(s, l, uow, d)
        { Init(l); }

        private void Init(ILogger l)
        {
            logger = l;
            SortColumns.Add("Name");
        }
        #endregion

        #region FindAll
        public ICollection<State> FindAll()
        {
            return base.FindAll(FINDALL_STMT, new StateMapToObject(logger));
        }
        #endregion

        #region FindAll(IPager)
        public IPager<State> FindAll(IPager<State> pager)
		{
            return base.FindAllPaged(FINDALLPAGER_STMT, FINDALLCOUNT_STMT, new StateMapToObject(logger), pager);
        }
		#endregion

		#region FindByPK(IPrimaryKey pk)
		public State FindByPK(IPrimaryKey pk)
		{
            return base.FindByPK(FINDBYPK_STMT, new StateMapToObject(logger), pk);
        }
		#endregion

		#region Add
		public object Add(State entity)
        {
            //Natural key, so returns the key when the row is added
            object result = base.Add(ADD_STMT, new StateMapFromObject(logger), entity, entity.PK);
            if (result is int rows && rows == 1)
                return entity.PK.Key;
            else
                return String.Empty;
        }
		#endregion

		#region Update
		public int Update(State entity)
        {
            return base.Update(UPDATE_STMT, new StateMapFromObject(logger), entity, entity.PK);
        }
		#endregion

		#region Delete
		public int Delete(PrimaryKey pk)
        {
            return base.Delete(DELETE_STMT, pk);
        }
        #endregion

        #region GetJSON
        public string GetJSON()
        {
            return base.ExecJSONQuery(JSON_STMT);
        }
        #endregion
    }
}
