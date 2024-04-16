using Sports_Exercise_Battle.Models.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sports_Exercise_Battle.Models.Users
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }

        public List<Tournament> PushUpHistory { get; set; }

        public User(string username, string password = "", string name = "", string bio = "", string image = "") 
        {
            Username = username;
            Password = password;
            Name = name;
            Bio = bio;
            Image = image;
            PushUpHistory = new List<Tournament>() { };
        }

       

        public UserStats Stats
        {
            get;
            private set;
        }

    }
}
