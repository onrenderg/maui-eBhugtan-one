using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Maui.Networking;

namespace eBhugtan
{
    /// <summary>
    /// Central HTTP service layer for eBhugtan.
    /// All pages call FetchPaymentsAsync and react to the returned status code.
    ///
    /// Status codes:
    ///   200 – data fetched and saved successfully
    ///   204 – no internet or API returned no records
    ///   300 – API returned a non-"Success" message (bad account / invalid params)
    ///   500 – network / parse exception
    /// </summary>
    internal class HitServices
    {
        private const string BaseUrl =
            "https://himkosh.nic.in/ehpoltis/wcfServices/OtherPayment.svc/GetPayeeOtherPayments";

        /// <summary>
        /// Fetches payments from the Himkosh API and stores them in the local DB.
        /// </summary>
        /// <param name="accountNumber">Bank account number to query.</param>
        /// <param name="finYear">Financial year (e.g. 2024).</param>
        /// <param name="db_Accounts">Accounts database instance.</param>
        /// <param name="db_Payments">Payments database instance.</param>
        /// <param name="clearExisting">If true, delete all existing payment rows before inserting new ones.</param>
        /// <returns>HTTP-style status code (200 / 204 / 300 / 500).</returns>
        public async Task<int> FetchPaymentsAsync(
            string accountNumber,
            int finYear,
            Database_Accounts db_Accounts,
            Database_Payments db_Payments,
            bool clearExisting = false)
        {
            // ── Connectivity check ──────────────────────────────────────────
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return 204;

            try
            {
                using var client = new HttpClient();
                var url = $"{BaseUrl}?payeeCode=IP99-99999&paymentMonth=&paymentYear={finYear}&bankAccNo={accountNumber}";
                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(json))
                    return 204;

                var parsed = JObject.Parse(json);

                // ── "message" node check ────────────────────────────────────
                var msgNode = parsed["message"]?["message"]?.ToString();
                if (msgNode != "Success")
                    return 300;

                // ── "otherPayments" node ────────────────────────────────────
                var nodes = parsed["otherPayments"];
                if (nodes == null)
                    return 204;

                if (clearExisting)
                    db_Payments.DeletePayments(accountNumber, finYear);

                var now = DateTime.Now;
                foreach (var node in nodes)
                {
                    var paidOn = node["PaidOn"]?.ToString();
                    DateTime asofDate = DateTime.ParseExact(
                        paidOn, "dd-MM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture);

                    var item = new Payments
                    {
                        BillID          = node["BillID"]?.ToString(),
                        BillType        = node["BillType"]?.ToString(),
                        DDOCode         = node["DDOCode"]?.ToString(),
                        GrossAmount     = node["GrossAmount"]?.ToString(),
                        NetAmount       = node["NetAmount"]?.ToString(),
                        PaidOn          = paidOn,
                        Payed_Month     = asofDate.ToString("MMMM, yyyy"),
                        Sortable_Date   = asofDate.ToString("yyyy-MM-dd"),
                        PayeeCode       = node["PayeeCode"]?.ToString(),
                        Remarks         = node["Remarks"]?.ToString(),
                        Treasury        = node["Treasury"]?.ToString(),
                        AccountNumber   = accountNumber,
                        Fin_Year        = finYear,
                        DataRefreshDate = now.ToString("dd-MM-yyyy"),
                        DataRefreshTime = now.ToString("HH:mm")
                    };
                    db_Payments.AddPayments(item);
                }

                return 200;
            }
            catch
            {
                return 500;
            }
        }
    }
}
