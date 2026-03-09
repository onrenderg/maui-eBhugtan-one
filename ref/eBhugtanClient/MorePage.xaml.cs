using eBhugtanClient.Database;
using eBhugtanClient.Models;
using System.Collections.ObjectModel;

namespace eBhugtanClient;

public partial class MorePage : ContentPage
{
    private Database_Accounts _dbAccounts;
    public ObservableCollection<Accounts> AccountsList { get; set; } = new ObservableCollection<Accounts>();

    public MorePage()
    {
        InitializeComponent();
        _dbAccounts = new Database_Accounts();
        AccountsListView.ItemsSource = AccountsList;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAccounts();
    }

    private void LoadAccounts()
    {
        var data = _dbAccounts.GetAccounts("SELECT * FROM Accounts");
        AccountsList.Clear();
        foreach (var acc in data)
        {
            AccountsList.Add(acc);
        }
    }

    private async void DeleteBtn_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var account = button?.CommandParameter as Accounts;

        if (account != null)
        {
            bool confirm = await DisplayAlert("Confirm", $"Delete account {account.AccountNumber}?", "Yes", "No");
            if (confirm)
            {
                _dbAccounts.Custom($"DELETE FROM Accounts WHERE IDs = {account.IDs}");
                // Also clean up payments
                var dbPayments = new Database_Payments();
                dbPayments.DeletePayments(account.AccountNumber, account.Fin_Year);
                
                LoadAccounts();

                if (App.AccountNumber == account.AccountNumber)
                {
                    App.AccountNumber = null;
                    App.Fin_Year = 0;
                    if (AccountsList.Count == 0)
                    {
                        Application.Current.MainPage = new NavigationPage(new LoginPage());
                    }
                }
            }
        }
    }

    private async void AddAccount_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }

    private async void SyncAll_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Sync", "Triggering background sync for all accounts...", "OK");
        // Logic to iterate and call HitServices could go here
    }
}
