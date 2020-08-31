--Testing Stored Procedures
--

DROP PROCEDURE IF EXISTS uspStoredProc;

CREATE PROCEDURE uspStoredProc
	@pk TEXT			-- Primary Key
	
AS
BEGIN
	UPDATE Contact SET ModifiedDt=GETDATE() 
		WHERE Id = @pk AND Active=1
END
