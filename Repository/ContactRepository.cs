using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.Interfaces;
using SQLite3Repository.Data.POCO;

namespace SQLite3Repository.Data.Repository
{
	public class ContactRepository : RepositoryBase<Contact>, IRepository<Contact>
	{
        private const string FINDALLCOUNT_STMT = "SELECT COUNT(Id) FROM Contact WHERE Active=1;";
        private const string FINDALL_STMT = "SELECT [Id],[FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],Active,ModifiedDt,CreateDt FROM Contact WHERE Active=1 ORDER BY Id;";
        private const string FINDALLPAGER_STMT = "SELECT [Id],[FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],Active,ModifiedDt,CreateDt FROM Contact WHERE Active=1 @orderBy LIMIT @pageSize OFFSET @offset;";
        private const string FINDALLVIEWCOUNT_STMT = "SELECT COUNT(Id) FROM vwFindAllContactView;";
        private const string FINDALLVIEW_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, CityName, StateId, StateName, Active, ModifiedDt, CreateDt FROM vwFindAllContactView ORDER BY Id;";
        private const string FINDALLVIEWPAGER_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, CityName, StateId, StateName, Active, ModifiedDt, CreateDt FROM vwFindAllContactView @orderBy LIMIT @pageSize OFFSET @offset;";
        private const string FINDBYPK_STMT = "SELECT [Id],[FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],Active,ModifiedDt,CreateDt FROM Contact WHERE Id=@pk AND Active=1;";
        private const string FINDBYPKVIEW_STMT = "SELECT [Id],[FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],[CityName],[StateId],[StateName],Active,ModifiedDt,CreateDt FROM vwFindAllContactView WHERE Id=@pk AND Active=1;";
        private const string ADD_STMT = "INSERT INTO Contact ([FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],[Active],[ModifiedDt],[CreateDt]) VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,1, strftime('%s','now'), strftime('%s','now')); SELECT last_insert_rowid();";
        private const string UPDATE_STMT = "UPDATE Contact SET [FirstName]=@p1,[LastName]=@p2,[Address1]=@p3,[Address2]=@p4,[Notes]=@p5,[ZipCode]=@p6,[HomePhone]=@p7,[WorkPhone]=@p8,[CellPhone]=@p9,[EMail]=@p10,CityId=@p11,ModifiedDt=strftime('%s','now') WHERE Id=@pk AND Active=1";
        private const string DELETE_STMT = "UPDATE Contact SET Active=0,ModifiedDt=strftime('%s','now') WHERE Id=@pk";
        private const string NONQUERY_TEST = "UPDATE Contact SET ModifiedDt=strftime('%s','now') WHERE Active=1;";

        private ILogger logger;

        #region ctor
        //Default constructor calls the base ctor
        public ContactRepository(AppSettingsConfiguration s, ILogger l, DBContext d) :
            base(s, l, d)
        { Init(l); }
        public ContactRepository(AppSettingsConfiguration s, ILogger l, UnitOfWork uow, DBContext d) :
            base(s, l, uow, d)
        { Init(l); }

        private void Init(ILogger l)
        {
            logger = l;
            SortColumns.UnionWith(new[] { "FirstName", "LastName", "ZipCode", "CityId" });
        }
        #endregion

        #region FindAll
        public ICollection<Contact> FindAll()
        {
            return base.FindAll(FINDALL_STMT, new ContactMapToObject(logger));
        }
        #endregion

        #region FindAll(IPager)
        public IPager<Contact> FindAll(IPager<Contact> pager)
        {
            return base.FindAllPaged(FINDALLPAGER_STMT, FINDALLCOUNT_STMT, new ContactMapToObject(logger), pager);
        }
        #endregion

        #region FindAllView
        public ICollection<Contact> FindAllView()
        {
            return base.FindAll(FINDALLVIEW_STMT, new ContactMapToObjectView(logger));
        }
        #endregion

        #region FindAllView(Pager)
        public IPager<Contact> FindAllView(IPager<Contact> pager)
        {
            return base.FindAllPaged(FINDALLVIEWPAGER_STMT, FINDALLVIEWCOUNT_STMT, new ContactMapToObjectView(logger), pager);
        }
        #endregion

        #region FindByPK(IPrimaryKey pk)
        public Contact FindByPK(IPrimaryKey pk)
        {
            return base.FindByPK(FINDBYPK_STMT, new ContactMapToObject(logger), pk);
        }
        #endregion

        #region FindViewByPK
        public Contact FindViewByPK(IPrimaryKey pk)
        {
            return base.FindByPK(FINDBYPKVIEW_STMT, new ContactMapToObjectView(logger), pk);
        }
        #endregion

        #region Add
        public object Add(Contact entity)
        {
            object result = base.Add(ADD_STMT, new ContactMapFromObject(logger), entity, entity.PK);
            if (result != null)
                return Convert.ToInt32(result);            // last_insert_rowid() returns 64 bits
            else
                return -1;
        }
        #endregion

        #region Update
        public int Update(Contact entity)
        {
            return base.Update(UPDATE_STMT, new ContactMapFromObject(logger), entity, entity.PK);
        }
        #endregion

        #region Delete
        public int Delete(PrimaryKey pk)
        {
            return base.Delete(DELETE_STMT, pk);
        }
        #endregion

        #region ExecNonQuery
        public int NonQuery()
        {
            return base.ExecNonQuery(NONQUERY_TEST);
        }
        #endregion
    }
}
