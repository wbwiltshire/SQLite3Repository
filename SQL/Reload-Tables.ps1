#############################################################################################################
# .SYNOPSIS
#  Recreate the regression test database (Contacts.db3)
#
# .DESCRIPTION
#  DESTRUCTIVE: deletes the database, then recreates the State, City, and Contact tables, loads the
#  seed data from the backup directory, and creates the views.
#  Rebuild the solution afterwards, so the new database is copied to the test output directory.
#
# .PARAMETER Database
#  Database file to recreate (default: ..\Regression.Test\Contacts.db3)
#
# .PARAMETER Sqlite3
#  Path to the SQLite3 command-line shell (default: C:\Program Files\SQLite3\Tools\sqlite3.exe, then PATH)
#
# .EXAMPLE
#  .\SQL\Reload-Tables.ps1
#############################################################################################################
Param (
    [string] $Database = (Join-Path $PSScriptRoot "..\Regression.Test\Contacts.db3"),
    [string] $Sqlite3 = "C:\Program Files\SQLite3\Tools\sqlite3.exe"
)
$ErrorActionPreference = "Stop"

if (-not (Test-Path -Path $Sqlite3)) {
    $Sqlite3 = (Get-Command sqlite3.exe).Source
}
$Database = [System.IO.Path]::GetFullPath($Database)

if (Test-Path -Path $Database) {
    Write-Host "Database exists, removing it"
    Remove-Item -Force $Database
}

Write-Host "Creating Database $Database"
# .import paths are relative to the sqlite3 working directory, so run from this script's directory
Push-Location $PSScriptRoot
try {
    $createCmd = @(
        Get-Content -Raw -Path .\CreateStateTable.sql
        ".import backup/State.txt State"
        Get-Content -Raw -Path .\CreateCityTable.sql
        ".import backup/City.txt City"
        Get-Content -Raw -Path .\CreateContactTable.sql
        ".import backup/Contact.txt Contact"
        Get-Content -Raw -Path .\CreateViews.sql
        ".quit"
    ) -join "`n"

    $createCmd | & $Sqlite3 -bail $Database
    if ($LASTEXITCODE -ne 0) {
        throw "sqlite3 failed with exit code $LASTEXITCODE"
    }

    & $Sqlite3 $Database "SELECT 'State: ' || COUNT(*) FROM State; SELECT 'City: ' || COUNT(*) FROM City; SELECT 'Contact: ' || COUNT(*) FROM Contact; SELECT 'Views: ' || COUNT(*) FROM sqlite_master WHERE type='view';"
}
finally {
    Pop-Location
}
