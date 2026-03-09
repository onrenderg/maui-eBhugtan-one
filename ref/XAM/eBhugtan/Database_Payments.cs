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
            conn = DependencyService.Get<ISQLite>().GetConnection();
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
        //DELETE  
        public string DeletePayments()
        {
            var del = conn.Query<Payments>("delete from Payments");
            return "success";
        }
        public string Custom(string query)
        {
            var del = conn.Query<Payments>(query);
            return "success";
        }
    }
}
