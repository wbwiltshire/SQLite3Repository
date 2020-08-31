using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data.Interfaces;

namespace SQLite3Repository.Data.Repository
{

    public class DBContext : IDBContext
    {
        private ILogger logger;
        private SqliteConnection connection = null;
        private bool isOpen = false;

        // ctor
        public DBContext(string connectionString, ILogger l)
        {
            logger = l;
            isOpen = false;
            try
            {
                connection = new SqliteConnection(connectionString);
                logger.LogInformation($"ConnectionString: {connectionString}");

            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);
            }
        }

        public SqliteConnection Connection
        {
            get { return connection; }
        }

        public bool Open()
        {
            //Only create a connection, if we don't already have one
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                    isOpen = true;
                    logger.LogInformation("DBContext connection opened.");
                }
                catch (SqliteException ex)
                {
                    logger.LogError(ex.Message);
                }
            }
            else
                logger.LogInformation("Using open DBContext connection.");

            return isOpen;
        }

        public void Close()
        {
            try
            {
                if (connection != null)
                {
                    connection.Close();
                    isOpen = false;
                    logger.LogInformation("DBConnection closed.");
                }
            }
            catch (SqliteException ex)
            {
                logger.LogError(ex.Message);

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
                    if (connection != null)
                        connection.Close();
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
