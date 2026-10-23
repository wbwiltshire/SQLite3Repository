# SQLite3Repository Agent Instructions

## Project

- Reference C# / .NET repository-pattern implementation using synchronous ADO.NET and `Microsoft.Data.Sqlite` against a SQLite3 database file.
- Solution projects: [Data](Data), [POCO](POCO), [Repository](Repository), and [Regression.Test](Regression.Test).
- Public behavior and API intent are described in [README.md](README.md). Treat it as useful context, not a substitute for checking current signatures.
- The API is intentionally synchronous: Microsoft.Data.Sqlite async methods run synchronously. Do not convert it to `Task` contracts without user confirmation.

## Architecture

- `Data` owns interfaces and base contracts.
- `POCO` owns entity models and their mappers.
- `Repository` owns SQL access, paging, sorting, and unit-of-work behavior.
- `Regression.Test` owns SQLite3 integration tests and the database fixture.
- Preserve the dependency flow: `Regression.Test` may reference `Repository`, `POCO`, and `Data`; `Repository` references `POCO` and `Data`; `POCO` references `Data`.
- Repository implementations inherit `RepositoryBase<TEntity>` and implement `IRepository<TEntity>`. They pass SQL text and mappers to the protected `RepositoryBase` helpers as arguments; do not reintroduce shared mutable command state.
- Preserve primary-key behavior in `MapperBase` and `PrimaryKey`: identity, natural, and composite keys produce the repository's existing parameter names (`@pk`, `@pk1..n`, `@p1..n`) and SQL shape.
- Keep paging and sorting coupled through `IPager`. Paged SQL uses the `@orderBy` token plus `@pageSize`/`@offset` parameters; sort columns are allowlisted by each repository's `SortColumns` rather than interpolated directly.
- SQLite3 has no stored procedures. All commands are parameterized `CommandType.Text` SQL.
- Deletes are soft deletes: set `Active = 0`, and normal reads (including views and JSON) exclude inactive rows.
- Database errors are logged and re-thrown; do not return `null`, `0`, or empty results to hide failures.

## Commands

Run from the repository root:

```powershell
dotnet restore .\SQLite3Repository.sln
dotnet build .\SQLite3Repository.sln -c Debug
dotnet test --project .\Regression.Test\Regression.Test.csproj -c Debug --no-build
```

Use `dotnet test` to run a focused class or test (xUnit v3 / Microsoft.Testing.Platform filter options):

```powershell
dotnet test --project .\Regression.Test\Regression.Test.csproj -c Debug --no-build --filter-class Regression.Test.QueryTests
dotnet test --project .\Regression.Test\Regression.Test.csproj -c Debug --no-build --filter-method Regression.Test.TransactionTests.SaveTest
```

[global.json](global.json) only opts `dotnet test` into Microsoft.Testing.Platform (required for xUnit v3 on the .NET 10 SDK); it does not pin an SDK. There is no repository-wide formatter/analyzer policy. Do not change target frameworks or dependency versions unless the task explicitly requires it.

## Database and Tests

- Tests require `Regression.Test\Contacts.db3`, created from the seed data by [Reload-Tables.ps1](SQL/Reload-Tables.ps1). It is generated and ignored by git.
- `Reload-Tables.ps1` destructively deletes and recreates the database (tables, seed data, and views). Read it before running it. It runs from any directory and needs `sqlite3.exe` (`-Sqlite3 <path>` overrides the default location).
- The build copies `Contacts.db3` and [appsettings.json](Regression.Test/appsettings.json) to the test output directory, so each build restores the seed data. `--no-build` reruns use the already-mutated output copy.
- The connection string uses `Mode=ReadWrite`, so a missing database fails instead of creating an empty file.
- `QueryTests` and `TransactionTests` share one xUnit collection fixture. Avoid adding parallel stateful tests or changing the fixture to hide live database dependencies.
- `AddTest`, `UpdateTest`, `DeleteTest`, and `SaveTest` mutate seed records and fail on a rerun until the output database is restored. New tests should be rerunnable (roll back, or restore what they change).
- Update [TestMatrix.md](Regression.Test/TestMatrix.md) when adding or removing repository behavior coverage.
- [RunRegressionTest.ps1](RunRegressionTest.ps1) builds and runs the net10.0 test assembly with `dotnet vstest`, writing `TestResults\TestResult.xml` for [XUnitSummary.ps1](XUnitSummary.ps1). Prefer direct `dotnet test` commands.

## SQL and Safety

- Use parameterized SQL; do not concatenate untrusted values into command text. Microsoft.Data.Sqlite needs `DBNull.Value`, not `null`, for NULL parameters.
- Dates are INTEGER Unix seconds, set in SQL with `strftime('%s','now')` and read as UTC `DateTime` values.
- Create, execute, and dispose commands and readers in their owning scope.
- The unit of work uses one managed `SqliteTransaction` per instance on its `DBContext` connection, and repositories in that unit of work must share that `DBContext`. It is not concurrent-safe. Do not introduce parallel access to one repository, `DBContext`, or unit of work instance.

## Editing Conventions

- Keep changes narrowly scoped to the owning project and behavior.
- Match existing C# namespaces, class names, public synchronous contracts, and repository-specific SQL patterns.
- Add or update focused xUnit integration coverage for behavior changes.
- Do not add packages or change project targets without user confirmation.
- Verify changes with the smallest relevant build or test command; database-dependent failures should be reported separately from compile/test regressions.
- Leave generated `bin/`, `obj/`, `TestResults/`, and `*.db3` artifacts untouched and untracked.
