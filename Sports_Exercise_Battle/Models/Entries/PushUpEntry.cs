using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sports_Exercise_Battle.Models.Entries
{
    public class PushUpEntry
    {
        public string Username { get; set; }
        public int Count { get; set; }
        public int Duration { get; set; }
        public DateTime EntryTime { get; set; }
    }
}
