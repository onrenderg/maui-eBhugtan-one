using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Microsoft.Maui.Controls;
namespace eBhugtan
{
    public class Database_Accounts
    {
        private SQLiteConnection conn;
        //CREATE  
        public Database_Accounts()
        {
            //conn = DependencyService.Get<ISQLite>().GetConnection();
            //conn.CreateTable<Accounts>();
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.Dbname);
            conn = new SQLiteConnection(dbPath);
            conn.CreateTable<Accounts>();
        }
        //READ  
        public IEnumerable<Accounts> GetAccounts(String Querryhere)
        {
            var list = conn.Query<Accounts>(Querryhere);
            return list.ToList();
        }
        //INSERT  
        public string AddAccounts(Accounts loginmodel)
        {
            conn.Insert(loginmodel);
            return "success";
        }
        //DELETE  
        public string DeleteAccounts()
        {
            var del = conn.Query<Accounts>("delete from Accounts");
            return "success";
        }
        public string Custom(string query)
        {
            var del = conn.Query<Accounts>(query);
            return "success";
        }
    }
}
