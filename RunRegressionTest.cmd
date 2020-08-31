dotnet build /p:Platform="Any CPU"
dotnet vstest .\Regression.Test\bin\Debug\netcoreapp3.1\Regression.Test.dll --logger:trx;LogFileName=TestResult.xml
pause