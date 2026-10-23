using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.Interfaces;
using SQLite3Repository.Data.POCO;

namespace SQLite3Repository.Data.Repository
{
	public class CityRepository : RepositoryBase<City>, IRepository<City>
	{
        private const string FINDALLCOUNT_STMT = "SELECT COUNT(Id) FROM City WHERE Active=1;";
        private const string FINDALL_STMT = "SELECT Id,Name,StateId,Active,ModifiedDt,CreateDt FROM City WHERE Active=1 ORDER BY Id;";
        private const string FINDALLPAGER_STMT = "SELECT Id,Name,StateId,Active,ModifiedDt,CreateDt FROM City WHERE Active=1 @orderBy LIMIT @pageSize OFFSET @offset;";
        private const string FINDALLVIEWCOUNT_STMT = "SELECT COUNT(Id) FROM vwFindAllCityView;";
        private const string FINDALLVIEW_STMT = "SELECT Id,Name,StateId,StateName,Active,ModifiedDt,CreateDt FROM vwFindAllCityView ORDER BY Id;";
        private const string FINDALLVIEWPAGER_STMT = "SELECT Id,Name,StateId,StateName,Active,ModifiedDt,CreateDt FROM vwFindAllCityView @orderBy LIMIT @pageSize OFFSET @offset;";
        private const string FINDBYPK_STMT = "SELECT Id,Name,StateId,Active,ModifiedDt,CreateDt FROM City WHERE Id=@pk AND Active=1;";
        private const string ADD_STMT = "INSERT INTO City ([Name],StateId,Active,ModifiedDt,CreateDt) VALUES(@p1, @p2, 1, strftime('%s','now'), strftime('%s','now')); SELECT last_insert_rowid();";
        private const string UPDATE_STMT = "UPDATE City SET Name=@p1,StateId=@p2,ModifiedDt=strftime('%s','now') WHERE Id =@pk AND Active=1;";
        private const string DELETE_STMT = "UPDATE City SET Active=0,ModifiedDt=strftime('%s','now') WHERE Id=@pk;";

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
            SortColumns.UnionWith(new[] { "Name", "StateId" });
        }
        #endregion

        #region FindAll
        public ICollection<City> FindAll()
        {
            return base.FindAll(FINDALL_STMT, new CityMapToObject(logger));
        }
        #endregion

        #region FindAll(IPager)
        public IPager<City> FindAll(IPager<City> pager)
        {
            return base.FindAllPaged(FINDALLPAGER_STMT, FINDALLCOUNT_STMT, new CityMapToObject(logger), pager);
        }
        #endregion

        #region FindAllView
        public ICollection<City> FindAllView()
        {
            return base.FindAll(FINDALLVIEW_STMT, new CityMapToObjectView(logger));
        }
        #endregion

        #region FindAllView(Pager)
        public IPager<City> FindAllView(IPager<City> pager)
        {
            return base.FindAllPaged(FINDALLVIEWPAGER_STMT, FINDALLVIEWCOUNT_STMT, new CityMapToObjectView(logger), pager);
        }
        #endregion

        #region FindByPK(IPrimaryKey pk)
        public City FindByPK(IPrimaryKey pk)
        {
            return base.FindByPK(FINDBYPK_STMT, new CityMapToObject(logger), pk);
        }
        #endregion

        #region Add
        public object Add(City entity)
        {
            object result = base.Add(ADD_STMT, new CityMapFromObject(logger), entity, entity.PK);
            if (result != null)
                return Convert.ToInt32(result);            // last_insert_rowid() returns 64 bits
            else
                return -1;
        }
        #endregion

        #region Update
        public int Update(City entity)
        {
            return base.Update(UPDATE_STMT, new CityMapFromObject(logger), entity, entity.PK);
        }
        #endregion

        #region Delete
        public int Delete(PrimaryKey pk)
        {
            return base.Delete(DELETE_STMT, pk);
        }
        #endregion
    }
}
