using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.Interfaces;
using SQLite3Repository.Data.POCO;

namespace SQLite3Repository.Data.Repository
{
	public class StateRepository : RepositoryBase<State>, IRepository<State>
	{
		private const string FINDALLCOUNT_STMT = "SELECT COUNT(Id) FROM State WHERE Active=1;";
		private const string FINDALL_STMT = "SELECT Id,Name,Active,ModifiedDt,CreateDt FROM State WHERE Active=1;";
        private const string FINDALLPAGER_STMT = "SELECT Id,Name,Active,ModifiedDt,CreateDt FROM State WHERE Active=1 ORDER BY Id LIMIT {1} OFFSET {0};";
        private const string FINDBYPK_STMT = "SELECT Id, Name, Active, ModifiedDt, CreateDt FROM State WHERE Id =@pk AND Active=1;";
        private const string ADD_STMT = "INSERT INTO State (Id,[Name],Active,ModifiedDt,CreateDt) VALUES(@pk, @p1, 1, strftime('%s','now'), strftime('%s','now'));";
        private const string UPDATE_STMT = "UPDATE State SET Name=@p1, ModifiedDt=strftime('%s','now') WHERE Id =@pk AND Active=1";
        private const string DELETE_STMT = "UPDATE State SET Active=0, ModifiedDt=strftime('%s','now') WHERE Id =@pk";
        private const string ORDERBY_STMT = " ORDER BY ";
        private const string ADD_PROC = "uspAddState";
        private const string UPDATE_PROC = "uspUpdateState";

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
            //Mapper = new StateMapper();
            OrderBy = "Id";
        }
        #endregion

        #region FindAll
        public override ICollection<State> FindAll()
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDALL_STMT;
            CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new StateMapToObject(logger);
            return base.FindAll();
        }
		#endregion

		public IPager<State> FindAll(IPager<State> pager)
		{
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = String.Format(FINDALLPAGER_STMT, pager.PageSize, pager.PageSize * pager.PageNbr);
            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new StateMapToObject(logger);
            pager.Entities = base.FindAll();
            CMDText = FINDALLCOUNT_STMT;
            pager.RowCount = base.FindAllCount();
            return pager;
        }

        #region FindByPK(IPrimaryKey pk)
        public override State FindByPK(IPrimaryKey pk)
		{
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDBYPK_STMT;
            MapToObject = new StateMapToObject(logger);
            return base.FindByPK(pk);
        }
		#endregion

		#region Add
		public object Add(State entity)
        {
            string storedProcedure = String.Empty;
            object result;

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == ADD_PROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = ADD_STMT;
            }
            else
            {
                SqlCommandType = Constants.DBCommandType.SPROC;
                CMDText = storedProcedure;
            }
            MapFromObject = new StateMapFromObject(logger);
            result = base.Add(entity, entity.PK);
            if (result != null && (int)result == 1)
                return entity.PK.Key;
            else
                return String.Empty;
        }
		#endregion

		#region Update
		public int Update(State entity)
        {
            string storedProcedure = String.Empty;

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == UPDATE_PROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = UPDATE_STMT;
            }
            else
            {
                SqlCommandType = Constants.DBCommandType.SPROC;
                CMDText = storedProcedure;
            }
            MapFromObject = new StateMapFromObject(logger);
            return base.Update(entity, entity.PK);
        }
		#endregion

		#region Delete
		public int Delete(PrimaryKey pk)
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = DELETE_STMT;
            return base.Delete(pk);
        }
		#endregion
	}
}