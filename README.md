# SQLite3Repository

A reference application written in C# .NET 10 which implements the Repository pattern and uses ADO.NET (`Microsoft.Data.Sqlite`) to access a SQLite3 database.
Regression testing of queries and transactions is performed using xUnit v3.

It is the SQLite3 companion to SQLRepositoryAsync (SQL Server). Because SQLite3 is an embedded, in-process database, the API here is synchronous (see [Why synchronous?](#why-synchronous)).

## Features

* Implements the Repository Pattern
* Supports Unit of Work, using one `SqliteTransaction` per unit of work
* Supports Paging (`LIMIT`/`OFFSET`, parameterized)
* Supports Sorting through the pager, with allowlisted sort columns
* Supports queries against views that return related entities
* Supports queries that return JSON
* Supports queries that don't return a value
* Soft deletes: `Delete` sets `Active = 0`, and every read excludes inactive rows
* Database errors are logged and re-thrown, rather than returned as `null`, `0`, or empty results
* Regression tests (query and transaction) via xUnit

## Solution

Project | Owns
------- | ----
`Data` | Interfaces (`IRepository`, `IUOW`, `IDBContext`, `IPager`), `PrimaryKey`, and mapper base classes
`POCO` | Entities (`Contact`, `City`, `State`), their mappers, and configuration classes
`Repository` | `RepositoryBase`, the entity repositories, `DBContext`, and `UnitOfWork`
`Regression.Test` | xUnit integration tests against `Contacts.db3`
`SQL` | Table and view scripts, seed data (`backup/*.txt`), and `Reload-Tables.ps1`

## API

#### Find All

```C#
ICollection<TEntity> FindAll()
```

Returns all active rows, ordered by `Id`.

#### Find All with Pager

```C#
IPager<TEntity> FindAll(IPager<TEntity> pager)
```

Returns one page of active rows and sets `pager.RowCount` to the total number of active rows.

* `PageNbr` is zero-based and `PageSize` must be greater than zero (default: page 0, 20 rows)
* `SortColumn` and `Direction` (`SQLOrderBy.ASC` or `DESC`) set the order (default: `Id` ASC)
* Sort columns are allowlisted by each repository. An unknown column falls back to `Id`, so caller text is never placed in the SQL.

```C#
IPager<State> page = stateRepos.FindAll(new Pager<State>() { PageNbr = 0, PageSize = 10, SortColumn = "Name", Direction = SQLOrderBy.DESC });
```

`ContactRepository` and `CityRepository` also provide `FindAllView()` and `FindAllView(IPager)`, which read from views and populate the related `City` and `State`. `ContactRepository.FindViewByPK` does the same for one row.

#### Find By Primary Key

```C#
TEntity FindByPK(IPrimaryKey pk)
```

Returns one active row, or `null` when no matching row exists.

#### Add

```C#
object Add(TEntity entity)
```

Adds a row. Returns the new identity key (`int`) for identity keys (`Contact`, `City`), or the natural key for natural keys (`State`).

#### Update

```C#
int Update(TEntity entity)
```

Updates an active row and returns the number of affected rows.

#### Delete

```C#
int Delete(PrimaryKey pk)
```

Soft-deletes a row by setting `Active = 0` and returns the number of affected rows.

#### Unit of Work

```C#
bool Save()
bool Rollback()
```

Pass a `UnitOfWork` to a repository's constructor to group writes. The first write begins a deferred `SqliteTransaction`, and later writes join it. Reads made through a repository in the same unit of work see its uncommitted writes.

* `Save()` commits the transaction once, and `Rollback()` rolls it back once. Both return `true` when there is nothing to do, so repeated calls are harmless.
* The unit of work must use the same `DBContext` as its repositories; otherwise the repository constructor throws `ArgumentException`.
* Disposing the `DBContext` without calling `Save()` discards the uncommitted work.
* A `DBContext`, its repositories, and its unit of work are not thread-safe. Use one per thread or request.

```C#
using (DBContext db = new DBContext(settings.Database.ConnectionString, logger))
{
    UnitOfWork uow = new UnitOfWork(db, logger);
    ContactRepository repos = new ContactRepository(settings, logger, uow, db);

    repos.Update(contact);
    repos.Add(newContact);
    uow.Save();
}
```

#### Errors

* SQL errors are logged and re-thrown as `SqliteException`.
* When the database can't be opened (or the unit of work can't start its transaction), the repository throws `InvalidOperationException`.
* `FindAll(IPager)` throws `ArgumentOutOfRangeException` for a `PageSize` less than 1 or a negative `PageNbr`.

## Running the Regression Tests

Requirements: .NET 10 SDK and the [SQLite3 command-line shell](https://www.sqlite.org/download.html) (`sqlite3.exe`).

```powershell
# 1. Create Regression.Test\Contacts.db3 (destructive: deletes and recreates the database)
.\SQL\Reload-Tables.ps1          # -Sqlite3 <path> if sqlite3.exe isn't in C:\Program Files\SQLite3\Tools or on PATH

# 2. Build (copies a fresh Contacts.db3 to the test output directory) and test
dotnet build .\SQLite3Repository.sln -c Debug
dotnet test --project .\Regression.Test\Regression.Test.csproj -c Debug --no-build
```

* `global.json` opts `dotnet test` into Microsoft.Testing.Platform, which xUnit v3 requires on the .NET 10 SDK. It does not pin an SDK version.
* Run one class or test with `--filter-class Regression.Test.QueryTests` or `--filter-method Regression.Test.TransactionTests.SaveTest`.
* `RunRegressionTest.ps1` writes `TestResults\TestResult.xml`, which `XUnitSummary.ps1` summarizes.
* Some transaction tests change seed rows. Rebuild, or rerun `Reload-Tables.ps1` and rebuild, before rerunning them.
* The connection string uses `Mode=ReadWrite`, so a missing database file fails the tests instead of silently creating an empty database.

## SQLite3 Notes

1. SQLite3 has five storage classes (NULL, INTEGER, REAL, TEXT, and BLOB) and no separate Boolean or DateTime types
1. Booleans are stored as INTEGER (0 = false; 1 = true)
1. Dates and times can be stored as TEXT (ISO 8601), REAL (Julian day numbers), or INTEGER (Unix time). This project uses INTEGER seconds since 1970-01-01 00:00:00 UTC.
1. Microsoft.Data.Sqlite requires `DBNull.Value`, rather than `null`, for NULL parameter values
1. An INSERT into a table with an INTEGER PRIMARY KEY is followed by `SELECT last_insert_rowid();` to return the new key (as a 64-bit integer)
1. Pages of data are returned with the LIMIT and OFFSET keywords: `SELECT Id FROM [table] ORDER BY Id LIMIT @pageSize OFFSET @offset;`
1. SQLite3 does NOT support stored procedures, so all commands are parameterized SQL
1. SQLite3 has a command-line shell (sqlite3.exe), which provides bcp/sqlcmd type functionality
1. SQLite3 supports in-memory databases (`Data Source=:memory:`)

### SQLite DateTime and POCOs

1. Dates are stored as INTEGER Unix time and converted to UTC `DateTime` values when they're read (`Constants.FromUnixTime`)
1. You don't need to set ModifiedDt or CreateDt in the POCO. They are set by the repository SQL during Update and Add:
    * Update: `UPDATE [table] SET ModifiedDt=strftime('%s','now');`
    * Add: `INSERT INTO [table] (ModifiedDt,CreateDt) VALUES(strftime('%s','now'),strftime('%s','now'));`
1. JSON queries format dates as ISO 8601 UTC (`strftime('%Y-%m-%dT%H:%M:%SZ', ModifiedDt, 'unixepoch')`), so they deserialize directly to `DateTime`

### Why synchronous?

Microsoft.Data.Sqlite's async methods execute synchronously, because SQLite3 has no asynchronous I/O. A `Task`-based API would add overhead without freeing threads, so this repository keeps a synchronous API. For concurrent readers alongside a writer, consider [Write-Ahead Logging (WAL)](https://www.sqlite.org/wal.html).

## Updates

- Projects target .NET 10, with updated packages and xUnit v3
- Repository operations take their SQL and mappers as arguments, rather than through shared mutable properties
- The unit of work uses a managed `SqliteTransaction`, and its terminal calls are idempotent
- Database and mapping errors are propagated rather than swallowed
- Paging is parameterized, with allowlisted sorting through the pager
- Stored-procedure branches were removed, because SQLite3 doesn't support stored procedures
- `Reload-Tables.ps1` runs from any directory, creates the views, and stops on errors

## Links

  *  [Repository Pattern](https://www.martinfowler.com/eaaCatalog/repository.html)
  *  [Microsoft.Data.Sqlite Overview](https://learn.microsoft.com/dotnet/standard/data/sqlite/)
  *  [Microsoft.Data.Sqlite Async limitations](https://learn.microsoft.com/dotnet/standard/data/sqlite/async)
  *  [Microsoft.Data.Sqlite Transactions](https://learn.microsoft.com/dotnet/standard/data/sqlite/transactions)
  *  [Datatypes in SQLite 3](https://www.sqlite.org/datatype3.html)
  *  [SQLite3 Date and Time Functions](https://www.sqlite.org/lang_datefunc.html)
  *  [SQLite3 Write-Ahead Logging (WAL)](https://www.sqlite.org/wal.html)
  *  [SQLite3 Command-line Shell](https://www.sqlite.org/cli.html)
  *  [SQLite3 In-memory databases](https://learn.microsoft.com/dotnet/standard/data/sqlite/in-memory-databases)
  *  [SQLite3 Downloads](https://www.sqlite.org/download.html)
