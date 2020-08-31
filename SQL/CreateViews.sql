--Contact Views
--
DROP VIEW IF EXISTS vwFindAllContactView;

CREATE VIEW vwFindAllContactView
AS
	SELECT c1.Id AS Id, FirstName, LastName, Address1, Address2, Notes, ZipCode, HomePhone, WorkPhone, CellPhone, EMail, CityId, 
		c2.Name AS CityName, c2.StateId AS StateId, 
		s.Name AS StateName, 
		c1.Active AS Active, c1.ModifiedDt AS ModifiedDt, c1.CreateDt AS CreateDt 
	FROM Contact c1 
	JOIN City c2 ON (c2.Id = c1.CityId) 
	JOIN State s ON (s.Id = c2.StateId) 
	WHERE c1.Active=1;
--City Views
--
DROP VIEW IF EXISTS vwFindAllCityView;

CREATE VIEW vwFindAllCityView
AS
	SELECT c.Id AS Id, c.Name AS Name, StateId, 
		s.Name AS StateName, 
		c.Active AS Active, c.ModifiedDt AS ModifiedDt, c.CreateDt AS CreateDt 
	FROM City c
	JOIN State s ON (s.Id = c.StateId) 
	WHERE c.Active=1;