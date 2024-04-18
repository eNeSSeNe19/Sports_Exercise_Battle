DROP SCHEMA public CASCADE;
CREATE SCHEMA public;

CREATE TABLE "User" (
    username VARCHAR(255) PRIMARY KEY,
    password VARCHAR(255) NOT NULL,
    name VARCHAR(255),
    token TEXT,
    bio TEXT,
    image TEXT
);

CREATE TABLE Tournaments (
    tournament_id SERIAL PRIMARY KEY,
    start_time TIMESTAMP NOT NULL,
	end_time TIMESTAMP NOT NULL,
	is_calculated BOOL DEFAULT FALSE
);

CREATE TABLE UserStats (
    username VARCHAR(255) PRIMARY KEY,
    elo INT DEFAULT 100,
    wins INT DEFAULT 0,
    losses INT DEFAULT 0,
    draws INT DEFAULT 0,
    counts INT DEFAULT 0,
    CONSTRAINT fk_userstats_user FOREIGN KEY (username)
        REFERENCES "User" (username)
);

CREATE TABLE PushUpHistory (
    history_id SERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    counts INT NOT NULL,
    duration INT NOT NULL,
    exercise_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tournament_id INT, 
    CONSTRAINT fk_pushuphistory_user FOREIGN KEY (username)
        REFERENCES "User" (username),
    CONSTRAINT fk_pushuphistory_tournament FOREIGN KEY (tournament_id)
        REFERENCES Tournaments (tournament_id)
);




