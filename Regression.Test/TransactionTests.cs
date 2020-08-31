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
        public void UOWConnectionTest()
        {
            //You can also use a 'using' statement block
            using (DBContext db = new DBContext(settings.Database.ConnectionString, logger))
            {
                Assert.NotNull(db);
                UnitOfWork uow = new UnitOfWork(db, logger);
                Assert.True(db.Open());
                Assert.True(db.Connection.State == ConnectionState.Open);
            }
        }

        [Fact]
        public void UpdateTest()
        {
            string oldString = String.Empty;
            string updateString = String.Empty;
            int rows = 0;

            using (DBContext db = new DBContext(settings.Database.ConnectionString, logger))
            {
                ContactRepository contactRepos = new ContactRepository(settings, logger, db);
                CityRepository cityRepos = new CityRepository(settings, logger, db);
                StateRepository stateRepos = new StateRepository(settings, logger, db);

                #region Update Contact Test
                oldString = "No notes";
                updateString = "Updated note.";
                Contact contact = contactRepos.FindByPK(new PrimaryKey() { Key = 1 });
                Assert.NotNull(contact);
                Assert.Equal(contact.Notes, oldString);
                contact.Notes = updateString;
                rows = contactRepos.Update(contact);
                Assert.Equal(1, rows);
                contact = contactRepos.FindByPK(new PrimaryKey() { Key = 1 });
                Assert.Equal(contact.Notes, updateString);
                #endregion

                #region Update City Test
                oldString = "Tampa";
                updateString = "Tampa(Updated)";
                City city = cityRepos.FindByPK(new PrimaryKey() { Key = 1 });
                Assert.NotNull(city);
                Assert.Equal(city.Name, oldString);
                city.Name = updateString;
                rows = cityRepos.Update(city);
                Assert.Equal(1, rows);
                city = cityRepos.FindByPK(new PrimaryKey() { Key = 1 });
                Assert.Equal(city.Name, updateString);
                #endregion

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
                ContactRepository contactRepos = new ContactRepository(settings, logger, db);
                CityRepository cityRepos = new CityRepository(settings, logger, db);
                StateRepository stateRepos = new StateRepository(settings, logger, db);

                #region Add Contact Test
                Contact contact = new Contact()
                {
                    FirstName = "New",
                    LastName = "User",
                    Address1 = "Address1",
                    Address2 = "Address2",
                    CellPhone = "8005551212",
                    HomePhone = "8005551212",
                    WorkPhone = "8005551212",
                    Notes = String.Empty,
                    ZipCode = "99999",
                    EMail = "NewUser@Mail.com",
                    CityId = 1
                };
                ICollection<Contact> contacts = contactRepos.FindAll();
                Assert.Null(contacts.Where(c => c.LastName == contact.LastName && c.FirstName == contact.FirstName).FirstOrDefault());
                key = (int)contactRepos.Add(contact);
                Assert.True(key > 0);
                Assert.NotNull(contactRepos.FindByPK(new PrimaryKey() { Key = key }));
                #endregion

                #region Add City Test
                City city = new City()
                {                    
                    Name = "New City",
                    StateId = "FL"
                };
                ICollection<City> cities = cityRepos.FindAll();
                Assert.Null(cities.Where(c => c.Name == city.Name).FirstOrDefault());
                key = (int)cityRepos.Add(city);
                Assert.True(key > 0);
                Assert.NotNull(cityRepos.FindByPK(new PrimaryKey() { Key = key }));
                #endregion

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
                ContactRepository contactRepos = new ContactRepository(settings, logger, db);
                CityRepository cityRepos = new CityRepository(settings, logger, db);
                StateRepository stateRepos = new StateRepository(settings, logger, db);

                #region Delete Contact Test
                Contact contact = contactRepos.FindByPK(new PrimaryKey() { Key = 8 });
                Assert.NotNull(contact);
                rows = contactRepos.Delete(new PrimaryKey() { Key = 8 });
                Assert.Equal(1, rows);
                contact = contactRepos.FindByPK(new PrimaryKey() { Key = 8 });
                Assert.Null(contact);
                #endregion

                #region Delete City Test
                City city = cityRepos.FindByPK(new PrimaryKey() { Key = 17 });
                Assert.NotNull(city);
                rows = cityRepos.Delete(new PrimaryKey() { Key = 17 });
                Assert.Equal(1, rows);
                city = cityRepos.FindByPK(new PrimaryKey() { Key = 17 });
                Assert.Null(city);
                #endregion

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

        [Fact]
        public void SaveTest()
        {
            string updateString = "Save this update.";
            Contact newContact = new Contact()
            {
                FirstName = "New",
                LastName = "SaveUser",
                Address1 = "Address1",
                Address2 = "Address2",
                CellPhone = "8005551212",
                HomePhone = "8005551212",
                WorkPhone = "8005551212",
                Notes = String.Empty,
                ZipCode = "99999",
                EMail = "NewSaveUser@Mail.com",
                CityId = 1
            };

            using (DBContext db = new DBContext(settings.Database.ConnectionString, logger))
            {
                Assert.NotNull(db);
                UnitOfWork uow = new UnitOfWork(db, logger);
                ContactRepository repos = new ContactRepository(settings, logger, uow, db);

                Contact contact = repos.FindByPK(new PrimaryKey() { Key = 11 });
                contact.Notes = updateString;
                int rows = repos.Update(contact);
                Assert.Equal(1, rows);
                ICollection<Contact> contacts = repos.FindAll();
                Assert.Null(contacts.Where(c => c.LastName == newContact.LastName && c.FirstName == newContact.FirstName).FirstOrDefault());
                int key = (int)repos.Add(newContact);
                Assert.True(uow.Save());
                contact = repos.FindByPK(new PrimaryKey() { Key = 11 });
                Assert.Equal(contact.Notes, updateString);
                Assert.True(key > 0);
                Assert.NotNull(repos.FindByPK(new PrimaryKey() { Key = key }));

            }
        }

        [Fact]
        public void RollBackTest()
        {
            string updateString = "Rollback this update.";
            string oldNotes = String.Empty;

            using (DBContext db = new DBContext(settings.Database.ConnectionString, logger))
            {
                Assert.NotNull(db);
                UnitOfWork uow = new UnitOfWork(db, logger);
                ContactRepository repos = new ContactRepository(settings, logger, uow, db);

                Contact contact = repos.FindByPK(new PrimaryKey() { Key = 11 });
                oldNotes = contact.Notes;
                contact.Notes = updateString;
                int rows = repos.Update(contact);
                Assert.Equal(1, rows);
                Assert.True(uow.Rollback());
                contact = repos.FindByPK(new PrimaryKey() { Key = 11 });
                Assert.Equal(contact.Notes, oldNotes);
            }
        }
    }
}
