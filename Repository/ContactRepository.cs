using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.Interfaces;
using SQLite3Repository.Data.POCO;

namespace SQLite3Repository.Data.Repository
{
	public class ContactRepository : RepositoryBase<Contact>, IRepository<Contact>
	{
        private const string FINDALLCOUNT_STMT = "SELECT COUNT(Id) FROM Contact WHERE Active=1;";
        private const string FINDALL_STMT = "SELECT [Id],[FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],Active,ModifiedDt,CreateDt FROM Contact WHERE Active=1;";
        private const string FINDALLPAGER_STMT = "SELECT [Id],[FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],Active,ModifiedDt,CreateDt FROM Contact WHERE Active=1 ORDER BY Id LIMIT {0} OFFSET {1};";
        private const string FINDALLVIEW_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, CityName, StateId, StateName, Active, ModifiedDt, CreateDt FROM vwFindAllContactView;";
        private const string FINDALLVIEWPAGER_STMT = "SELECT Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, CityName, StateId, StateName, Active, ModifiedDt, CreateDt FROM vwFindAllContactView ORDER BY Id LIMIT {0} OFFSET {1};";
        private const string FINDBYPK_STMT = "SELECT [Id],[FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],Active,ModifiedDt,CreateDt FROM Contact WHERE Id=@pk AND Active=1;";
        private const string FINDBYPKVIEW_STMT = "SELECT [Id],[FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],[CityName],[StateId],[StateName],Active,ModifiedDt,CreateDt FROM vwFindAllContactView WHERE Id=@pk AND Active=1;";
        private const string ADD_STMT = "INSERT INTO Contact ([FirstName],[LastName],[Address1],[Address2],[Notes],[ZipCode],[HomePhone],[WorkPhone],[CellPhone],[EMail],[CityId],[Active],[ModifiedDt],[CreateDt]) VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,1, strftime('%s','now'), strftime('%s','now')); SELECT last_insert_rowid();";
        private const string UPDATE_STMT = "UPDATE Contact SET [FirstName]=@p1,[LastName]=@p2,[Address1]=@p3,[Address2]=@p4,[Notes]=@p5,[ZipCode]=@p6,[HomePhone]=@p7,[WorkPhone]=@p8,[CellPhone]=@p9,[EMail]=@p10,CityId=@p11,ModifiedDt=strftime('%s','now') WHERE Id=@pk AND Active=1";
        private const string DELETE_STMT = "UPDATE Contact SET Active=0,ModifiedDt=strftime('%s','now') WHERE Id=@pk";
        private const string ORDERBY_STMT = " ORDER BY ";
        private const string ADD_PROC = "uspAddContact";
        private const string UPDATE_PROC = "uspUpdateContact";
        private const string FINDALL_PAGEDVIEWPROC = "uspFindAllContactViewPaged";
        //private const string STORED_PROC = "uspStoredProc";
        private const string NONQUERY_PROC = "uspNonQuery";
        private const string NONQUERY_TEST = "UPDATE Contact SET ModifiedDt=strftime('%s','now');";

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
            //Mapper = new ContactMapper();
            OrderBy = "Id";
        }
        #endregion

        #region FindAll
        public override ICollection<Contact> FindAll()
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDALL_STMT;
            CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new ContactMapToObject(logger);
            return base.FindAll();
        }
        #endregion

        #region FindAll(IPager)
        public IPager<Contact> FindAll(IPager<Contact> pager)
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = String.Format(FINDALLPAGER_STMT, pager.PageSize, pager.PageSize * pager.PageNbr);
            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new ContactMapToObject(logger);
            pager.Entities = base.FindAll();
            CMDText = FINDALLCOUNT_STMT;
            pager.RowCount = base.FindAllCount();
            return pager;
        }
        #endregion

        #region FindAllView
        public ICollection<Contact> FindAllView()
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDALLVIEW_STMT;
            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new ContactMapToObjectView(logger);
            return base.FindAll();
        }
        #endregion

        #region FindAllView(Pager)
        public IPager<Contact> FindAllView(IPager<Contact> pager)
        {
            string storedProcedure = String.Empty;

            //CMDText += ORDERBY_STMT + OrderBy;
            MapToObject = new ContactMapToObjectView(logger);

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
        public override Contact FindByPK(IPrimaryKey pk)
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDBYPK_STMT;
            MapToObject = new ContactMapToObject(logger);
            return base.FindByPK(pk);
        }
        #endregion

        #region FindViewByPK
        public Contact FindViewByPK(IPrimaryKey pk)
        {
            SqlCommandType = Constants.DBCommandType.SQL;
            CMDText = FINDBYPKVIEW_STMT;
            MapToObject = new ContactMapToObjectView(logger);
            return base.FindByPK(pk);
        }
        #endregion

        #region Add
        public object Add(Contact entity)
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
            MapFromObject = new ContactMapFromObject(logger);
            result = base.Add(entity, entity.PK);
            if (result != null)
                return (int)Convert.ToInt32(result);            // Apparently, the default returned is 64 bits
            else
                return -1;
        }
        #endregion

        #region Update
        public int Update(Contact entity)
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
            MapFromObject = new ContactMapFromObject(logger);
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

        #region ExecNonQuery
        public int NonQuery()
        {
            int rows = 0;
            string storedProcedure = String.Empty;
            IList<SqliteParameter> parms = new List<SqliteParameter>();

            storedProcedure = Settings.Database.StoredProcedures.FirstOrDefault(p => p == NONQUERY_PROC);
            if (storedProcedure == null)
            {
                SqlCommandType = Constants.DBCommandType.SQL;
                CMDText = NONQUERY_TEST;
                rows = base.ExecNonQuery(parms);
            }
            else
            {
                CMDText = storedProcedure;
                SqlCommandType = Constants.DBCommandType.SPROC;
                rows = base.ExecNonQuery(parms);
            }
            return rows;
        }
        #endregion

        #region ExecStoredProc
        public int StoredProc(int id)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
