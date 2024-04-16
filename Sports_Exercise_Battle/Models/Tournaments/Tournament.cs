﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sports_Exercise_Battle.Models.Entries
{
    public class Tournament
    {
        public int TournamentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string State { get; set; }

        public bool Is_Calculated { get; set; }

    }
}
