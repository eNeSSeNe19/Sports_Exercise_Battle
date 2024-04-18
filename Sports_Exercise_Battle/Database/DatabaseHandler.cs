using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;
using System.Xml;
using Sports_Exercise_Battle.Models.Users;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Sports_Exercise_Battle.Models.Entries;
using System.Collections;
using Npgsql.TypeMapping;
using System.Diagnostics.Metrics;


namespace Sports_Exercise_Battle.Database
{
    public class DatabaseHandler
    {
        NpgsqlConnection connection;
        private static DatabaseHandler instance = null;
        private static readonly object padlock = new object();
        public string? Token;
        public string? AuthorizedUser = null;


        public const int K_FACTOR = 32;

        public static DatabaseHandler Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DatabaseHandler();
                    }
                    instance.Token = null;
                    return instance;
                }
            }
        }


        public void Connect()
        {
            string cnfg = AppDomain.CurrentDomain.BaseDirectory + "/dbConnection.json";
            Console.WriteLine("Configuration file path: " + cnfg);
            try
            {
                if (File.Exists(cnfg))
                {
                    var pConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(cnfg));
                    if (pConfig == null || pConfig["host"] == null || pConfig["username"] == null || pConfig["password"] == null || pConfig["database"] == null)
                    {
                        throw new IOException("Database Configuratoin is invalid!");
                    }

                    string cs = $"Host={pConfig["host"]};Username={pConfig["username"]};Password={pConfig["password"]};Database={pConfig["database"]};Include Error Detail=true";
                    connection = new NpgsqlConnection(cs);
                    connection.Open();

                    Console.WriteLine("Database connection confirmed!");
                }
                else
                {
                    Console.WriteLine("Database Configuration File is missing!");
                    System.Environment.Exit(-1);
                }
            }
            catch (NpgsqlException ex)
            {
                // Now the exception should include more details
                Console.WriteLine("Failed to connect to Database: " + ex.Message);
                System.Environment.Exit(-1);
            }
        }

        public User? GetUserByUsername(string username)
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    try
                    {
                        NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, name, bio, image FROM public.\"User\" WHERE username = @p1;", connection);
                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd.Prepare();
                        cmd.Parameters["p1"].Value = username;

                        NpgsqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            User user = new User((string)dr[0], "", (string)dr[1], (string)dr[2], (string)dr[3]);
                            dr.Close();
                            return user;
                        }
                        else
                        {
                            dr.Close();
                            return null;
                        }
                    }
                    catch (PostgresException e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception("No User found!");
                    }


                }
                else
                {
                    Console.WriteLine("Database not connected!");
                    return null;
                }
            }
        }

        public int RegisterUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                throw new Exception("Invalid parameters");
            }

            lock (padlock)
            {
                if (connection != null)
                {
                    try
                    {

                        NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO public.\"User\"(username, password, name,  bio, image) VALUES(@p1, @p2, @p3, @p4, @p5);", connection); //token, p4
                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p3", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p4", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p5", DbType.String));

                        cmd.Prepare();
                        cmd.Parameters["p1"].Value = user.Username;
                        cmd.Parameters["p2"].Value = user.Password;
                        cmd.Parameters["p3"].Value = user.Name == null ? "" : user.Name;
                        cmd.Parameters["p4"].Value = user.Bio == null ? "" : user.Bio;
                        cmd.Parameters["p5"].Value = user.Image == null ? "" : user.Image;

                        cmd.ExecuteNonQuery();

                        NpgsqlCommand cmd2 = new NpgsqlCommand("INSERT INTO public.\"userstats\"(username, elo, wins, losses, draws, counts) VALUES(@p1, 100, 0, 0, 0, 0);", connection);
                        cmd2.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd2.Prepare();
                        cmd2.Parameters["p1"].Value = user.Username;
                        cmd2.ExecuteNonQuery();

                        return 0;
                    }
                    catch (PostgresException e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception("User already exists!");
                    }
                }
                else
                {
                    throw new Exception("No Database connection!");
                }
            }
        }

        public int UpdateUser(User user)
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    try
                    {
                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE public.\"User\" SET name = @p1, bio = @p2, image = @p3 WHERE username = @p4;", connection);

                        cmd.Parameters.AddWithValue("p1", user.Name);
                        cmd.Parameters.AddWithValue("p2", user.Bio);
                        cmd.Parameters.AddWithValue("p3", user.Image);
                        cmd.Parameters.AddWithValue("p4", user.Username);

                        cmd.ExecuteNonQuery();

                        return 0; // Assuming 0 means success
                    }
                    catch (PostgresException e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception("User could not be updated!");
                    }
                }
                else
                {
                    throw new Exception("No Database connection!");
                }
            }
        }

        public string? LoginUser(string username, string password)
        {


            if (username == null || password == null)
            {
                throw new Exception("Invalid parameters");
            }

            lock (padlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT password FROM public.\"User\" WHERE username = @p1;", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();
                    cmd.Parameters["p1"].Value = username;


                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        if (password == (string)dr[0])
                        {
                            Console.WriteLine("Login successful");
                            string token = username + "-sebToken";
                            dr.Close();
                            InsertNewToken(username, token);
                            return token;
                        }
                        else
                        {
                            dr.Close();
                            throw new Exception("Incorrect password");
                        }
                    }
                    else
                    {
                        dr.Close();
                        throw new Exception("No such user!");
                    }
                }
                return null;
            }
        }

        public bool AuthorizeToken()
        {
            lock (padlock)
            {
                if (Token == null)
                {
                    return false;
                }
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT username FROM public.\"User\" WHERE token = @p1;", connection);
                cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                cmd.Prepare();
                cmd.Parameters["p1"].Value = Token;
                cmd.ExecuteNonQuery();
                NpgsqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    AuthorizedUser = (string)dr[0];
                    dr.Close();
                    return true;
                }
                else
                {
                    AuthorizedUser = null;
                    dr.Close();
                    return false;
                }
            }
        }

        private void InsertNewToken(string username, string token)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("UPDATE public.\"User\" SET token = @p1 WHERE username = @p2;", connection);
                cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.String));
                cmd.Prepare();
                cmd.Parameters["p1"].Value = token;
                cmd.Parameters["p2"].Value = username;

                cmd.ExecuteNonQuery();
                AuthorizedUser = username;
            }
            catch (PostgresException)
            {
                Console.WriteLine("Could not insert token, user does not exist");
            }
        }

        public void UpdateUserStats(UserStats stats)
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    try
                    {
                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE public.\"userstats\" SET elo = @p1, wins = @p2, losses = @p3, draws = @p4, counts = @p5  WHERE username = @p6;", connection);

                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p3", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p4", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p5", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p6", DbType.String));
                        cmd.Prepare();

                        if (stats.Elo <= 0)
                        {
                            cmd.Parameters["p1"].Value = 1;
                        }
                        else
                        {
                            cmd.Parameters["p1"].Value = stats.Elo;

                        }
                        cmd.Parameters["p2"].Value = stats.Wins;
                        cmd.Parameters["p3"].Value = stats.Losses;
                        cmd.Parameters["p4"].Value = stats.Draws;
                        cmd.Parameters["p5"].Value = stats.Counts;
                        cmd.Parameters["p6"].Value = stats.Username;

                        cmd.ExecuteNonQuery();
                    }
                    catch (PostgresException)
                    {
                        Console.WriteLine("ERR: updating stats");
                    }
                }
            }
        }

        public User GetUserByToken(string token)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, password, name, bio, image FROM public.\"User\" WHERE token = @p1;", connection);// to get the username to work with
                cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                cmd.Prepare();
                cmd.Parameters["p1"].Value = token;
                NpgsqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    User user = new User((string)dr[0], (string)dr[1], (string)dr[2], (string)dr[3], (string)dr[4]);
                    dr.Close();
                    return user;
                }
                else
                {
                    dr.Close();
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("getUserByToken:", ex);
            }
        }

        public UserStats? GetUserStats()
        {
            lock (padlock)
            {
                if (connection != null && Token != null)
                {
                    try
                    {


                        var user = GetUserByToken(Token);
                        var username = user.Username;

                        if (string.IsNullOrWhiteSpace(username))
                            return null;

                        var tournaments = GetAllTournaments();
                        foreach (var tournament in tournaments) // updating userstats if not already up-to-date
                        {
                            if (tournament.EndTime < DateTime.Now && tournament.Is_Calculated == false)
                            {
                                CalculateTournamentResultsAndUpdateElo(tournament.TournamentId);
                                MarkTournamentAsCalculated(tournament.TournamentId);
                            }
                            else
                            {
                                Console.WriteLine("Tournament already calculated or not ended");
                            }
                        }

                        NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, elo, wins, losses, draws, counts FROM public.\"userstats\" WHERE username = @p1;", connection); //add more stats if needed
                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd.Prepare();
                        cmd.Parameters["p1"].Value = username;
                        NpgsqlDataReader dr = cmd.ExecuteReader();
                        UserStats userStats = new UserStats();
                        if (dr.Read())
                        {
                            userStats.Username = (string)dr[0];
                            userStats.Elo = (int)dr[1];
                            userStats.Wins = (int)dr[2];
                            userStats.Losses = (int)dr[3];
                            userStats.Draws = (int)dr[4];
                            userStats.Counts = (int)dr[5];
                            dr.Close();
                            return userStats;
                        }
                        else
                        {
                            dr.Close();
                            return null;
                        }
                    }
                    catch (PostgresException e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception("No User found!");
                    }

                }
                else
                {
                    Console.WriteLine("Database not connected!");
                    return null;
                }
            }
        }

        public UserStats? GetUserStatsByUsername(string Username)
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    try
                    {
                        string username = Username;

                        if (string.IsNullOrWhiteSpace(username))
                            return null;

                        NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, elo, wins, losses, draws, counts FROM public.\"userstats\" WHERE username = @p1;", connection);
                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd.Prepare();
                        cmd.Parameters["p1"].Value = username;
                        NpgsqlDataReader dr = cmd.ExecuteReader();
                        UserStats userStats = new UserStats();
                        if (dr.Read())
                        {
                            userStats.Username = (string)dr[0];
                            userStats.Elo = (int)dr[1];
                            userStats.Wins = (int)dr[2];
                            userStats.Losses = (int)dr[3];
                            userStats.Draws = (int)dr[4];
                            userStats.Counts = (int)dr[5];
                            dr.Close();
                            return userStats;
                        }
                        else
                        {
                            dr.Close();
                            return null;
                        }
                    }
                    catch (PostgresException e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception("No User found!");
                    }

                }
                else
                {
                    Console.WriteLine("Database not connected!");
                    return null;
                }
            }
        }

        public List<UserStats>? GetUserScoreboard()
        {
            lock (padlock)
            {
                if (connection != null && AuthorizedUser != null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, elo, wins, losses, draws, counts FROM public.\"userstats\" ORDER BY elo DESC;", connection);
                    cmd.Prepare();

                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    List<UserStats> stats = new List<UserStats>();
                    while (dr.Read())
                    {
                        UserStats element = new UserStats
                        {
                            Username = (string)dr[0],
                            Elo = (int)dr[1],
                            Wins = (int)dr[2],
                            Losses = (int)dr[3],
                            Draws = (int)dr[4],
                            Counts = (int)dr[5]
                        };

                        stats.Add(element);
                    }

                    dr.Close();
                    return stats.Count > 0 ? stats : null;
                }
                else
                {
                    Console.WriteLine("Database not connected");
                    return null;
                }
            }
        }

        public string GetUserPushUpHistory()
        {
            lock (padlock)
            {
                if (connection != null && Token != null)
                {
                    string username = GetUserByToken(Token).Username;
                    if (string.IsNullOrWhiteSpace(username))
                    {
                        return null;
                    }

                    List<PushUpEntry> history = new List<PushUpEntry>();
                    string query = "SELECT counts, exercise_date, duration FROM public.\"pushuphistory\" WHERE username = @p1;";

                    using (var cmd2 = new NpgsqlCommand(query, connection))
                    {
                        cmd2.Parameters.AddWithValue("p1", username);

                        using (var dr2 = cmd2.ExecuteReader())
                        {
                            while (dr2.Read())
                            {
                                PushUpEntry entry = new PushUpEntry
                                {
                                    Username = username,
                                    Count = dr2.GetInt32(dr2.GetOrdinal("counts")),
                                    EntryTime = dr2.GetDateTime(dr2.GetOrdinal("exercise_date")),
                                    DurationInSeconds = dr2.GetInt32(dr2.GetOrdinal("duration"))
                                };
                                history.Add(entry);
                            }
                        }
                    }

                    return history.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(new { histories = history }) : "[]";
                }
                else
                {
                    Console.WriteLine("Database not connected!");
                    return null;
                }
            }
        }

        public List<PushUpEntry> GetPushUpEntriesByTournamentId(int tournamentId)
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    List<PushUpEntry> entries = new List<PushUpEntry>();

                    using (var cmd = new NpgsqlCommand("SELECT username, counts, exercise_date, duration FROM public.\"pushuphistory\" WHERE tournament_id = @tournamentId;", connection))
                    {
                        cmd.Parameters.AddWithValue("tournamentId", tournamentId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PushUpEntry entry = new PushUpEntry
                                {
                                    Username = reader.GetString(reader.GetOrdinal("username")),
                                    Count = reader.GetInt32(reader.GetOrdinal("counts")),
                                    EntryTime = reader.GetDateTime(reader.GetOrdinal("exercise_date")),
                                    DurationInSeconds = reader.GetInt32(reader.GetOrdinal("duration"))
                                };
                                entries.Add(entry);
                            }
                        }
                    }

                    return entries;
                }
                else
                {
                    Console.WriteLine("Database not connected!");
                    return new List<PushUpEntry>(); // return an empty list instead of null
                }
            }
        }

        public void InsertNewPushUpEntry(PushUpEntry entry)
        {
            lock (padlock)
            {
                if (Token == null)
                {
                    return;
                }

                var lastActiveTournament = GetLastActiveTournament();

                if (lastActiveTournament == null)
                {
                    InsertNewTournament();
                    lastActiveTournament = GetLastActiveTournament();
                }

                NpgsqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Insert the new pushup entry
                    NpgsqlCommand insertCmd = new NpgsqlCommand("INSERT INTO public.pushuphistory(username, counts, duration, exercise_date, tournament_id) VALUES(@p1, @p2, @p3, @p4, @p5);", connection);
                    insertCmd.Parameters.AddWithValue("p1", entry.Username);
                    insertCmd.Parameters.AddWithValue("p2", entry.Count);
                    insertCmd.Parameters.AddWithValue("p3", entry.DurationInSeconds);
                    insertCmd.Parameters.AddWithValue("p4", DateTime.UtcNow); // Using UTC directly
                    insertCmd.Parameters.AddWithValue("p5", lastActiveTournament.TournamentId);
                    insertCmd.Transaction = transaction;
                    insertCmd.ExecuteNonQuery();

                    // Update the total counts in UserStats
                    NpgsqlCommand updateCmd = new NpgsqlCommand(
                        "UPDATE public.userstats SET counts = counts + @count WHERE username = @username;", connection);
                    updateCmd.Parameters.AddWithValue("count", entry.Count);
                    updateCmd.Parameters.AddWithValue("username", entry.Username);
                    updateCmd.Transaction = transaction;
                    updateCmd.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (PostgresException ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public Tournament GetLastActiveTournament()
        {
            lock (padlock)
            {
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM public.tournaments WHERE end_time >= @p1;", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.DateTime));
                    cmd.Prepare();
                    cmd.Parameters["p1"].Value = DateTime.Now.ToUniversalTime();

                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        Tournament tournament = new Tournament();
                        tournament.TournamentId = (int)dr[0];
                        tournament.StartTime = (DateTime)dr[1];
                        tournament.EndTime = (DateTime)dr[2];
                        dr.Close();
                        return tournament;
                    }
                    else
                    {
                        dr.Close();
                        return null;
                    }
                }
                catch (PostgresException e)
                {
                    Console.WriteLine(e.Message);
                    throw new Exception("No active tournament");
                }
            }
        }

        public void InsertNewTournament()
        {
            lock (padlock)
            {
                try
                {
                    var tournamentDuration = TimeSpan.FromMinutes(2);

                    NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO public.tournaments(start_time, end_time) VALUES(@p1, @p2);", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.DateTime));
                    cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.DateTime));
                    cmd.Prepare();
                    cmd.Parameters["p1"].Value = DateTime.Now.ToUniversalTime();
                    cmd.Parameters["p2"].Value = DateTime.Now.Add(tournamentDuration).ToUniversalTime();
                    cmd.ExecuteNonQuery();
                }
                catch (PostgresException e)
                {
                    throw new Exception("InsertNewTournament: " + e);
                }
            }
        }

        public List<Tournament> GetAllTournaments()
        {
            lock (padlock)
            {
                List<Tournament> tournaments = new List<Tournament>();
                HashSet<int> seenTournamentIds = new HashSet<int>(); // To track already added tournament IDs

                try
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT tournament_id, start_time, end_time, is_calculated FROM public.tournaments", connection))
                    {
                        using (NpgsqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                int tournamentId = (int)dr["tournament_id"];
                                if (!seenTournamentIds.Contains(tournamentId))
                                {
                                    Tournament tournament = new Tournament
                                    {
                                        TournamentId = tournamentId,
                                        StartTime = (DateTime)dr["start_time"],
                                        EndTime = (DateTime)dr["end_time"],
                                        Is_Calculated = (bool)dr["is_calculated"]
                                    };
                                    tournaments.Add(tournament);
                                    seenTournamentIds.Add(tournamentId); // Mark this ID as seen
                                }
                            }
                            dr.Close();
                        }
                    }
                    // After fetching, update the status
                    foreach (var tournament in tournaments)
                    {
                        if (tournament.EndTime > DateTime.Now)
                        {
                            tournament.State = "still active";
                        }
                        else
                        {
                            tournament.State = $"ended at {tournament.EndTime}";
                        }
                    }

                    return tournaments;

                }
                catch (PostgresException e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        public void CalculateTournamentResultsAndUpdateElo(int tournamentId)
        {
            lock (padlock)
            {
                if (connection == null)
                {
                    Console.WriteLine("Database not connected!");
                    return;
                }

                try
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, SUM(counts) as total_counts FROM public.pushuphistory WHERE tournament_id = @p1 GROUP BY username ORDER BY total_counts DESC;", connection))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("p1", tournamentId));
                        cmd.Prepare();

                        var userResults = new List<(string Username, int Counts)>();
                        using (NpgsqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                userResults.Add((dr.GetString(0), dr.GetInt32(1)));
                            }
                        }

                        // Fetch current stats for all participants
                        var userStatsList = userResults.Select(result => GetUserStatsByUsername(result.Username)).ToList();
                        int averageOpponentElo = (int)userStatsList.Average(us => us.Elo);

                        foreach (var userStats in userStatsList)
                        {
                            double expectedScore = CalculateExpectedScore(userStats.Elo, averageOpponentElo);
                            double actualScore = DetermineActualScore(userStats.Username, userResults);
                            userStats.Elo = CalculateNewElo(userStats.Elo, actualScore, expectedScore);
                            if (actualScore == 1)
                            {
                                userStats.Wins++;
                            }
                            else if (actualScore == 0)
                            {
                                userStats.Losses++;
                            }
                            else if (actualScore == 0.5)
                            {
                                userStats.Draws++;
                            }
                            UpdateUserStats(userStats);
                        }

                        MarkTournamentAsCalculated(tournamentId);
                    }
                }
                catch (PostgresException e)
                {
                    Console.WriteLine(e.Message);
                    throw new Exception("Error calculating tournament results.");
                }
            }
        }

        public void MarkTournamentAsCalculated(int tournamentId)
        {
            lock (padlock)
            {
                if (connection == null)
                {
                    Console.WriteLine("Database not connected!");
                    return;
                }



                try
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE public.Tournaments SET is_calculated = TRUE WHERE tournament_id = @p1;", connection))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("p1", tournamentId));
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (PostgresException e)
                {
                    Console.WriteLine(e.Message);
                    throw new Exception("Error marking tournament as calculated.");
                }
            }
        }

        public double CalculateExpectedScore(int playerRating, int opponentRating)
        {
            return 1 / (1 + Math.Pow(10, (opponentRating - playerRating) / 400.0));
        }

        public int CalculateNewElo(int currentElo, double actualScore, double expectedScore)
        {
            return currentElo + (int)(K_FACTOR * (actualScore - expectedScore));
        }

        private double DetermineActualScore(string username, List<(string Username, int Counts)> userResults)
        {
            // Find the user's result.
            var userResult = userResults.Find(ur => ur.Username == username);

            // Find all unique counts.
            var uniqueCounts = userResults.Select(ur => ur.Counts).Distinct().ToList();

            // Determine ranks by count.
            var rankedResults = userResults
                .GroupBy(ur => ur.Counts)
                .Select(grp => new { Count = grp.Key, Users = grp.Select(g => g.Username).ToList() })
                .OrderByDescending(r => r.Count)
                .ToList();

            // Determine if there is a tie at the user's count level.
            var userRank = rankedResults.FindIndex(r => r.Users.Contains(username));
            var isTie = rankedResults[userRank].Users.Count > 1;

            // Return the actual score based on the rank and whether there's a tie.
            if (userRank == 0) return isTie ? 0.5 : 1; // Winner or draw if there's a tie for the first place
            return 0; // Loser if not in the first place or part of a tie for the first place
        }

        public void DeleteAllUserStats()
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    try
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM public.\"userstats\"", connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (PostgresException e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception("UserStats could not be deleted!");
                    }
                }
                else
                {
                    throw new Exception("No Database connection!");
                }
            }
        }

        public void DeleteAllUser()
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    try
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM public.\"User\"", connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (PostgresException e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception("User could not be deleted!");
                    }
                }
                else
                {
                    throw new Exception("No Database connection!");
                }
            }
        }
    }
}
