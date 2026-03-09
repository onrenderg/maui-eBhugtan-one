using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using eBhugtanClient.Models;

namespace eBhugtanClient.Database
{
    public class Database_Payments
    {
        private SQLiteConnection conn;

        public Database_Payments()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.Dbname);
            conn = new SQLiteConnection(dbPath);
            conn.CreateTable<Payments>();
        }

        public IEnumerable<Payments> GetPayments(string query)
        {
            return conn.Query<Payments>(query).ToList();
        }

        public string AddPayments(Payments payment)
        {
            conn.Insert(payment);
            return "success";
        }

        public string DeletePayments(string accountNumber = null, int finYear = 0)
        {
            if (!string.IsNullOrEmpty(accountNumber) && finYear > 0)
                conn.Query<Payments>($"DELETE FROM Payments WHERE AccountNumber = '{accountNumber}' AND Fin_Year = {finYear}");
            else
                conn.Query<Payments>("DELETE FROM Payments");
            return "success";
        }

        public string Custom(string query)
        {
            conn.Query<Payments>(query);
            return "success";
        }
    }
}
