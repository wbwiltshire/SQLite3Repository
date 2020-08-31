SQLite3Repository
===

A sample application written in .Net Core (3.1) which uses ADO.NET and the Repository pattern to access a SQLite3 database  

Notes
---
1.  SQLite3 has only three data types (Text, Integer, and Real)
1.  Dates are represented as Integer days since November 24, 4714 BC (UTC)
1.  DateTimes are represented as Integer seconds since 1970-01-01 00:00:00 UTC
1.  Boolean is represented as Integer (0 = false; 1 = true)
1.  SQLite3 doesn't really support async.  Which is fine, since it's not really for client/server and multi-user access.  However, you can run it with WAL (see below)
1.  SQLite3 has a command-line shell program(sqlite3.exe), which provies bcp/sqlcmd type functionality 
1.  SQLite3 supports In-memory databases
1.  XUnit is used as the testing framework
1.  INSERT should be followed by SELECT last_insert_rowid() for tables with IDENTITY as their Primary Key
1.  Returning Pages of data is accomplished via the LIMIT and OFFSET keywords: SELECT Id FROM [table] LIMIT 10 OFFSET 0;
1.  SQLite3 does NOT support Stored Procedures

SQLite DateTime and POCOs
---
1. You store them as REAL and convert them to DateTime when you read them
1. You don't need to set ModifiedDt or CreateDt in the POCO.  These are set in the repository during Update and Add
1. Update: UPDATE [table] SET ModifiedDt=strftime('%s','now');
1. Add: INSERT INTO [table] (ModifiedDt,CreateDt) VALUES(strftime('%s','now'),strftime('%s','now'));

Links
---
  *  [Microsoft.Data.Sqlite Overview](https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite)
  *  [Datatypes in SQLite 3](https://www.sqlite.org/datatype3.html)
  *  [Async limitations](https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/async)
  *  [SQLite3 Write Ahead Logging(WAL)](https://www.sqlite.org/wal.html)
  *  [An Asynchronous I/O Module For SQLite3](https://www.sqlite.org/asyncvfs.html)
  *  [SQLite3 Command-line](https://www.sqlite.org/cli.html)
  *  [SQLite3 In-memory databases](https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases)
  *  [SQLite3 Command-line Shell download](https://www.sqlite.org/2020/sqlite-tools-win32-x86-3330000.zip)
  *  [SQLite3 Date and Time Functions](https://www.sqlite.org/lang_datefunc.html)
  *  [SQLite3 Dates Tutorial](https://www.sqlitetutorial.net/sqllite-date)
