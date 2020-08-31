using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data.Interfaces;
using SQLite3Repository.Data.POCO;

namespace SQLite3Repository.Data.Repository
{
	public class UnitOfWork : IUOW
	{
        private readonly ILogger logger;
        private DBContext dbc;
        private string CMDText;
        private int transactionCount;

        public UnitOfWork(DBContext d, ILogger l)
        {
            logger = l;
            transactionCount = 0;
            dbc = d;
        }

        public bool Enlist()
        {
            string CMDText = "BEGIN DEFERRED TRANSACTION;";
            int rows;

            transactionCount++;
            //Begin transaction, if first time
            if (transactionCount == 1)
            {
                try
                {
                    using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                    {
                        rows = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqliteException ex)
                {
                    logger.LogError(ex.Message);
                }

            }
            return true;
        }

        public bool Save()
        {
            CMDText = "COMMIT TRANSACTION;";
            bool status = false;
            int rows;

            //TODO: Do I need to check connection state here?
            //ANSWER: No, if we don't have an open connection, we have bigger problems
            //if (dbc.Connection.State != ConnectionState.Open)
            //    await dbc.Open();

            //Nothing to do if no transactions have enlisted
            if (transactionCount > 0)
            {
                try
                {
                    using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                    {
                        rows = cmd.ExecuteNonQuery();
                        status = true;
                        transactionCount = 0;
                        logger.LogInformation("Save complete and unit of work committed.");
                    }
                }
                catch (SqliteException ex)
                {
                    logger.LogError(ex.Message);
                }
            }
            else
            {
                status = true;
                logger.LogInformation("Save ignored, because no transactions have enlisted.");
            }
            return status;
        }

        public bool Rollback()
        {
            CMDText = "ROLLBACK TRANSACTION;";
            bool status = false;
            int rows;

            //TODO: Do I need to check connection state here?
            //ANSWER: No, if we don't have an open connection, we have bigger problems
            //if (dbc.Connection.State != ConnectionState.Open)
            //    await dbc.Open();

            if (transactionCount > 0)
            {
                try
                {
                    using (SqliteCommand cmd = new SqliteCommand(CMDText, dbc.Connection))
                    {
                        rows = cmd.ExecuteNonQuery();
                        status = true;
                        transactionCount = 0;
                        logger.LogInformation("Rollback complete and unit of work cleared.");
                    }
                }
                catch (SqliteException ex)
                {
                    logger.LogError(ex.Message);
                }
            }
            else
            {
                status = true;
                logger.LogInformation("Rollback ignored, because no transactions have enlisted.");
            }
            return status;
        }
    }
}
