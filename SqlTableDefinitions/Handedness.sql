CREATE TABLE Handedness
(
	Id INT IDENTITY(1, 1) PRIMARY KEY,
	Hand VARCHAR(5) NOT NULL
);

CREATE UNIQUE INDEX Handedness_Hand
ON Handedness(Hand);

INSERT INTO Handedness (Hand)
VALUES ('LEFT');

INSERT INTO Handedness (Hand)
VALUES ('RIGHT');