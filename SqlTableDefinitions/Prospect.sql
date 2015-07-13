CREATE TABLE Prospect
(
Id int IDENTITY(1, 1) PRIMARY KEY,
Name varchar(255) NOT NULL,
Team varchar(255),
Position varchar(10) NOT NULL,
Handedness varchar(1) NOT NULL,
DraftYear int NOT NULL
);

CREATE UNIQUE INDEX Prospect_Name
ON Prospect (Name);