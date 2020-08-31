using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SQLite3Repository.Data;
using SQLite3Repository.Data.POCO;
using SQLite3Repository.Data.Repository;
using Xunit;

namespace Regression.Test
{
	[Collection("Test Collection")]
	public class TransactionTests
	{
        private SetupFixture fixture;
        private ILogger logger;
        private AppSettingsConfiguration settings;

        public TransactionTests(SetupFixture f)
        {
            fixture = f;
            logger = f.Logger;
            settings = f.Settings;
        }

        [Fact]
        public void UpdateTest()
        {
            string oldString = String.Empty;
            string updateString = String.Empty;
            int rows = 0;

            using (DBContext db = new DBContext(settings.Database.ConnectionString, logger))
            {
                StateRepository stateRepos = new StateRepository(settings, logger, db);

                #region Update State Test
                oldString = "NA";
                updateString = "NA(Updated)";
                State state = stateRepos.FindByPK(new PrimaryKey() { Key = "00" });
                Assert.NotNull(state);
                Assert.Equal(state.Name, oldString);
                state.Name = updateString;
                rows = stateRepos.Update(state);
                Assert.Equal(1, rows);
                state = stateRepos.FindByPK(new PrimaryKey() { Key = "00" });
                Assert.Equal(state.Name, updateString);
                #endregion
            }
        }

        [Fact]
        public void AddTest()
        {
            int key = 0;
            string naturalKey = String.Empty;

            using (DBContext db = new DBContext(settings.Database.ConnectionString, logger))
            {
                Assert.NotNull(db);
                //ContactRepository contactRepos = new ContactRepository(settings, logger, db);
                //CityRepository cityRepos = new CityRepository(settings, logger, db);
                StateRepository stateRepos = new StateRepository(settings, logger, db);

                #region Add State Test
                State newState = new State()
                {
                    Id = "ZZ",
                    Name = "New State"
                };
                State state = stateRepos.FindByPK(new PrimaryKey() { Key = newState.Id, IsIdentity = false });
                Assert.Null(state);
                naturalKey = (string)stateRepos.Add(newState);
                Assert.True(naturalKey == (string)newState.PK.Key);
                Assert.NotNull(stateRepos.FindByPK(new PrimaryKey() { Key = newState.Id, IsIdentity = false }));
                #endregion

            }
        }

        [Fact]
        public void DeleteTest()
        {
            int rows = 0;

            using (DBContext db = new DBContext(settings.Database.ConnectionString, logger))
            {
                Assert.NotNull(db);
                //ContactRepository contactRepos = new ContactRepository(settings, logger, db);
                //CityRepository cityRepos = new CityRepository(settings, logger, db);
                StateRepository stateRepos = new StateRepository(settings, logger, db);

                #region Delete State Test
                State state = stateRepos.FindByPK(new PrimaryKey() { Key = "WA" });
                Assert.NotNull(state);
                rows = stateRepos.Delete(new PrimaryKey() { Key = "WA" });
                Assert.Equal(1, rows);
                state = stateRepos.FindByPK(new PrimaryKey() { Key = "WA" });
                Assert.Null(state);
                #endregion
            }
        }
    }
}
