using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using eBhugtanClient.Models;

namespace eBhugtanClient.Database
{
    public class Database_Accounts
    {
        private SQLiteConnection conn;

        public Database_Accounts()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.Dbname);
            conn = new SQLiteConnection(dbPath);
            conn.CreateTable<Accounts>();
        }

        public IEnumerable<Accounts> GetAccounts(string query)
        {
            return conn.Query<Accounts>(query).ToList();
        }

        public string AddAccounts(Accounts account)
        {
            conn.Insert(account);
            return "success";
        }

        public string DeleteAccounts()
        {
            conn.Query<Accounts>("delete from Accounts");
            return "success";
        }

        public string Custom(string query)
        {
            conn.Query<Accounts>(query);
            return "success";
        }
    }
}
