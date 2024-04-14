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
                       // cmd.Parameters.Add(new NpgsqlParameter("p6", DbType.String));
                        cmd.Prepare();
                        cmd.Parameters["p1"].Value = user.Username;
                        cmd.Parameters["p2"].Value = user.Password;
                        cmd.Parameters["p3"].Value = user.Name == null ? "" : user.Name;
                        //cmd.Parameters["p4"].Value = user.Token == null ? "" : user.Token;
                        cmd.Parameters["p4"].Value = user.Bio == null ? "" : user.Bio;
                        cmd.Parameters["p5"].Value = user.Image == null ? "" : user.Image;



                        cmd.ExecuteNonQuery();




                        return 0;
                    }
                    catch (PostgresException e)
                    {
                        Console.WriteLine(e.Message);
                        throw new Exception("User already exists!");
                        //return 1;
                    }
                }
                else
                {
                    throw new Exception("No Database connection!");
                    //return -1;
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
                        //return 1;
                    }
                }
                else
                {
                    throw new Exception("No Database connection!");
                    //return -1;
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
                            insertNewToken(username, token);
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

        private void insertNewToken(string username, string token)
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
                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE public.\"userstats\" SET elo = @p1, wins = @p2, losses = @p3, draws = @p4  WHERE username = @p5;", connection);

                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p3", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p4", DbType.Int32));
                        cmd.Parameters.Add(new NpgsqlParameter("p5", DbType.String));
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
                        cmd.Parameters["p5"].Value = stats.Username;

                        cmd.ExecuteNonQuery();
                    }
                    catch (PostgresException)
                    {
                        Console.WriteLine("ERR: updating stats");
                    }
                }
            }
        }

        public UserStats? GetUserStats(string username)
        {
            lock (padlock)
            {
                if (connection != null && username != null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, elo, wins, losses, draws FROM public.\"userstats\" WHERE username = @p1;", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();
                    cmd.Parameters["p1"].Value = username;

                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        UserStats userStats = new UserStats();
                        userStats.Username = (string)dr[0];
                        userStats.Elo = (int)dr[1];
                        userStats.Wins = (int)dr[2];
                        userStats.Losses = (int)dr[3];
                        userStats.Draws = (int)dr[4];

                        dr.Close();
                        return userStats;
                    }
                    else
                    {
                        dr.Close();
                        return null;
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
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, elo, wins, losses, draws FROM public.\"userstats\" ORDER BY elo DESC;", connection);
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


    }
}
