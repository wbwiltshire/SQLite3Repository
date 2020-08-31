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
using Newtonsoft.Json;

namespace Regression.Test
{
	[Collection("Test Collection")]
	public class QueryTests
	{
		private SetupFixture fixture;
		private ILogger logger;
		private AppSettingsConfiguration settings;
		private DBContext dbc;

		//Fixture instantiated at the beginning of all the tests in this class and passed to constructor
		public QueryTests(SetupFixture f)
		{
			fixture = f;
			logger = f.Logger;
			settings = f.Settings;
		}

		[Fact]
		public void ConnectionTest()
		{
			logger.LogInformation("Running Connection Regression Test");
			Assert.NotNull(dbc = new DBContext(settings.Database.ConnectionString, logger));
			Assert.True(dbc.Open());
			Assert.True(dbc.Connection.State == ConnectionState.Open);

			dbc.Close();
		}

		[Fact]
		public void FindAllTest()
		{
			Assert.NotNull(dbc = new DBContext(settings.Database.ConnectionString, logger));
			//ContactRepository contactRepos = new ContactRepository(settings, logger, dbc);
			//CityRepository cityRepos = new CityRepository(settings, logger, dbc);
			StateRepository stateRepos = new StateRepository(settings, logger, dbc);

			//ICollection<Contact> contacts = await contactRepos.FindAll();
			//Assert.NotEmpty(contacts);
			//ICollection<City> cities = await cityRepos.FindAll();
			//Assert.NotEmpty(cities);
			ICollection<State> states = stateRepos.FindAll();
			Assert.NotEmpty(states);

			dbc.Close();
		}

		[Fact]
		public void FindAllPagedTest()
		{
			Assert.NotNull(dbc = new DBContext(settings.Database.ConnectionString, logger));
			//ContactRepository contactRepos = new ContactRepository(settings, logger, dbc);
			//CityRepository cityRepos = new CityRepository(settings, logger, dbc);
			StateRepository stateRepos = new StateRepository(settings, logger, dbc);

			IPager<State> states = stateRepos.FindAll(new Pager<State>() { PageNbr = 1, PageSize = 5 });
			Assert.NotNull(states.Entities);
			Assert.True(states.Entities.Count == 5);
			Assert.True(states.RowCount > 0);
			Assert.NotNull(states.Entities);
		}

		[Fact]
		public void FindByPKAlphaTest()
		{
			Assert.NotNull(dbc = new DBContext(settings.Database.ConnectionString, logger));
			StateRepository repos = new StateRepository(settings, logger, dbc);

			State state = repos.FindByPK(new PrimaryKey() { Key = "FL" });
			Assert.NotNull(state);

			dbc.Close();
		}

		[Fact]
		public void DateTimeTest()
		{
			State state = null;
			ICollection<State> states = null;
			DateTime beginTestDate = new DateTime(2020, 8, 30, 0, 0, 0, 0);
			DateTime endTestDate = new DateTime(2020, 8, 30, 23, 59, 59, 999);

			Assert.NotNull(dbc = new DBContext(settings.Database.ConnectionString, logger));
			StateRepository stateRepos = new StateRepository(settings, logger, dbc);
			state = stateRepos.FindByPK(new PrimaryKey() { Key = "FL", IsIdentity = false });
			Assert.NotNull(state);

			// State data was created on 8/30/20 00:43:33
			Assert.True(state.CreateDt > beginTestDate && state.CreateDt < endTestDate );
			states = stateRepos.FindAll().Where(s => s.CreateDt > beginTestDate && s.CreateDt < endTestDate).ToList();
			Assert.NotNull(states);

		}

		// Helper methods
		public DateTime ToDateTime(double sqliteDate)
		{
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return dt.AddSeconds(sqliteDate).ToLocalTime();
		}

	}
}
