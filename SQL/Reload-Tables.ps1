Write-Host "Reload State"
Write-Host ""
$createCmd = Get-Content -Raw -Path .\CreateStateTable.sql
$createCmd += "`n.import backup/State.txt State`n.quit"
#Write-Host "$createCmd"
$createCmd | &"C:\Program Files\SQLite3\sqlite3.exe" \Source\CSharp\Database\SQLite3Repository\Regression.Test\Contacts.db3
Write-Host "Reload City"
Write-Host ""
$createCmd = Get-Content -Raw -Path .\CreateCityTable.sql
$createCmd += "`n.import backup/City.txt City`n.quit"
#Write-Host "$createCmd"
$createCmd | &"C:\Program Files\SQLite3\sqlite3.exe" \Source\CSharp\Database\SQLite3Repository\Regression.Test\Contacts.db3
Write-Host "Reload Contact"
Write-Host ""
$createCmd = Get-Content -Raw -Path .\CreateContactTable.sql
$createCmd += "`n.import backup/Contact.txt Contact`n.quit"
#Write-Host "$createCmd"
$createCmd | &"C:\Program Files\SQLite3\sqlite3.exe" \Source\CSharp\Database\SQLite3Repository\Regression.Test\Contacts.db3