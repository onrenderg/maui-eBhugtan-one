using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace eBhugtan
{
    public partial class LoginPage : ContentPage
    {
        Database_Accounts database_Accounts;
        Database_Payments database_Payments;
        public LoginPage()
        {
            InitializeComponent();
            database_Payments = new Database_Payments();
            database_Accounts = new Database_Accounts();
            Picker_FinYear.ItemsSource = App.YearsList;
            Picker_FinYear.SelectedIndex = 0;
        }
        private async void LoginBtn(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AcNo.Text))
            {
                await DisplayAlert("eBhugtan", "Please Enter Account Number", "OK");
                AcNo.Focus();
                return;
            }
            activity1.IsVisible = true;
            int finYear = int.Parse(Picker_FinYear.Items[Picker_FinYear.SelectedIndex]);
            var svc = new HitServices();
            int status = await svc.FetchPaymentsAsync(AcNo.Text, finYear,
                             database_Accounts, database_Payments, clearExisting: true);
            activity1.IsVisible = false;

            switch (status)
            {
                case 200:
                    database_Accounts.DeleteAccounts();
                    database_Accounts.AddAccounts(new Accounts
                    {
                        AccountNumber   = AcNo.Text,
                        Fin_Year        = finYear,
                        SelectedAccount = 1
                    });
                    App.Accounts_List = database_Accounts.GetAccounts("select * from accounts");
                    Application.Current.MainPage = new NavigationPage(new TabViewPage("sync"));
                    break;

                case 204:
                    await DisplayAlert("eBhugtan",
                        "It seems you are offline. Please check your internet connection.", "OK");
                    break;

                case 300:
                    await DisplayAlert("eBhugtan",
                        "No Records Found or invalid credentials.", "OK");
                    break;

                default:
                    await DisplayAlert("eBhugtan",
                        "Unable to retrieve data from server.\nPlease try again.", "OK");
                    break;
            }
        }

    }
}
