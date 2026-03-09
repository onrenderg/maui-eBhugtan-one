  private async Task<string> ServiceGetRequest()
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
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