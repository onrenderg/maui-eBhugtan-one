using System;
using System.IO;
using Microsoft.Maui.Controls;
using SQLite;
using eBhugtan.iOS;

[assembly: Dependency(typeof(IOS_SQLite))]
namespace eBhugtan.iOS
{
    public class IOS_SQLite : ISQLite
    {
        public SQLiteConnection GetConnection()
        {
            var dbName = "eBhugtan2019.sqlite";
            string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder  
            string libraryPath = Path.Combine(dbPath, "..", "Library"); // Library folder  
            var path = Path.Combine(libraryPath, dbName);
            var conn = new SQLite.SQLiteConnection(path);
            return conn;
        }
    }
}
