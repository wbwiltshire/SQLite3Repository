## Run SQL\Reload-Tables.ps1 once to create Regression.Test\Contacts.db3.
## The build copies Contacts.db3 to the test output directory, so each build restores the seed data.

dotnet build .\SQLite3Repository.sln /p:Platform="Any CPU"
dotnet vstest .\Regression.Test\bin\Debug\net10.0\Regression.Test.dll --logger:"trx;LogFileName=TestResult.xml"

## Run XUnitSummary.ps1 after the test run to generate a summary of the test results.
pause
