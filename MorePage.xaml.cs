using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace eBhugtan
{
    // Wrapper class for checkbox binding in the account list
    public class SelectableAccount
    {
        public string AccountNumber { get; set; }
        public bool IsSelected { get; set; }
    }

    public partial class MorePage : ContentPage
    {
        Database_Accounts database_Accounts;
        Database_Payments database_Payments;

        public MorePage()
        {
            InitializeComponent();
            database_Payments = new Database_Payments();
            database_Accounts = new Database_Accounts();
        }

        void AddRemoveBtn(object sender, EventArgs e)
        {
            // Build selectable list with checkboxes
            var selectableList = App.Accounts_List
                .Select(a => new SelectableAccount
                {
                    AccountNumber = a.AccountNumber,
                    IsSelected = false
                }).ToList();

            Content_DetailedList.ItemsSource = selectableList;
            TitleOfContent.Text = "Select Accounts To Remove";
            Lbl_Add_new.Text = "Add Account";
            Btn_Delete.IsVisible = true;
            Content_DetailedList.IsVisible = true;
            Grid_Add_Account.IsVisible = false;
            popupDetails.IsVisible = true;
        }

        void Close_Btn(object s, EventArgs e)
        {
            popupDetails.IsVisible = false;
        }

        async void DeleteSelected_Btn(object s, EventArgs e)
        {
            var items = Content_DetailedList.ItemsSource as List<SelectableAccount>;
            if (items == null) return;

            var selectedItems = items.Where(x => x.IsSelected).ToList();

            if (!selectedItems.Any())
            {
                await DisplayAlert("eBhugtan", "Please select at least one account to delete.", "OK");
                return;
            }

            // Check if user is trying to delete ALL accounts
            if (selectedItems.Count == App.Accounts_List.Count())
            {
                var confirmAll = await DisplayAlert("eBhugtan",
                    "Are you sure you want to remove ALL accounts from the App?", "Yes", "Cancel");
                if (confirmAll)
                {
                    database_Accounts.DeleteAccounts();
                    database_Payments.DeletePayments();
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
                return;
            }

            var confirm = await DisplayAlert("eBhugtan",
                "Are you sure you want to remove the selected account(s)?", "Yes", "Cancel");

            if (confirm)
            {
                foreach (var item in selectedItems)
                {
                    database_Accounts.Custom(
                        $"delete from accounts where AccountNumber == '{item.AccountNumber}'");
                    await Task.Delay(15);
                    database_Payments.Custom(
                        $"delete from payments where AccountNumber == '{item.AccountNumber}'");
                    await Task.Delay(15);
                }

                database_Accounts.Custom(
                    "update Accounts set SelectedAccount = CASE WHEN IDs = (SELECT min(IDS) FROM Accounts) THEN 1 ELSE 0 END");
                await Task.Delay(20);
                App.Accounts_List = database_Accounts.GetAccounts("Select * from Accounts");
                popupDetails.IsVisible = false;
                Application.Current.MainPage = new NavigationPage(new TabViewPage("3"));
            }
        }

        async void Submit_Btn(object s, EventArgs e)
        {
            if (Content_DetailedList.IsVisible)
            {
                // Switch to Add Account form
                TitleOfContent.Text = "Add Account";
                AcNo.Text = null;
                AcNo.Placeholder = "Enter Account Number";
                Content_DetailedList.IsVisible = false;
                Grid_Add_Account.IsVisible = true;
                Btn_Delete.IsVisible = false;
                Lbl_Add_new.Text = "Submit";
            }
            else
            {
                // Submit new account
                if (string.IsNullOrEmpty(AcNo.Text))
                {
                    await DisplayAlert("eBhugtan", "Please Enter Account Number", "OK");
                    AcNo.Focus();
                    return;
                }
                if (App.Accounts_List.Where(x => x.AccountNumber == AcNo.Text).Count() == 0)
                {
                    activity1.IsVisible = true;
                    await AddAccountServiceRequest();
                }
                else
                {
                    await DisplayAlert("eBhugtan", "Account Already Exists", "OK");
                }
            }
        }

        void NIC_Call(object sender, EventArgs e)
        {
            Launcher.TryOpenAsync("tel:01772622132");
        }
        async void NIC_Website(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://himkosh.nic.in/", BrowserLaunchMode.SystemPreferred);
        }
        async void NIC_Email(object sender, EventArgs e)
        {
            await Launcher.OpenAsync(new Uri("mailto:dirtre-hp@nic.in"));
        }
        async void PrivacyPolicy(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://mobileappshp.nic.in/assets/pdf/mobile-app-privacy-policy/eBhugtan.html", BrowserLaunchMode.SystemPreferred);
        }

        private async Task AddAccountServiceRequest()
        {
            int finYear = App.Fin_Year;
            var svc = new HitServices();
            int status = await svc.FetchPaymentsAsync(AcNo.Text, finYear,
                             database_Accounts, database_Payments, clearExisting: false);
            activity1.IsVisible = false;

            switch (status)
            {
                case 200:
                    database_Accounts.AddAccounts(new Accounts
                    {
                        AccountNumber   = AcNo.Text,
                        Fin_Year        = finYear,
                        SelectedAccount = 0
                    });
                    await DisplayAlert("eBhugtan", "Account Added Successfully", "OK");
                    App.Accounts_List = database_Accounts.GetAccounts("Select * from Accounts");
                    Application.Current.MainPage = new NavigationPage(new TabViewPage("3"));
                    break;

                case 300:
                    // No data for selected year, but still add the account
                    database_Accounts.AddAccounts(new Accounts
                    {
                        AccountNumber   = AcNo.Text,
                        Fin_Year        = finYear,
                        SelectedAccount = 0
                    });
                    await DisplayAlert("eBhugtan",
                        "Account added. No data found for the selected year. You can change the year later.", "OK");
                    App.Accounts_List = database_Accounts.GetAccounts("Select * from Accounts");
                    Application.Current.MainPage = new NavigationPage(new TabViewPage("3"));
                    break;

                case 204:
                    await DisplayAlert("eBhugtan",
                        "It seems you are offline. Please check your internet connection.", "OK");
                    break;

                default:
                    await DisplayAlert("eBhugtan",
                        "Unable to retrieve data from server.\nPlease try again.", "OK");
                    break;
            }
        }
    }
}
