DROP TABLE IF EXISTS State;

CREATE TABLE State(
	Id TEXT NOT NULL,				 -- Two character Id
	[Name] TEXT NOT NULL,			 -- State Name
	Active INTEGER NOT NULL,			 -- Boolean
	ModifiedDt INTEGER NOT NULL,        -- DateTime(YYYY-MM-DD HH:MM:SS.SSS)
	CreateDt INTEGER NOT NULL           -- DateTime(YYYY-MM-DD HH:MM:SS.SSS)
 );
CREATE UNIQUE INDEX PK_State ON State(Id);
