using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Microsoft.Maui.Controls;
namespace eBhugtan
{
    public class Database_Payments
    {
        private SQLiteConnection conn;
        //CREATE  
        public Database_Payments()
        {
            //conn = DependencyService.Get<ISQLite>().GetConnection();
            //conn.CreateTable<Payments>();
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.Dbname);
            conn = new SQLiteConnection(dbPath);
            conn.CreateTable<Payments>();
        }
        //READ  
        public IEnumerable<Payments> GetPayments(String Querryhere)
        {
            var list = conn.Query<Payments>(Querryhere);
            return list.ToList();
        }
        //INSERT  
        public string AddPayments(Payments loginmodel)
        {
            conn.Insert(loginmodel);
            return "success";
        }
        //DELETE (scoped to account+year so other accounts are not wiped)
        public string DeletePayments(string accountNumber = null, int finYear = 0)
        {
            if (!string.IsNullOrEmpty(accountNumber) && finYear > 0)
                conn.Query<Payments>(
                    $"DELETE FROM Payments WHERE AccountNumber = '{accountNumber}' AND Fin_Year = {finYear}");
            else
                conn.Query<Payments>("DELETE FROM Payments");
            return "success";
        }
        public string Custom(string query)
        {
            var del = conn.Query<Payments>(query);
            return "success";
        }
    }
}
