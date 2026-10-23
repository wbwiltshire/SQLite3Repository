using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.POCO;
using SQLite3Repository.Data.Repository;
using Xunit;

namespace Regression.Test
{
	[Collection("Test Collection")]
	public class QueryTests
	{
		private SetupFixture fixture;
		private ILogger logger;
		private AppSettingsConfiguration settings;

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
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				Assert.True(dbc.Open());
				Assert.True(dbc.Connection.State == ConnectionState.Open);
			}
		}

		[Fact]
		public void FindAllTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				ContactRepository contactRepos = new ContactRepository(settings, logger, dbc);
				CityRepository cityRepos = new CityRepository(settings, logger, dbc);
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);

				ICollection<Contact> contacts = contactRepos.FindAll();
				Assert.NotEmpty(contacts);
				Assert.Equal(contacts.Select(c => c.Id).OrderBy(id => id), contacts.Select(c => c.Id));
				ICollection<City> cities = cityRepos.FindAll();
				Assert.NotEmpty(cities);
				Assert.Equal(cities.Select(c => c.Id).OrderBy(id => id), cities.Select(c => c.Id));
				ICollection<State> states = stateRepos.FindAll();
				Assert.NotEmpty(states);
				Assert.Equal(states.Select(s => s.Id).OrderBy(id => id, StringComparer.Ordinal), states.Select(s => s.Id));
			}
		}

		[Fact]
		public void FindAllPagedTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				ContactRepository contactRepos = new ContactRepository(settings, logger, dbc);
				CityRepository cityRepos = new CityRepository(settings, logger, dbc);
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);

				IPager<Contact> contacts = contactRepos.FindAll(new Pager<Contact>() { PageNbr = 1, PageSize = 5 });
				Assert.NotNull(contacts.Entities);
				Assert.Equal(5, contacts.Entities.Count);
				Assert.True(contacts.RowCount > 5);
				IPager<City> cities = cityRepos.FindAll(new Pager<City>() { PageNbr = 1, PageSize = 5 });
				Assert.NotNull(cities.Entities);
				Assert.Equal(5, cities.Entities.Count);
				Assert.True(cities.RowCount > 5);
				IPager<State> states = stateRepos.FindAll(new Pager<State>() { PageNbr = 1, PageSize = 5 });
				Assert.NotNull(states.Entities);
				Assert.Equal(5, states.Entities.Count);
				Assert.True(states.RowCount > 5);
			}
		}

		[Fact]
		public void FindAllPagedPagesDoNotOverlapTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				CityRepository cityRepos = new CityRepository(settings, logger, dbc);

				// PageNbr is zero-based
				IPager<City> first = cityRepos.FindAll(new Pager<City>() { PageNbr = 0, PageSize = 5 });
				IPager<City> second = cityRepos.FindAll(new Pager<City>() { PageNbr = 1, PageSize = 5 });
				Assert.Equal(5, first.Entities.Count);
				Assert.Equal(5, second.Entities.Count);
				Assert.Empty(first.Entities.Select(c => c.Id).Intersect(second.Entities.Select(c => c.Id)));
				Assert.True(first.Entities.Max(c => c.Id) < second.Entities.Min(c => c.Id));
			}
		}

		[Fact]
		public void FindAllPagedSortedTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);

				IPager<State> states = stateRepos.FindAll(new Pager<State>() { PageNbr = 0, PageSize = 100, SortColumn = "Name", Direction = SQLOrderBy.DESC });
				List<string> names = states.Entities.Select(s => s.Name).ToList();
				Assert.True(names.Count > 1);
				Assert.Equal(names.OrderByDescending(n => n, StringComparer.Ordinal), names);
			}
		}

		[Fact]
		public void FindAllPagedUnknownSortColumnFallsBackToIdTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);

				// Sort columns are allowlisted, so caller text is never interpolated into SQL
				IPager<State> states = stateRepos.FindAll(new Pager<State>() { PageNbr = 0, PageSize = 100, SortColumn = "Name; DROP TABLE State" });
				List<string> ids = states.Entities.Select(s => s.Id).ToList();
				Assert.True(ids.Count > 1);
				Assert.Equal(ids.OrderBy(id => id, StringComparer.Ordinal), ids);
				Assert.NotEmpty(stateRepos.FindAll());
			}
		}

		[Fact]
		public void FindAllPagedInvalidPagerTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);

				Assert.Throws<ArgumentOutOfRangeException>(() => stateRepos.FindAll(new Pager<State>() { PageNbr = 0, PageSize = 0 }));
				Assert.Throws<ArgumentOutOfRangeException>(() => stateRepos.FindAll(new Pager<State>() { PageNbr = -1, PageSize = 5 }));
			}
		}

		[Fact]
		public void FindAllViewTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				ContactRepository contactRepos = new ContactRepository(settings, logger, dbc);
				CityRepository cityRepos = new CityRepository(settings, logger, dbc);

				ICollection<Contact> contacts = contactRepos.FindAllView();
				Assert.NotEmpty(contacts);
				Assert.NotNull(contacts.First().City);
				Assert.NotNull(contacts.First().City.State);
				Assert.Equal(contacts.First().City.StateId, contacts.First().City.State.Id);
				ICollection<City> cities = cityRepos.FindAllView();
				Assert.NotEmpty(cities);
				Assert.NotNull(cities.First().State);
				Assert.Equal(cities.First().StateId, cities.First().State.Id);
			}
		}

		[Fact]
		public void FindAllViewPagedTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				ContactRepository contactRepos = new ContactRepository(settings, logger, dbc);
				CityRepository cityRepos = new CityRepository(settings, logger, dbc);

				IPager<Contact> contacts = contactRepos.FindAllView(new Pager<Contact>() { PageNbr = 1, PageSize = 5 });
				Assert.Equal(5, contacts.Entities.Count);
				Assert.Equal(contactRepos.FindAllView().Count, contacts.RowCount);
				Assert.NotNull(contacts.Entities.First().City);
				Assert.NotNull(contacts.Entities.First().City.State);
				IPager<City> cities = cityRepos.FindAllView(new Pager<City>() { PageNbr = 1, PageSize = 5 });
				Assert.Equal(5, cities.Entities.Count);
				Assert.Equal(cityRepos.FindAllView().Count, cities.RowCount);
				Assert.NotNull(cities.Entities.First().State);
			}
		}

		[Fact]
		public void FindByPKAlphaTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				StateRepository repos = new StateRepository(settings, logger, dbc);

				State state = repos.FindByPK(new PrimaryKey() { Key = "FL" });
				Assert.NotNull(state);
				Assert.Null(repos.FindByPK(new PrimaryKey() { Key = "NotAState" }));
			}
		}

		[Fact]
		public void FindByPKNumericTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				ContactRepository contactRepos = new ContactRepository(settings, logger, dbc);
				CityRepository cityRepos = new CityRepository(settings, logger, dbc);

				Contact contact = contactRepos.FindByPK(new PrimaryKey() { Key = 1 });
				Assert.NotNull(contact);
				City city = cityRepos.FindByPK(new PrimaryKey() { Key = 1 });
				Assert.NotNull(city);
				Assert.Null(cityRepos.FindByPK(new PrimaryKey() { Key = -1 }));
			}
		}

		[Fact]
		public void FindByPKViewTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				ContactRepository repos = new ContactRepository(settings, logger, dbc);

				Contact contact = repos.FindViewByPK(new PrimaryKey() { Key = 1 });
				Assert.NotNull(contact);
				Assert.True(contact.Id == 1);
				Assert.NotNull(contact.City);
				Assert.NotNull(contact.City.State);
			}
		}

		[Fact]
		public void ExecNonQueryTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				ContactRepository repos = new ContactRepository(settings, logger, dbc);

				Assert.True(repos.NonQuery() > 1);
			}
		}

		[Fact]
		public void ExecJSONQueryTest()
		{
			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);

				string stateJson = stateRepos.GetJSON();
				List<State> states = JsonSerializer.Deserialize<List<State>>(stateJson);
				ICollection<State> activeStates = stateRepos.FindAll();
				Assert.NotNull(states);
				// JSON only includes active rows, like the other reads
				Assert.Equal(activeStates.Select(s => s.Id), states.Select(s => s.Id));
				Assert.Equal(activeStates.First().CreateDt, states.First().CreateDt.ToUniversalTime());
			}
		}

		[Fact]
		public void DateTimeTest()
		{
			// State data was created on 8/30/20 04:43:33 UTC (Unix time 1598762613)
			DateTime createDt = new DateTime(2020, 8, 30, 4, 43, 33, DateTimeKind.Utc);

			using (DBContext dbc = new DBContext(settings.Database.ConnectionString, logger))
			{
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);
				State state = stateRepos.FindByPK(new PrimaryKey() { Key = "FL", IsIdentity = false });
				Assert.NotNull(state);

				Assert.Equal(createDt, state.CreateDt);
				Assert.Equal(DateTimeKind.Utc, state.CreateDt.Kind);
				Assert.Contains(stateRepos.FindAll(), s => s.CreateDt == createDt);
			}
		}

		[Fact]
		public void SqlFailureIsPropagatedTest()
		{
			// An empty in-memory database has none of the tables, so the query fails
			using (DBContext dbc = new DBContext("Data Source=:memory:", logger))
			{
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);

				Assert.Throws<SqliteException>(() => stateRepos.FindAll());
			}
		}

		[Fact]
		public void MissingDatabaseIsNotCreatedTest()
		{
			string missingFile = $"Missing-{Guid.NewGuid():N}.db3";

			// Mode=ReadWrite fails, rather than silently creating an empty database
			using (DBContext dbc = new DBContext($"Data Source={missingFile};Mode=ReadWrite", logger))
			{
				StateRepository stateRepos = new StateRepository(settings, logger, dbc);

				Assert.False(dbc.Open());
				Assert.Throws<InvalidOperationException>(() => stateRepos.FindAll());
			}
			Assert.False(System.IO.File.Exists(missingFile));
		}

		[Fact]
		public void PrimaryKeyToStringTest()
		{
			PrimaryKey pk = new PrimaryKey() { CompositeKey = new object[] { 1, 2 }, IsComposite = true };

			Assert.Equal("|1|2|True", pk.ToString());
			Assert.Equal(pk.ToString(), pk.ToString());
			Assert.Equal("|FL|False", new PrimaryKey() { Key = "FL" }.ToString());
		}

	}
}
