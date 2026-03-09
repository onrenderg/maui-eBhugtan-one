using eBhugtanClient.Database;
using eBhugtanClient.Models;
using System.Linq;

namespace eBhugtanClient;

public partial class App : Application
{
    public static string Dbname = "eBhugtanClient.db";
    public static IEnumerable<Accounts> Accounts_List;
    public static int Fin_Year;
    public static string AccountNumber;

    public App()
    {
        InitializeComponent();

        var db = new Database_Accounts();
        Accounts_List = db.GetAccounts("SELECT * FROM Accounts");

        if (Accounts_List != null && Accounts_List.Any())
        {
            var active = Accounts_List.First(); // For now pick first
            AccountNumber = active.AccountNumber;
            Fin_Year = active.Fin_Year;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (Accounts_List != null && Accounts_List.Any())
        {
            return new Window(new NavigationPage(new TabViewPage()));
        }

        return new Window(new NavigationPage(new LoginPage()));
    }
}