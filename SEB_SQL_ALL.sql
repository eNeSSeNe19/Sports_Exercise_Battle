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

CREATE TABLE UserStats (
    username VARCHAR(255) PRIMARY KEY,
    elo INT DEFAULT 100,
    wins INT DEFAULT 0,
    losses INT DEFAULT 0,
    draws INT DEFAULT 0,
    count INT NOT NULL,
    CONSTRAINT fk_userstats_user FOREIGN KEY (username)
    REFERENCES "User" (username)
);

CREATE TABLE PushUpHistory (
    history_id SERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    count INT NOT NULL,
    duration INTERVAL NOT NULL,
    exercise_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_pushuphistory_user FOREIGN KEY (username)
    REFERENCES "User" (username)
);

CREATE TABLE Tournaments (
    tournament_id SERIAL PRIMARY KEY,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE TournamentEntries (
    entry_id SERIAL PRIMARY KEY,
    tournament_id INT NOT NULL,
    username VARCHAR(255) NOT NULL,
    push_up_count INT NOT NULL,
    CONSTRAINT fk_tournamententries_tournaments FOREIGN KEY (tournament_id)
    REFERENCES Tournaments (tournament_id),
    CONSTRAINT fk_tournamententries_user FOREIGN KEY (username)
    REFERENCES "User" (username)
);

CREATE TABLE TournamentResults (
    result_id SERIAL PRIMARY KEY,
    tournament_id INT NOT NULL,
    username VARCHAR(255) NOT NULL,
    result_position INT,
    elo_change INT,
    CONSTRAINT fk_tournamentresults_tournaments FOREIGN KEY (tournament_id)
    REFERENCES Tournaments (tournament_id),
    CONSTRAINT fk_tournamentresults_user FOREIGN KEY (username)
    REFERENCES "User" (username)
);

