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

namespace Sports_Exercise_Battle.Server.SEB
{
    public class UsersEndpoint : IHttpEndpoint
    {
        DatabaseHandler db = DatabaseHandler.Instance;
        public bool HandleRequest(HttpRequest rq, HttpResponse rs)
        {
            if (rq.Headers.ContainsKey("Authorization"))
            {
                db.Token = rq.Headers["Authorization"].Split(' ')[1]; // deleting Basic
            }
            else
            {
                db.Token = null;
            }
            if (rq.Method == HttpMethod.POST)
            {
                if (rq.Path[1] == "users")
                {
                    if (rq.Path.Length == 2 || (rq.Path.Length > 2 && string.IsNullOrWhiteSpace(rq.Path[2]))) // was passiert wenns garnicht existiert? -> not empty or no whitespace?  
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

                // TODO: Add logic to interact with the database to save the user
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
                if(user != null &&string.IsNullOrWhiteSpace(user.Username) && rq.Path.Length > 1 && !string.IsNullOrWhiteSpace(rq.Path[2]))
                {
                    user.Username = rq.Path[2];
                }
                if (user == null || string.IsNullOrWhiteSpace(user.Username))
                {
                    throw new Exception("Invalid user data.");
                }

                // TODO: Add logic to interact with the database to save the user
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

                // TODO: Add logic to interact with the database to save the user
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

        public void GetUsers(HttpRequest rq, HttpResponse rs)
        {
            // TODO: Add logic to retrieve users from the database
            // The data returned should exclude sensitive information like passwords

            //var users = new List<User>();


            //rs.Content = JsonSerializer.Serialize(users);
            //rs.Headers.Add("Content-Type", "application/json");


            try
            {
                //var user = JsonSerializer.Deserialize<User>(rq.Content ?? "");
                //if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                //{
                //    throw new Exception("Invalid user data.");
                //}

                // TODO: Add logic to interact with the database to save the user
                db.GetUserByID(rq.Path[2]);


                rs.ResponseCode = 201;
                rs.ResponseMessage = "Found!";
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

        //bool
}
}
