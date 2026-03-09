using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using Xamarin.Essentials;
using Microsoft.Maui.Controls;

namespace eBhugtan
{
    public partial class MorePage : ContentPage
    {
        Database_Accounts database_Accounts;
        Database_Payments database_Payments;
        public MorePage()
        {
            InitializeComponent();
            database_Payments = new Database_Payments();
            database_Accounts = new Database_Accounts();
            Content_DetailedList.ItemTapped += Content_DetailedList_ItemTapped;
        }

        private async void Content_DetailedList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var MyPopup = await DisplayAlert("eBhugtan", "Are you sure You Want to Remove Account From App?", "Yes", "Cancel");
            if (App.Accounts_List.Count() == 1)
            {
                if (MyPopup == true)
                {
                    database_Accounts.DeleteAccounts();
                    database_Payments.DeletePayments();
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
                else
                {
                    return;
                }
            }else
            {
                if (MyPopup == true)
                {
                    var currentrecord = e.Item as Accounts;
                    database_Accounts.Custom($"delete from accounts where AccountNumber == '{currentrecord.AccountNumber}'");
                    await Task.Delay(15);
                    database_Accounts.Custom($"update Accounts set SelectedAccount = CASE WHEN IDs = (SELECT min(IDS) FROM Accounts) THEN 1 ELSE 0 END");
                    await Task.Delay(15);
                    database_Payments.Custom($"delete from payments where AccountNumber == '{currentrecord.AccountNumber}'");
                    await Task.Delay(20);
                    App.Accounts_List = database_Accounts.GetAccounts("Select * from Accounts");
                    popupDetails.IsVisible = false;
                    Application.Current.MainPage = new NavigationPage(new TabViewPage("3"));
                }
                else
                {
                    return;
                }
            }
        }

        void AddRemoveBtn(object sender, System.EventArgs e)
        {
            if (!Picker_FinYear.Items.Any())
            {
                Picker_FinYear.ItemsSource = App.YearsList;
                Picker_FinYear.SelectedIndex = 0;
            }
            else
            {
                Picker_FinYear.SelectedIndex = 0;
            }
            Content_DetailedList.ItemsSource = App.Accounts_List;
            TitleOfContent.Text = "Tap Account To Remove";
            Lbl_Add_new.Text = "Add Account";
            Content_DetailedList.IsVisible = true;
            Grid_Add_Account.IsVisible = false;
            popupDetails.IsVisible = true;
        }
        void Close_Btn(object s, EventArgs e)
        {
            popupDetails.IsVisible = false;
        }
        async void Submit_Btn(object s, EventArgs e)
        {
            if (TitleOfContent.Text == "Tap Account To Remove")
            {
                TitleOfContent.Text = "Add Account";
                AcNo.Text = null;
                Username.Text = null;
                AcNo.Placeholder = "Enter Account Number";
                Username.Placeholder = "First 3 Letters of Name";
                Content_DetailedList.IsVisible = false;
                Grid_Add_Account.IsVisible = true;
                Lbl_Add_new.Text = "Submit";
            }else
            {
                
                if (string.IsNullOrEmpty(AcNo.Text))
                {
                    await DisplayAlert("eBhugtan", "Please Enter Account Number", "OK");
                    AcNo.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(Username.Text))
                {
                    await DisplayAlert("eBhugtan", "Please Enter First Three Letters of Your Name", "OK");
                    Username.Focus();
                    return;
                }
                else
                {
                    if (App.Accounts_List.Where(x => x.AccountNumber == AcNo.Text).Count() == 0)
                    {
                        activity1.IsVisible = true;
                        await ServiceGetRequest();
                    }else
                    {
                        await DisplayAlert("eBhugtan", "Account Already Exists", "OK");
                    }
                }
            }
        }
        void NIC_Call(object sender, EventArgs e)
        {
            Launcher.TryOpenAsync("tel:01772624045");
        }
        void NIC_Website(object sender, EventArgs e)
        {
            Launcher.TryOpenAsync("https://himachal.nic.in/en-IN/nichp.html");
        }
        void OurAppsBtn(object sender, EventArgs e)
        {
            Launcher.TryOpenAsync("https://mobileappshp.nic.in/EN-IN/");
        }
        private async Task<string> ServiceGetRequest()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                activity1.IsVisible = true;
                HttpResponseMessage responce;
                try
                {
                    var client = new HttpClient();
                    responce = await client.GetAsync($"https://himkosh.nic.in/ehpoltis/wcfServices/OtherPayment.svc/GetPayeeOtherPayments?payeeCode=IP99-99999&paymentMonth=&paymentYear={Picker_FinYear.SelectedItem.ToString()}&bankAccNo={AcNo.Text}");
                    var OtherPaymnetJson = await responce.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(OtherPaymnetJson))
                    {
                        JObject parsed = JObject.Parse(OtherPaymnetJson);
                        //database_Payments.DeletePayments();
                        foreach (var pair in parsed)
                        {
                            if (pair.Key == "message")
                            {
                                var nodesM = pair.Value;
                                string Message = nodesM["message"].ToString();
                                if (Message != "Success")
                                {
                                    activity1.IsVisible = false;
                                    await DisplayAlert("eBhugtan", Message, "OK");
                                    return null;
                                }else
                                {
                                    var itemAccount = new Accounts();
                                    itemAccount.AccountNumber = AcNo.Text;
                                    itemAccount.Fin_Year = Picker_FinYear.SelectedIndex;
                                    database_Accounts.AddAccounts(itemAccount);
                                    activity1.IsVisible = false;
                                    await DisplayAlert("eBhugtan", "Account Added Successfully", "OK");
                                    App.Accounts_List = database_Accounts.GetAccounts("Select * from Accounts");
                                    Application.Current.MainPage = new NavigationPage(new TabViewPage("sync"));

                                }
                            }
                        }
                    }
                }
                catch
                {
                    activity1.IsVisible = false;
                    await DisplayAlert("eBhugtan", "Unable to Retrive Data from Server \n Please Try again", "OK");
                }
            }
            else
            {
                activity1.IsVisible = false;
                await DisplayAlert("eBhugtan", "It seems that you are offline. Please check your internet connection", "OK");
            }
            activity1.IsVisible = false;
            return "ok";
        }
    }
}
