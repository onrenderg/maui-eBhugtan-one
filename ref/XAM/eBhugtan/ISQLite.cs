using System;
namespace eBhugtan
{
    public interface ISQLite
    {
        SQLite.SQLiteConnection GetConnection();
    }
}
