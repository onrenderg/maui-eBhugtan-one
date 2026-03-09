using SQLite;

namespace eBhugtanClient
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
