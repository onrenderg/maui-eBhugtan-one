using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace eBhugtan
{
    public partial class DDOWisePage : ContentPage
    {
        Database_Accounts database_Accounts;
        Database_Payments database_Payments;
        string PageAcountNumber;
        int PageFinYear;
        public DDOWisePage()
        {
            InitializeComponent();
            database_Payments = new Database_Payments();
            database_Accounts = new Database_Accounts();
            Picker_Account.SelectedIndexChanged += Picker_Account_SelectedIndexChanged;
            Picker_fYear.SelectedIndexChanged += Picker_fYear_SelectedIndexChanged;
            DetailedList.ItemTapped += DetailedList_ItemTapped;

            Picker_Account.ItemsSource = App.Accounts_List.ToList();
            Picker_Account.ItemDisplayBinding = new Binding("AccountNumber");
            App.AccountNumber = App.Accounts_List.Where(x => x.SelectedAccount == 1).ElementAt(0).AccountNumber;
            Picker_Account.Title = App.AccountNumber;

            Picker_fYear.ItemsSource = App.YearsList;
            App.Fin_Year = App.Accounts_List.Where(x => x.SelectedAccount == 1).ElementAt(0).Fin_Year;
            Picker_fYear.Title = App.Fin_Year.ToString();

            PageFinYear = App.Fin_Year;
            PageAcountNumber = App.AccountNumber;
            LoadCachedData();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Sync pickers and data with current globals when tab becomes visible
            PageAcountNumber = App.AccountNumber;
            PageFinYear = App.Fin_Year;
            Picker_Account.Title = App.AccountNumber;
            Picker_fYear.Title = App.Fin_Year.ToString();
            LoadCachedData();
        }

        private async void Picker_fYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Picker_fYear.SelectedIndex != -1)
            {
                PageFinYear = int.Parse(Picker_fYear.Items[Picker_fYear.SelectedIndex]);
                PageAcountNumber = App.AccountNumber;
                await LoadLocalOrFetchRemote();
            }
            else
            {
                Picker_fYear.SelectedItem = App.Fin_Year;
                return;
            }
        }

        private async void Picker_Account_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Picker_Account.SelectedIndex != -1)
            {
                PageAcountNumber = Picker_Account.Items[Picker_Account.SelectedIndex];
                PageFinYear = App.Fin_Year;
                await LoadLocalOrFetchRemote();
            }
            else
            {
                Picker_Account.SelectedItem = App.AccountNumber;
                return;
            }
        }

        /// <summary>
        /// Check local DB first. If data exists, show it immediately.
        /// If no local data, call the remote API.
        /// </summary>
        private async Task LoadLocalOrFetchRemote()
        {
            var localData = database_Payments.GetPayments(
                $"SELECT *,(substr(BillID,1,5) || ' - ' || DDOCode || ' (' || count(DDOCode) || ')')FreeCell1," +
                $"('₹ ' || sum(NetAmount))FreeCell2 from Payments " +
                $"where AccountNumber = '{PageAcountNumber}' and Fin_Year = {PageFinYear} " +
                $"GROUP by DDOCode ORDER by Sortable_Date DESC");

            if (localData.Any())
            {
                // Local data found — show it without calling API
                App.AccountNumber = PageAcountNumber;
                App.Fin_Year      = PageFinYear;
                database_Accounts.Custom(
                    $"update Accounts set Fin_Year = CASE WHEN AccountNumber = '{App.AccountNumber}' " +
                    $"THEN {App.Fin_Year} else Fin_Year END, SelectedAccount = CASE WHEN AccountNumber " +
                    $"= '{App.AccountNumber}' THEN 1 ELSE 0 END");
                App.Accounts_List = database_Accounts.GetAccounts("select * from accounts");

                DetailedList.ItemsSource = localData;
                Refresh_Line.Text = $"Data Refresh {localData.ElementAt(0).DataRefreshDate} at {localData.ElementAt(0).DataRefreshTime}";
                var bottom = database_Payments.GetPayments(
                    $"SELECT sum(NetAmount)FreeCell3,count(DDOCode)FreeCell4 from Payments " +
                    $"where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year}");
                Total_Line.Text = $"Total Amount : ₹ {bottom.ElementAt(0).FreeCell3} For {bottom.ElementAt(0).FreeCell4} Bills";
            }
            else
            {
                // No local data — fetch from remote API
                await ExecuteServiceRequest();
            }
        }

        private void DetailedList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var currentrecord = e.Item as Payments;
            TitleofContent.Text = currentrecord.FreeCell1;
            Content_DetailedList.ItemsSource = database_Payments.GetPayments($"select * from Payments where DDOCode = '{currentrecord.DDOCode}'");
            popupDetails.IsVisible = true;
        }

        void Close_Btn(object s, EventArgs e)
        {
            popupDetails.IsVisible = false;
        }

        private async Task ExecuteServiceRequest()
        {
            activity1.IsVisible = true;
            var svc = new HitServices();
            int status = await svc.FetchPaymentsAsync(PageAcountNumber, PageFinYear,
                             database_Accounts, database_Payments, clearExisting: true);
            activity1.IsVisible = false;

            switch (status)
            {
                case 200:
                    App.AccountNumber = PageAcountNumber;
                    App.Fin_Year      = PageFinYear;
                    database_Accounts.Custom(
                        $"update Accounts set Fin_Year = CASE WHEN AccountNumber = '{App.AccountNumber}' " +
                        $"THEN {App.Fin_Year} else Fin_Year END, SelectedAccount = CASE WHEN AccountNumber " +
                        $"= '{App.AccountNumber}' THEN 1 ELSE 0 END");
                    App.Accounts_List = database_Accounts.GetAccounts("select * from accounts");
                    Application.Current.MainPage = new NavigationPage(new TabViewPage("2"));
                    break;

                case 204:
                    await DisplayAlert("eBhugtan",
                        "It seems you are offline. Please check your internet connection.", "OK");
                    LoadCachedData();
                    break;

                case 300:
                    App.AccountNumber = PageAcountNumber;
                    App.Fin_Year      = PageFinYear;
                    database_Accounts.Custom(
                        $"update Accounts set Fin_Year = CASE WHEN AccountNumber = '{App.AccountNumber}' " +
                        $"THEN {App.Fin_Year} else Fin_Year END, SelectedAccount = CASE WHEN AccountNumber " +
                        $"= '{App.AccountNumber}' THEN 1 ELSE 0 END");
                    App.Accounts_List = database_Accounts.GetAccounts("select * from accounts");
                    await DisplayAlert("eBhugtan", "No data for selected year.", "OK");
                    Application.Current.MainPage = new NavigationPage(new TabViewPage("2"));
                    break;

                default:
                    await DisplayAlert("eBhugtan",
                        "Unable to retrieve data from server.\nPlease try again.", "OK");
                    break;
            }
        }

        private void LoadCachedData()
        {
            var data = database_Payments.GetPayments(
                $"SELECT *,(substr(BillID,1,5) || ' - ' || DDOCode || ' (' || count(DDOCode) || ')')FreeCell1," +
                $"('₹ ' || sum(NetAmount))FreeCell2 from Payments " +
                $"where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year} " +
                $"GROUP by DDOCode ORDER by Sortable_Date DESC");
            DetailedList.ItemsSource = data;
            if (data.Any())
            {
                Refresh_Line.Text = $"Data Refresh {data.ElementAt(0).DataRefreshDate} at {data.ElementAt(0).DataRefreshTime}";
                var bottom = database_Payments.GetPayments(
                    $"SELECT sum(NetAmount)FreeCell3,count(DDOCode)FreeCell4 from Payments " +
                    $"where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year}");
                Total_Line.Text = $"Total Amount : ₹ {bottom.ElementAt(0).FreeCell3} For {bottom.ElementAt(0).FreeCell4} Bills";
            }
            else
            {
                Refresh_Line.Text = "";
                Total_Line.Text = "";
            }
        }
    }
}
