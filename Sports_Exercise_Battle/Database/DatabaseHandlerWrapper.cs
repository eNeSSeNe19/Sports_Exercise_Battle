using Sports_Exercise_Battle.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sports_Exercise_Battle.Database
{
    public class DatabaseHandlerWrapper : IDatabasehandler
    {
        private readonly DatabaseHandler _databaseHandler;
        public DatabaseHandlerWrapper() 
        {
            // You can also pass in any required parameters for DatabaseHandler constructor here if there are any
            _databaseHandler = new DatabaseHandler();
        }

        public int RegisterUser(User user)
        {
            return _databaseHandler.RegisterUser(user);
        }
    }
}
