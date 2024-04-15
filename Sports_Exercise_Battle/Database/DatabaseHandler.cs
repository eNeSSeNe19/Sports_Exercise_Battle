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


namespace Sports_Exercise_Battle.Database
{
    public class DatabaseHandler
    {
        NpgsqlConnection connection;
        private static DatabaseHandler instance = null;
        private static readonly object padlock = new object();
        public string? Token;
        public string? AuthorizedUser = null;


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
            string config = AppDomain.CurrentDomain.BaseDirectory + "/dbConnection.json";
            Console.WriteLine("Config file path: " + config);
            try
            {
                if (File.Exists(config))
                {
                    var pConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(config));
                    if (pConfig == null || pConfig["host"] == null || pConfig["username"] == null || pConfig["password"] == null || pConfig["database"] == null)
                    {
                        throw new IOException("DbConfig is invalid");
                    }

                    string cs = $"Host={pConfig["host"]};Username={pConfig["username"]};Password={pConfig["password"]};Database={pConfig["database"]};Include Error Detail=true";
                    connection = new NpgsqlConnection(cs);
                    connection.Open();

                    Console.WriteLine("Database connection established!");
                }
                else
                {
                    Console.WriteLine("Database config is missing!");
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

        public User? GetUserByID(string username)
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
                            //user.Username = (string)dr[0];
                            //user.Name = (string)dr[1];
                            //user.Bio = (string)dr[2];
                            //user.Image = (string)dr[3];
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
                        //return 1;
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
                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE public.\"User\" SET name = '@p1', bio = @p2, image = @p3 WHERE username = @p4;", connection);

                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p3", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p4", DbType.String));
                        cmd.Prepare();

                        cmd.Parameters["p1"].Value = user.Name;
                        cmd.Parameters["p2"].Value = user.Bio;
                        cmd.Parameters["p3"].Value = user.Image;
                        cmd.Parameters["p4"].Value = user.Username;
                        cmd.ExecuteNonQuery();



                        return 0;
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

        private string GetRandomSalt(int length) //Needed for Hashing
        {
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = rand.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                sb.Append(letter);
            }
            return sb.ToString();
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
                            string usernameWithSalt = username + GetRandomSalt(5);

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
                string username;
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

                        NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, elo, counts FROM public.\"userstats\" WHERE username = @p1;", connection); //add more stats if needed
                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd.Prepare();
                        cmd.Parameters["p1"].Value = username;
                        NpgsqlDataReader dr = cmd.ExecuteReader();
                        UserStats userStats = new UserStats();
                        if (dr.Read())
                        {
                            userStats.Username = (string)dr[0];
                            userStats.Elo = (int)dr[1];
                            userStats.Counts = (int)dr[2];
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

        public string? GetUserScoreboard() 
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
                        UserStats element = new UserStats();
                        element.Username = (string)dr[0];
                        element.Elo = (int)dr[1];
                        element.Wins = (int)dr[2];
                        element.Losses = (int)dr[3];
                        element.Draws = (int)dr[4];
                        element.Counts = (int)dr[5];

                        stats.Add(element);
                    }

                    dr.Close();

                    if (stats.Count > 0)
                    {
                        string json = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            scoreboard = stats
                        });
                        return json;
                    }
                    else
                    {
                        Console.WriteLine("Empty scoreboard");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("Database not connected");
                    return null;
                }
            }
        }

        //public string GetUserPushUpHistory()
        //{
        //    lock (padlock)
        //    {
        //        if (connection != null && Token != null)
        //        {
        //            try
        //            {
        //                NpgsqlCommand cmd = new NpgsqlCommand("SELECT username FROM public.\"User\" WHERE token = @p1;", connection);// to get the username to work with
        //                cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
        //                cmd.Prepare();
        //                cmd.Parameters["p1"].Value = Token;
        //                NpgsqlDataReader dr = cmd.ExecuteReader(); //GETUSERBYTOKEN HERE?
        //                string username;
        //                if (dr.Read())
        //                {
        //                    username = (string)dr[0];
        //                    dr.Close();
        //                }
        //                else
        //                {
        //                    dr.Close();
        //                    return null;
        //                }

        //                if (string.IsNullOrWhiteSpace(username))
        //                {
        //                    return null;

        //                }

        //                NpgsqlCommand cmd2 = new NpgsqlCommand("SELECT counts, exercise_date, duration FROM public.\"pushuphistory\" WHERE username = @p1;", connection);
        //                cmd2.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
        //                cmd2.Prepare();
        //                cmd2.Parameters["p1"].Value = username;
        //                cmd2.ExecuteNonQuery();
        //                NpgsqlDataReader dr2 = cmd2.ExecuteReader();
        //                List<PushUpEntry> history = new List<PushUpEntry>();
        //                while (dr2.Read())
        //                {
        //                    PushUpEntry entry = new PushUpEntry();
        //                    entry.Username = username;
        //                    entry.Count = (int)dr[0];
        //                    entry.EntryTime = (DateTime)dr[2];
        //                    entry.Duration = (int)dr[3];

        //                    history.Add(entry);

        //                }
        //                dr2.Close();

        //                if (history.Count > 0)
        //                {
        //                    string json = System.Text.Json.JsonSerializer.Serialize(new
        //                    {
        //                        histories = history
        //                    });
        //                    return json;
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Empty history");
        //                    return null;
        //                }

        //            }
        //            catch (PostgresException e)
        //            {
        //                Console.WriteLine(e.Message);
        //                throw new Exception("History not found!");
        //            }

        //        }
        //        else
        //        {
        //            Console.WriteLine("Database not connected!");
        //            return null;
        //        }
        //    }
        //}

        public string GetUserPushUpHistory()
        {
            lock (padlock)
            {
                if (connection != null && Token != null)
                {
                    string username = GetUserByToken(Token).Username; // This method should return the username or null
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
                                    Duration = dr2.GetInt32(dr2.GetOrdinal("duration")) 
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


        //public void InsertNewPushUpEntry(PushUpEntry entry)
        //{
        //    lock (padlock)
        //    {
        //        if (Token == null)
        //        {
        //            return;
        //        }

        //        //var user = GetUserByToken(Token);
        //        var lastActiveTournament = GetLastActiveTournament();

        //        if (lastActiveTournament == null)
        //        {
        //            InsertNewTournament();
        //        }
        //        lastActiveTournament = GetLastActiveTournament();

        //        try
        //        {
        //            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO public.pushuphistory(username, counts, duration,  exercise_date, tournament_id) VALUES(@p1, @p2, @p3, @p4, @p5);", connection);
        //            cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
        //            cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.Int64));
        //            cmd.Parameters.Add(new NpgsqlParameter("p3", DbType.Int64));
        //            cmd.Parameters.Add(new NpgsqlParameter("p4", DbType.DateTime));
        //            cmd.Parameters.Add(new NpgsqlParameter("p5", DbType.Int64));
        //            cmd.Prepare();
        //            cmd.Parameters["p1"].Value = entry.Username;
        //            cmd.Parameters["p2"].Value = entry.Count;
        //            cmd.Parameters["p3"].Value = entry.Duration;
        //            cmd.Parameters["p4"].Value = DateTime.Now.ToUniversalTime();
        //            cmd.Parameters["p5"].Value = lastActiveTournament.TournamentId;
        //            cmd.ExecuteNonQuery();


        //        }
        //        catch (PostgresException ex)
        //        {
        //            throw new Exception("InsertNewPushUpEntry: ", ex);
        //        }
        //    }
        //}

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
                    insertCmd.Parameters.AddWithValue("p3", entry.Duration);
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
                    var tournamentDuration = TimeSpan.FromMinutes(2); //maybe set it somewhere as global/class parameter

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

        //public List<Tournament> GetAllTournaments()
        //{
        //    lock (padlock)
        //    {
        //        List<Tournament> tournaments = new List<Tournament>();
        //        try
        //        {
        //            using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT tournament_id, start_time, end_time FROM public.tournaments", connection))
        //            {
        //                using (NpgsqlDataReader dr = cmd.ExecuteReader())
        //                {
        //                    while (dr.Read())
        //                    {
        //                        Tournament tournament = new Tournament
        //                        {
        //                            TournamentId = (int)dr["tournament_id"],
        //                            StartTime = (DateTime)dr["start_time"],
        //                            EndTime = (DateTime)dr["end_time"]  
        //                        };
        //                        //if(tournaments.First(x => x.TournamentId == tournament.TournamentId)) CHECK IF ITS ALREADY EXIST
        //                        tournaments.Add(tournament);
        //                    }
        //                    dr.Close();  // This line is technically not needed because of 'using', but left for clarity
        //                }
        //            }
        //            return tournaments;
        //        }
        //        catch (PostgresException e)
        //        {
        //            Console.WriteLine(e.Message);
        //            throw;  
        //        }
        //    }
        //}

        public List<Tournament> GetAllTournaments()
        {
            lock (padlock)
            {
                List<Tournament> tournaments = new List<Tournament>();
                HashSet<int> seenTournamentIds = new HashSet<int>(); // To track already added tournament IDs

                try
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT tournament_id, start_time, end_time FROM public.tournaments", connection))
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
                                        EndTime = (DateTime)dr["end_time"]
                                    };
                                    tournaments.Add(tournament);
                                    seenTournamentIds.Add(tournamentId); // Mark this ID as seen
                                }
                            }
                            dr.Close();
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


        //public List<Tournament> GetAllTournaments()
        //{
        //    lock (padlock)
        //    {
        //        List<Tournament> tournaments = new List<Tournament>();
        //        try
        //        {
        //            NpgsqlCommand cmd = new NpgsqlCommand("SELECT tournament_id, start_time, end_time FROM public.tournaments", connection);
        //            cmd.Prepare();
        //            NpgsqlDataReader dr = cmd.ExecuteReader();
        //            while (dr.Read())
        //            {
        //                Tournament tournament = new Tournament();
        //                tournament.TournamentId = (int)dr["tournament_id"];
        //                tournament.StartTime = (DateTime)dr["start_time"];
        //                tournament.EndTime = (DateTime)dr["end:time"];
        //                tournaments.Add(tournament);
        //            }
        //            dr.Close();
        //            return tournaments;
        //        }
        //        catch (PostgresException e)
        //        {
        //            Console.WriteLine(e.Message);
        //            throw new Exception("No active tournament");
        //        }
        //    }
        //}
    }
}
