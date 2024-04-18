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
using System.Net;
using Npgsql;
using System.Data;

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
                db.GetUserByUsername(rq.Path[2]);
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
                var userStats = db.GetUserScoreboard();
                if (userStats != null)
                {
                    rs.ResponseCode = 201;
                    rs.ResponseMessage = "UserStats found successfully!";
                    rs.Content = FormatScoreboard(userStats);
                    rs.Headers.Add("Content-Type", "text/plain");
                }
                else
                {
                    rs.ResponseCode = 204; // No Content
                    rs.Content = "No scoreboard data available.";
                    rs.Headers.Add("Content-Type", "text/plain");
                }
            }
            catch (Exception ex)
            {
                rs.ResponseCode = 400;
                rs.Content = $"Error retrieving user stats: {ex.Message}";
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
                var tournaments = db.GetAllTournaments();
                if (tournaments.Count > 0)
                {
                    List<string> tournamentDetails = new List<string>();

                    foreach (var tournament in tournaments)
                    {
                        // Get the participants and their entry counts for each tournament
                        var participantEntries = db.GetPushUpEntriesByTournamentId(tournament.TournamentId);

                        // Group entries by username and sum the counts
                        var participantSummary = participantEntries
                            .GroupBy(pe => pe.Username)
                            .Select(g => new { Username = g.Key, TotalCount = g.Sum(pe => pe.Count) })
                            .ToList();

                        // Format the participant details
                        var participantDetails = participantSummary.Select(ps => $"{ps.Username} - {ps.TotalCount} push-ups").ToList();

                        // Create a message for this tournament
                        string tournamentMessage = $"Tournament {tournament.TournamentId} started at {tournament.StartTime}, State: {tournament.State}, Participants: {string.Join(", ", participantDetails)}";

                        tournamentDetails.Add(tournamentMessage);
                    }

                    rs.ResponseCode = 201;
                    rs.ResponseMessage = "Tournaments found successfully!";
                    rs.Content = string.Join("\n", tournamentDetails);
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

        public string FormatScoreboard(List<UserStats> userStatsList)
        {
            StringBuilder formattedScoreboard = new StringBuilder("Scoreboard:\n");

            foreach (var stats in userStatsList.OrderByDescending(u => u.Elo))
            {
                formattedScoreboard.AppendLine($"{stats.Username} - Elo: {stats.Elo}, Wins: {stats.Wins}, Losses: {stats.Losses}, Draws: {stats.Draws}, Total Push-Ups: {stats.Counts}");
            }

            return formattedScoreboard.ToString();
        }


    }
}

