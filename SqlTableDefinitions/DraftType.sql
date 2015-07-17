CREATE TABLE DraftType
(
	Id INT IDENTITY(1, 1) PRIMARY KEY,
	Name VARCHAR(31),
	UsesTeams BIT,
);


INSERT INTO DraftType (Name, UsesTeams)
VALUES ('REAL', 1);

INSERT INTO DraftType (Name, UsesTeams)
VALUES ('MOCK', 1);

INSERT INTO DraftType (Name, UsesTeams)
VALUES ('MOCK', 0);

INSERT INTO DraftType (Name, UsesTeams)
VALUES ('RANKINGS', 0);