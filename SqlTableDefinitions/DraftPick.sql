CREATE TABLE DraftPick
(
	Id INT IDENTITY(1, 1) PRIMARY KEY,
	DraftId INT FOREIGN KEY REFERENCES Draft (Id) NOT NULL,
	ProspectId INT FOREIGN KEY REFERENCES Prospect (Id) NOT NULL,
	TeamId INT FOREIGN KEY REFERENCES Team (Id),
	PickNumber INT NOT NULL
);

CREATE UNIQUE INDEX DraftPick_Draft_PickNumber
ON DraftPick (DraftId, PickNumber);

CREATE UNIQUE INDEX DraftPick_Draft_Prospect
ON DraftPick (DraftId, ProspectId);