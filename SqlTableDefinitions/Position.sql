CREATE TABLE Position
(
	Id INT IDENTITY(1, 1) PRIMARY KEY,
	Position VARCHAR(10) NOT NULL
);

CREATE UNIQUE INDEX Position_Position
ON Position (Position);


INSERT INTO Position (Position)
VALUES ('C');

INSERT INTO Position (Position)
VALUES ('LW');

INSERT INTO Position (Position)
VALUES ('RW');

INSERT INTO Position (Position)
VALUES ('D');

INSERT INTO Position (Position)
VALUES ('G');

INSERT INTO Position (Position)
VALUES ('C/LW');

INSERT INTO Position (Position)
VALUES ('C/RW');

INSERT INTO Position (Position)
VALUES ('C/D');

INSERT INTO Position (Position)
VALUES ('LW/RW');

INSERT INTO Position (Position)
VALUES ('LW/D');

INSERT INTO Position (Position)
VALUES ('RW/D');

INSERT INTO Position (Position)
VALUES ('C/LW/RW');

INSERT INTO Position (Position)
VALUES ('C/LW/D');

INSERT INTO Position (Position)
VALUES ('C/RW/D');

INSERT INTO Position (Position)
VALUES ('LW/RW/D');

INSERT INTO Position (Position)
VALUES ('C/LW/RW/D');