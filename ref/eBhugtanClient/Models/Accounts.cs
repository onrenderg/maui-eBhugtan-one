using System;
using SQLite;

namespace eBhugtanClient.Models
{
    public class Accounts
    {
        [PrimaryKey, AutoIncrement]
        public int IDs { get; set; }
        public string AccountNumber { get; set; }
        public int Fin_Year { get; set; }
        public int SelectedAccount { get; set; }
    }
}
