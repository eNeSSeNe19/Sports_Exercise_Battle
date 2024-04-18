using Sports_Exercise_Battle.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sports_Exercise_Battle.Database
{
    public interface IDatabasehandler
    {
        int RegisterUser(User user);
    }
}
