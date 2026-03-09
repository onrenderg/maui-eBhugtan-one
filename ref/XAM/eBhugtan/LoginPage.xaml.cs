using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
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
            if (string.IsNullOrEmpty(Username.Text))
            {
                await DisplayAlert("eBhugtan", "Please Enter First Three Letters of Your Name", "OK");
                Username.Focus();
                return;
            }
            else
            {
                activity1.IsVisible = true;
                await ServiceGetRequest();
            }
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
                        database_Payments.DeletePayments();
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
                                }
                            }
                            if (pair.Key == "otherPayments")
                            {
                                var nodes = pair.Value;
                                var item = new Payments();
                                item.DataRefreshDate = DateTime.Now.ToString("dd-MM-yyyy");
                                item.DataRefreshTime = DateTime.Now.ToString("HH:mm");
                                foreach (var node in nodes)
                                {
                                    item.BillID = node["BillID"].ToString();
                                    item.BillType = node["BillType"].ToString();
                                    item.DDOCode = node["DDOCode"].ToString();
                                    item.GrossAmount = node["GrossAmount"].ToString();
                                    item.NetAmount = node["NetAmount"].ToString();
                                    item.PaidOn = node["PaidOn"].ToString();
                                    //Console.WriteLine(item.PaidOn);
                                    DateTime AsofDate = DateTime.ParseExact(item.PaidOn, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    item.Payed_Month = AsofDate.ToString("MMMM, yyyy");
                                    item.Sortable_Date = AsofDate.ToString("yyyy-MM-dd");
                                    item.PayeeCode = node["PayeeCode"].ToString();
                                    item.Remarks = node["Remarks"].ToString();
                                    item.Treasury = node["Treasury"].ToString();
                                    item.AccountNumber = AcNo.Text;
                                    item.Fin_Year = int.Parse(Picker_FinYear.Items[Picker_FinYear.SelectedIndex]);
                                    database_Payments.AddPayments(item);
                                }
                            }
                        }
                        database_Accounts.DeleteAccounts();
                        var item1 = new Accounts();
                        item1.AccountNumber = AcNo.Text;
                        item1.Fin_Year = int.Parse(Picker_FinYear.Items[Picker_FinYear.SelectedIndex]);
                        item1.SelectedAccount = 1;
                        database_Accounts.AddAccounts(item1);
                        App.Accounts_List = database_Accounts.GetAccounts("select * from accounts");
                        Application.Current.MainPage = new NavigationPage(new TabViewPage("sync"));
                    }
                }
                catch(Exception e)
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
