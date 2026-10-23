SQLite3Repository Test Matrix
===

P = covered and passing. NA = not applicable (SQLite3 has no stored procedures; State has no view).

Test| SQL | View
 -- | --- | ----
**Contact** | |
FindAll (ordered by Id) | P | NA
FindAllPaged     | P | NA
FindAllView      | NA | P
FindAllViewPaged (view row count) | NA | P
FindByPK         | P | NA
FindViewByPK     | NA | P
Add              | P | NA
Update           | P | NA
Delete (soft)    | P | NA
ExecNonQuery     | P | NA
---------------- | --- | ----
**City** | |
FindAll (ordered by Id) | P | NA
FindAllPaged     | P | NA
FindAllPaged pages do not overlap | P | NA
FindAllView      | NA | P
FindAllViewPaged (view row count) | NA | P
FindByPK (found / not found) | P | NA
Add              | P | NA
Update           | P | NA
Delete (soft)    | P | NA
---------------- | --- | ----
**State** | |
FindAll (ordered by Id) | P | NA
FindAllPaged     | P | NA
FindAllPagedSorted (Name DESC) | P | NA
Unknown sort column falls back to Id | P | NA
Invalid pager (PageSize/PageNbr) | P | NA
FindByPK natural key (found / not found) | P | NA
Add (natural key) | P | NA
Update           | P | NA
Delete (soft)    | P | NA
ExecJSONQuery (active rows, UTC dates) | P | NA
DateTime (Unix time to UTC) | P | NA
---------------- | --- | ----
**Infrastructure** | |
Connection       | P | NA
SQL failure is propagated | P | NA
Missing database is not created | P | NA
PrimaryKey ToString | P | NA
---------------- | --- | ----
**Transactional** | |
UOW connection   | P | NA
Save (reads see uncommitted writes) | P | NA
Rollback update  | P | NA
Rollback add     | P | NA
Terminal lifecycle (repeated Save/Rollback) | P | NA
Dispose discards uncommitted work | P | NA
UOW requires the repository's DBContext | P | NA

`AddTest`, `UpdateTest`, `DeleteTest`, and `SaveTest` change seed rows. Rebuild (which copies a fresh
`Contacts.db3` to the output directory) or run `SQL\Reload-Tables.ps1` before rerunning them.
