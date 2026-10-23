using System;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data.Interfaces;

namespace SQLite3Repository.Data.Repository
{
	public class UnitOfWork : IUOW
	{
        private readonly ILogger logger;
        private readonly DBContext dbc;
        private SqliteTransaction transaction;

        internal SqliteTransaction Transaction => transaction;
        internal DBContext Context => dbc;

        public UnitOfWork(DBContext d, ILogger l)
        {
            logger = l;
            dbc = d;
        }

        //Begin the transaction on the first enlisted operation; later operations share it
        public bool Enlist()
        {
            if (transaction is not null)
                return true;

            try
            {
                if (!dbc.Open())
                    return false;

                transaction = dbc.Connection.BeginTransaction(deferred: true);
                logger.LogInformation("Unit of work transaction started.");
                return true;
            }
            catch (Exception ex) when (ex is SqliteException || ex is InvalidOperationException)
            {
                logger.LogError(ex, "Unable to start unit of work transaction.");
                return false;
            }
        }

        public bool Save()
        {
            if (transaction is null)
            {
                logger.LogInformation("Save ignored, because no transactions have enlisted.");
                return true;
            }

            try
            {
                transaction.Commit();
                logger.LogInformation("Save complete and unit of work committed.");
                return true;
            }
            catch (Exception ex) when (ex is SqliteException || ex is InvalidOperationException)
            {
                logger.LogError(ex, "Unable to commit unit of work transaction.");
                return false;
            }
            finally
            {
                //A failed commit is rolled back by Dispose
                transaction.Dispose();
                transaction = null;
            }
        }

        public bool Rollback()
        {
            if (transaction is null)
            {
                logger.LogInformation("Rollback ignored, because no transactions have enlisted.");
                return true;
            }

            try
            {
                transaction.Rollback();
                logger.LogInformation("Rollback complete and unit of work cleared.");
                return true;
            }
            catch (Exception ex) when (ex is SqliteException || ex is InvalidOperationException)
            {
                logger.LogError(ex, "Unable to roll back unit of work transaction.");
                return false;
            }
            finally
            {
                transaction.Dispose();
                transaction = null;
            }
        }
    }
}
