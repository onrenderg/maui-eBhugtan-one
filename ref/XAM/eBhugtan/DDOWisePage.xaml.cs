using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
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

            var MyListData = database_Payments.GetPayments($"SELECT *,(substr(BillID,1,5) || ' - ' || DDOCode || ' (' || count(DDOCode) || ')')FreeCell1, ('₹ ' ||  sum(NetAmount))FreeCell2 from Payments where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year} GROUP by DDOCode ORDER by Sortable_Date DESC");
            DetailedList.ItemsSource = MyListData;
            if (MyListData.Any())
            {
                try
                {
                    Refresh_Line.Text = $"Data Refresh {MyListData.ElementAt(0).DataRefreshDate} at {MyListData.ElementAt(0).DataRefreshTime}";
                    var BottomData = database_Payments.GetPayments($"SELECT sum(NetAmount)FreeCell3,count(DDOCode)FreeCell4 from Payments where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year}");
                    Total_Line.Text = $"Total Amount : ₹ {BottomData.ElementAt(0).FreeCell3} For {BottomData.ElementAt(0).FreeCell4} Bills";
                }
                catch (Exception ey)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Exception", ey.Message, "OK");
                    });
                }
            }
        }
        private async void Picker_fYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Picker_fYear.SelectedIndex != -1)
            {
                PageFinYear = int.Parse(Picker_fYear.Items[Picker_fYear.SelectedIndex]);
                PageAcountNumber = App.AccountNumber;
                await ServiceGetRequest();
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
                await ServiceGetRequest();
            }
            else
            {
                Picker_Account.SelectedItem = App.AccountNumber;
                return;
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
        private async Task<string> ServiceGetRequest()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                activity1.IsVisible = true;
                HttpResponseMessage responce;
                try
                {
                    var client = new HttpClient();
                    responce = await client.GetAsync($"https://himkosh.nic.in/ehpoltis/wcfServices/OtherPayment.svc/GetPayeeOtherPayments?payeeCode=IP99-99999&paymentMonth=&paymentYear={PageFinYear}&bankAccNo={PageAcountNumber}");
                    var OtherPaymnetJson = await responce.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(OtherPaymnetJson))
                    {
                        JObject parsed = JObject.Parse(OtherPaymnetJson);
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
                                    Picker_Account.SelectedItem = App.AccountNumber;
                                    Picker_fYear.SelectedItem = App.Fin_Year;
                                    return null;
                                }
                                else
                                {
                                    App.AccountNumber = PageAcountNumber;
                                    App.Fin_Year = PageFinYear;
                                    database_Accounts.Custom($"update Accounts set Fin_Year = CASE WHEN AccountNumber = '{App.AccountNumber}' THEN {App.Fin_Year} else Fin_Year END, SelectedAccount = CASE WHEN AccountNumber = '{App.AccountNumber}'  THEN 1 ELSE 0 END");
                                }
                            }
                            if (pair.Key == "otherPayments")
                            {
                                database_Payments.Custom($"delete from payments where AccountNumber = {App.AccountNumber} and Fin_Year = {App.Fin_Year}");
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
                                    DateTime AsofDate = DateTime.ParseExact(item.PaidOn, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    item.Payed_Month = AsofDate.ToString("MMMM, yyyy");
                                    item.Sortable_Date = AsofDate.ToString("yyyy-MM-dd");
                                    item.PayeeCode = node["PayeeCode"].ToString();
                                    item.Remarks = node["Remarks"].ToString();
                                    item.Treasury = node["Treasury"].ToString();
                                    item.AccountNumber = App.AccountNumber;
                                    item.Fin_Year = App.Fin_Year;
                                    database_Payments.AddPayments(item);
                                }
                            }
                        }
                        App.Accounts_List = database_Accounts.GetAccounts("select * from accounts");
                        Application.Current.MainPage = new NavigationPage(new TabViewPage("2"));
                    }
                }
                catch (Exception ey)
                {
                    activity1.IsVisible = false;
                    await DisplayAlert("eBhugtan", "Unable to Retrive Data from Server \n Please Try again", "OK");
                }
            }
            else
            {
                activity1.IsVisible = false;
                await DisplayAlert("eBhugtan", "It seems that you are offline. Please check your internet connection", "OK");
                var MyListData = database_Payments.GetPayments($"SELECT *,(substr(BillID,1,5) || ' - ' || DDOCode || ' (' || count(DDOCode) || ')')FreeCell1, ('₹ ' ||  sum(NetAmount))FreeCell2 from Payments where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year} GROUP by DDOCode ORDER by Sortable_Date DESC");
                DetailedList.ItemsSource = MyListData;
                if (MyListData.Any())
                {
                    try
                    {
                        Refresh_Line.Text = $"Data Refresh {MyListData.ElementAt(0).DataRefreshDate} at {MyListData.ElementAt(0).DataRefreshTime}";
                        var BottomData = database_Payments.GetPayments($"SELECT sum(NetAmount)FreeCell3,count(DDOCode)FreeCell4 from Payments where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year}");
                        Total_Line.Text = $"Total Amount : ₹ {BottomData.ElementAt(0).FreeCell3} For {BottomData.ElementAt(0).FreeCell4} Bills";
                    }
                    catch (Exception ey)
                    {
                        await DisplayAlert("Exception", ey.Message, "OK");
                    }
                }
            }
            activity1.IsVisible = false;
            return "ok";
        }
    }
}
