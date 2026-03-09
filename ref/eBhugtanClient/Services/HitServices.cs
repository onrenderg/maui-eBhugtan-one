using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using eBhugtanClient.Models;
using eBhugtanClient.Database;

namespace eBhugtanClient.Services
{
    public class HitServices
    {
        // Use 10.0.2.2 for Android Emulator to hit localhost, or localhost for Windows
        public static string BaseUrl = Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.Android 
            ? "http://10.0.2.2:5000/" 
            : "http://localhost:5000/";

        public async Task<int> FetchPaymentsAsync(string payeeCode, string bankAccNo, int paymentYear)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    string url = $"{BaseUrl}ehpoltis/wcfServices/OtherPayment.svc/GetPayeeOtherPayments?payeeCode={payeeCode}&bankAccNo={bankAccNo}&paymentYear={paymentYear}";
                    
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(content);

                        string message = json["message"]?["message"]?.ToString();
                        bool status = json["message"]?["status"]?.ToString().ToLower() == "true";

                        if (status && message == "Success")
                        {
                            var paymentsJson = json["otherPayments"]?.ToString();
                            var payments = JsonConvert.DeserializeObject<List<Payments>>(paymentsJson);

                            if (payments != null && payments.Count > 0)
                            {
                                var db = new Database_Payments();
                                // Clean up old data for this account/year before adding new one
                                db.DeletePayments(bankAccNo, paymentYear);
                                
                                foreach (var p in payments)
                                {
                                    p.AccountNumber = bankAccNo;
                                    p.Fin_Year = paymentYear;
                                    db.AddPayments(p);
                                }
                                return 200; // Success
                            }
                            return 204; // No records
                        }
                        else if (message == "No Record Found.")
                        {
                            return 204;
                        }
                        return 300; // Invalid Input or "Unable to get Data"
                    }
                    return 500; // Server error
                }
            }
            catch (Exception)
            {
                return 500; // Network/Timeout
            }
        }
    }
}
