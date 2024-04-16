using Sports_Exercise_Battle.Server.HTTP;
using Sports_Exercise_Battle.Models.Users;
using HttpMethod = Sports_Exercise_Battle.Server.HTTP.HttpMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sports_Exercise_Battle.Database;
using Sports_Exercise_Battle.Models.Entries;

namespace Sports_Exercise_Battle.Server.SEB
{
    public class UsersEndpoint : IHttpEndpoint
    {
        DatabaseHandler db = DatabaseHandler.Instance;
        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Headers.ContainsKey("Authorization"))
            {
                db.Token = rq.Headers["Authorization"].Split(' ')[1]; // deleting Basic from curl
            }
            else
            {
                db.Token = null;
            }
            if (rq.Method == HttpMethod.POST)
            {
                if (rq.Path[1] == "users")
                {
                    if (rq.Path.Length == 2 || (rq.Path.Length > 2 && string.IsNullOrWhiteSpace(rq.Path[2])))
                    {
                        CreateUser(rq, rs);

                    }
                }
                else if (rq.Path[1] == "sessions")
                {
                    if (rq.Path.Length == 2 || (rq.Path.Length > 2 && string.IsNullOrWhiteSpace(rq.Path[2])))
                    {
                        LoginUser(rq, rs);
                    }
                }
                else if (rq.Path[1] == "history")
                {
                    if (rq.Path.Length == 2 || (rq.Path.Length > 2 && string.IsNullOrWhiteSpace(rq.Path[2])))
                    {
                        InsertNewPushUpEntry(rq, rs);
                    }
                }
                return true;
            }
            else if (rq.Method == HttpMethod.GET)
            {
                if (rq.Path[1] == "users")
                {
                    if (db.AuthorizeToken())
                    {
                        if (rq.Path.Length > 2 && (!string.IsNullOrWhiteSpace(rq.Path[2]) && (rq.Path[2] == db.AuthorizedUser)))
                        {
                            GetUsers(rq, rs);

                        }
                        else
                        {
                            rs.ResponseCode = 400;
                            rs.Content = $"Failed to get user!";
                            rs.Headers.Add("Content-Type", "application/json");
                        }
                    }
                }
                else if (rq.Path[1] == "stats")
                {
                    if (db.AuthorizeToken())
                    {
                        if (rq.Path.Length == 2 || (rq.Path.Length > 2 && string.IsNullOrWhiteSpace(rq.Path[2])))
                        {
                            GetUserStats(rq, rs);
                        }
                    }
                }
                else if (rq.Path[1] == "score")
                {
                    if (db.AuthorizeToken())
                    {
                        if (rq.Path.Length == 2 || (rq.Path.Length > 2 && string.IsNullOrWhiteSpace(rq.Path[2])))
                        {
                            GetUserScoreboard(rq, rs);
                        }
                    }
                }
                else if (rq.Path[1] == "history")
                {
                    if (db.AuthorizeToken())
                    {
                        if (rq.Path.Length == 2 || (rq.Path.Length > 2 && string.IsNullOrWhiteSpace(rq.Path[2])))
                        {
                            GetUserHistory(rq, rs);
                        }
                        

                    }
                }
                else if (rq.Path[1] == "tournament")
                {
                    if (db.AuthorizeToken())
                    {
                        if (rq.Path.Length == 2 || (rq.Path.Length > 2 && string.IsNullOrWhiteSpace(rq.Path[2])))
                        {
                            GetTournaments(rq, rs);
                        }
                       

                    }
                }

                return true;
            }
            else if (rq.Method == HttpMethod.PUT)
            {
                if (rq.Path[1] == "users")
                {
                    if (db.AuthorizeToken())
                    {

                        if (rq.Path.Length > 2 && (!string.IsNullOrWhiteSpace(rq.Path[2]) && (rq.Path[2] == db.AuthorizedUser)))
                        {
                            UpdateUser(rq, rs);

                        }
                        else
                        {
                            rs.ResponseCode = 400;
                            rs.Content = $"Failed to update user!";
                            rs.Headers.Add("Content-Type", "application/json");
                        }
                    }


                }
                return true;
            }
            return false;
        }


        public void CreateUser(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                var user = JsonSerializer.Deserialize<User>(rq.Content ?? "");
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                {
                    throw new Exception("Invalid user credentials.");
                }

                db.RegisterUser(user);


                rs.ResponseCode = 201;
                rs.ResponseMessage = "Created";
                rs.Content = JsonSerializer.Serialize(new { message = "User created successfully!" });
                rs.Headers.Add("Content-Type", "application/json");
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"Failed to create user: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

        public void UpdateUser(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                var user = JsonSerializer.Deserialize<User>(rq.Content ?? "");
                if (user != null && string.IsNullOrWhiteSpace(user.Username) && rq.Path.Length > 1 && !string.IsNullOrWhiteSpace(rq.Path[2]))
                {
                    user.Username = rq.Path[2];
                }
                if (user == null || string.IsNullOrWhiteSpace(user.Username))
                {
                    throw new Exception("Invalid user data.");
                }

                db.UpdateUser(user);


                rs.ResponseCode = 201;
                rs.ResponseMessage = "Updated";
                rs.Content = JsonSerializer.Serialize(new { message = "User updated successfully!" });
                rs.Headers.Add("Content-Type", "application/json");
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"Failed to update user: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

        public void LoginUser(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                var user = JsonSerializer.Deserialize<User>(rq.Content ?? "");
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                {
                    throw new Exception("Invalid user data.");
                }

                db.LoginUser(user.Username, user.Password);


                rs.ResponseCode = 201;
                rs.ResponseMessage = "Login successfull!";
                rs.Content = JsonSerializer.Serialize(new { message = "User logged in successfully!" });
                rs.Headers.Add("Content-Type", "application/json");
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"Failed to login: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

        public void InsertNewPushUpEntry(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                var entry = JsonSerializer.Deserialize<PushUpEntry>(rq.Content ?? "");
                db.InsertNewPushUpEntry(entry);


                rs.ResponseCode = 201;
                rs.ResponseMessage = "Entry successfull!";
                rs.Content = JsonSerializer.Serialize(new { message = "Entry inserted successfully!" });
                rs.Headers.Add("Content-Type", "application/json");
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"Failed to insert entry: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

        public void GetUsers(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                db.GetUserByID(rq.Path[2]);
                rs.ResponseCode = 201;
                rs.ResponseMessage = "Found User!";
                rs.Content = JsonSerializer.Serialize(new { message = "User found successfully!" });
                rs.Headers.Add("Content-Type", "application/json");
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"No such User!: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

        public void GetUserStats(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                var userStats = db.GetUserStats();
                rs.ResponseCode = 201;
                rs.ResponseMessage = "UserStats found successfully!";
                rs.Content = JsonSerializer.Serialize(new { message = "UserStats from " + userStats?.Username + ": Elo: " + userStats?.Elo + " TotalCounts: " + userStats?.Counts });
                rs.Headers.Add("Content-Type", "application/json");
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"No such UserStats!: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

        public void GetUserScoreboard(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                if (db.Token == null)
                {
                    return;
                }
                var userStats = db.GetUserScoreboard(); //db.Token
                rs.ResponseCode = 201;
                rs.ResponseMessage = "UserStats found successfully!";
                rs.Content = userStats;
                rs.Headers.Add("Content-Type", "application/json");
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"No such UserStats!: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

        public void GetUserHistory(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                var history = db.GetUserPushUpHistory();
                if (!string.IsNullOrWhiteSpace(history) && history != "[]")
                {
                    rs.ResponseCode = 201;
                    rs.ResponseMessage = "PushUp History found successfully!";
                    rs.Content = history;
                    rs.Headers.Add("Content-Type", "application/json");
                }
                else 
                {
                    rs.ResponseCode = 400;
                    rs.Content = $"No such History!";
                    rs.Headers.Add("Content-Type", "application/json");
                }
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"No such History!: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

        public void GetTournaments(HttpRequest rq, HttpResponse rs)
        {
            try
            {
                var tournaments = db.GetAllTournaments(); //probably because there are 2 GETS in the curl for tournaments // deleted one
                if (tournaments.Count > 0)
                {
                    rs.ResponseCode = 201;
                    rs.ResponseMessage = "Tournaments found successfully!";
                    string tournamentMessage = String.Join(", ", tournaments.Select(x => $"Tournament {x.TournamentId} started at {x.StartTime} State: {x.State} Participants: Counts: ").ToArray()); //empty stuffs have to be corrected/added
                    rs.Content = tournamentMessage;
                    rs.Headers.Add("Content-Type", "application/json");
                }
                else
                {
                    rs.ResponseCode = 400;
                    rs.Content = $"No Tournaments found!";
                    rs.Headers.Add("Content-Type", "application/json");
                }
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"No tournaments found!: {ex.Message}";
                rs.Headers.Add("Content-Type", "application/json");
            }
        }

    }
}

