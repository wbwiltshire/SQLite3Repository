using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data.Interfaces;

namespace SQLite3Repository.Data.Repository
{

    public class DBContext : IDBContext
    {
        private readonly ILogger logger;
        private readonly SqliteConnection connection;

        // ctor
        public DBContext(string connectionString, ILogger l)
        {
            logger = l;
            //Throws ArgumentException for an invalid connection string
            connection = new SqliteConnection(connectionString);
            logger.LogInformation("DBContext created for data source {DataSource}.", connection.DataSource);
        }

        public SqliteConnection Connection
        {
            get { return connection; }
        }

        public bool Open()
        {
            //Only open the connection, if it isn't already open
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                    logger.LogInformation("DBContext connection opened.");
                }
                catch (SqliteException ex)
                {
                    logger.LogError(ex, "Unable to open DBContext connection.");
                }
            }
            else
                logger.LogInformation("Using open DBContext connection.");

            return connection.State == ConnectionState.Open;
        }

        public void Close()
        {
            try
            {
                connection.Close();
                logger.LogInformation("DBContext connection closed.");
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex, "Unable to close DBContext connection.");
            }
        }

        #region Dispose
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //Closing the connection rolls back any uncommitted transaction
                    connection.Dispose();
                    logger.LogInformation("DBContext connection closed via dispose.");
                }
                this.disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
