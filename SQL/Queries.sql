SELECT Id,[Name],Active,datetime(ModifiedDt,'unixepoch','localtime'),datetime(CreateDt,'unixepoch','localtime') FROM State;
SELECT Id,[Name],Active,datetime(ModifiedDt,'unixepoch'),datetime(CreateDt,'unixepoch') FROM State;

SELECT Id,[Name],Active,datetime(ModifiedDt,'unixepoch','localtime'),datetime(CreateDt,'unixepoch','localtime') FROM State LIMIT 5 OFFSET 0;

INSERT INTO State (Id,[Name],Active,ModifiedDt,CreateDt) VALUES('WW','Warren',1,strftime('%s','now'),strftime('%s','now'));
SELECT last_insert_rowid()