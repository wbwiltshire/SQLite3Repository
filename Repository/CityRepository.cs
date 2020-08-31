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
	public class CityRepository : RepositoryBase<City>, IRepository<City>
	{
        private const string FINDALLCOUNT_STMT = "SELECT COUNT(Id) FROM City WHERE Active=1;";
        private const string FINDALL_STMT = "SELECT Id,Name,StateId,Active,ModifiedDt,CreateDt FROM City WHERE Active=1;";
        private const string FINDALLPAGER_STMT = "SELECT Id,Name,StateId,Active,ModifiedDt,CreateDt FROM City WHERE Active=1 ORDER BY Id LIMIT {0} OFFSET {1};";
        private const string FINDALLVIEW_STMT = "SELECT Id,Name,StateId,StateName,Active,ModifiedDt,CreateDt FROM vwFindAllCityView ORDER BY Id;";
        private const string FINDALLVIEWPAGER_STMT = "SELECT Id,Name,StateId,StateName,Active,ModifiedDt,CreateDt FROM vwFindAllCityView ORDER BY Id LIMIT {0} OFFSET {1};";
        private const string FINDBYPK_STMT = "SELECT Id,Name,StateId,Active,ModifiedDt,CreateDt FROM City WHERE Id=@pk AND Active=1;";
        private const string ADD_STMT = "INSERT INTO City ([Name],StateId,Active,ModifiedDt,CreateDt) VALUES(@p1, @p2, 1, strftime('%s','now'), strftime('%s','now')); SELECT last_insert_rowid();";
        private const string UPDATE_STMT = "UPDATE City SET Name=@p1,StateId=@p2,ModifiedDt=strftime('%s','now') WHERE Id =@pk AND Active=1;";
        private const string DELETE_STMT = "UPDATE City SET Active=0,ModifiedDt=strftime('%s','now') WHERE Id=@pk;";
        private const string ORDERBY_STMT = " ORDER BY ";
        private const string ADD_PROC = "uspAddCity";
        private const string UPDATE_PROC = "uspUpdateCity";
        private const string FINDALL_PAGEDVIEWPROC = "uspFindAllCityViewPaged";

        private ILogger logger;

        #region ctor
        //Default constructor calls the base ctor
        public CityRepository(AppSettingsConfiguration s, ILogger l, DBContext d) :
            base(s, l, d)
        { Init(l); }
        public CityRepository(AppSettingsConfiguration s, ILogger l, UnitOfWork uow, DBContext d) :
            base(s, l, uow, d)
        { Init(l); }

		private void Init(ILogger l)
        {
            logger = l;
            //Mapper = new CityMapper();
            OrderBy = "Id";
        }
        #endregion

        #region FindAll
        public override ICollection<City> FindAll()
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDALL_STMT;
            CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new CityMapToObject(logger);
            return base.FindAll();
        }
        #endregion

        #region FindAll(IPager)
        public IPager<City> FindAll(IPager<City> pager)
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = String.Format(FINDALLPAGER_STMT, pager.PageSize, pager.PageSize * pager.PageNbr);
            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new CityMapToObject(logger);
            pager.Entities = base.FindAll();
            CMDText = FINDALLCOUNT_STMT;
            pager.RowCount = base.FindAllCount();
            return pager;
        }
        #endregion

        #region FindAllView
        public ICollection<City> FindAllView()
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDALLVIEW_STMT;
            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new CityMapToObjectView(logger);
            return base.FindAll();
        }
        #endregion

        #region FindAllView(Pager)
        public IPager<City> FindAllView(IPager<City> pager)
        {
            string storedProcedure = String.Empty;

            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new CityMapToObjectView(logger);

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == FINDALL_PAGEDVIEWPROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = String.Format(FINDALLVIEWPAGER_STMT, pager.PageSize, pager.PageSize * pager.PageNbr);
                pager.Entities = base.FindAll();
            }
            else
            {
                SqlCommandType = Constants.DBCommandType.SPROC;
                CMDText = storedProcedure;
                pager.Entities = base.FindAllPaged(pager.PageSize, pager.PageSize * pager.PageNbr);
            }

            CMDText = FINDALLCOUNT_STMT;
            pager.RowCount = base.FindAllCount();
            return pager;
        }
        #endregion

        #region FindByPK(IPrimaryKey pk)
        public override City FindByPK(IPrimaryKey pk)
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDBYPK_STMT;
            MapToObject = new CityMapToObject(logger);
            return base.FindByPK(pk);
        }
        #endregion

        #region Add
        public object Add(City entity)
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
            MapFromObject = new CityMapFromObject(logger);
            result = base.Add(entity, entity.PK);
            if (result != null)
                return (int)Convert.ToInt32(result);            // Apparently, the default returned is 64 bits
            else
                return -1;
        }
        #endregion

        #region Update
        public int Update(City entity)
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
            MapFromObject = new CityMapFromObject(logger);
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
