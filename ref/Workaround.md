# Workaround – Central Service Layer via `HitServices.cs`

## Problem

Every content page in the project (`LoginPage`, `MonthWisePage`, `DDOWisePage`, `MorePage`) contains its own **copy** of `ServiceGetRequest()` that:

- Creates a raw `HttpClient` inline
- Repeats the exact same URL, JSON-parsing logic and error-handling
- Is tightly coupled to UI controls (`activity1`, `DisplayAlert`, `Picker_*`)

This means any endpoint or JSON-structure change must be updated in **four separate files**.

---

## Goal

Move all HTTP logic into a single class — **`HitServices.cs`** — exactly like the pattern already used in the reference project at  
`ref/eTransfer/Models/HitServices.cs`.

Content pages must **only** call the service method and react to the returned status code.

---

## Reference Pattern (from `ref/eTransfer/Models/HitServices.cs`)

```csharp
// Central class – owns the URL constants, HttpClient, and all parsing
public class HitServices
{
    private const string BaseUrl =
        "https://himkosh.nic.in/ehpoltis/wcfServices/OtherPayment.svc/GetPayeeOtherPayments";

    // Returns an int status code so the page can switch on it
    public async Task<int> FetchPaymentsAsync(
        string accountNumber, int finYear,
        Database_Accounts db_Accounts, Database_Payments db_Payments,
        bool clearExisting = false)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return 204;   // No internet

        try
        {
            using var client = new HttpClient();
            var url = $"{BaseUrl}?payeeCode=IP99-99999&paymentMonth=&paymentYear={finYear}&bankAccNo={accountNumber}";
            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(json))
                return 500;

            var parsed = JObject.Parse(json);

            // --- "message" node ---
            var msgNode = parsed["message"]?["message"]?.ToString();
            if (msgNode != "Success")
                return 300;   // API-level failure; caller shows the message

            // --- "otherPayments" node ---
            var nodes = parsed["otherPayments"];
            if (nodes == null) return 204;

            if (clearExisting)
                db_Payments.DeletePayments();

            var now = DateTime.Now;
            foreach (var node in nodes)
            {
                var item = new Payments
                {
                    BillID          = node["BillID"]?.ToString(),
                    BillType        = node["BillType"]?.ToString(),
                    DDOCode         = node["DDOCode"]?.ToString(),
                    GrossAmount     = node["GrossAmount"]?.ToString(),
                    NetAmount       = node["NetAmount"]?.ToString(),
                    PaidOn          = node["PaidOn"]?.ToString(),
                    PayeeCode       = node["PayeeCode"]?.ToString(),
                    Remarks         = node["Remarks"]?.ToString(),
                    Treasury        = node["Treasury"]?.ToString(),
                    AccountNumber   = accountNumber,
                    Fin_Year        = finYear,
                    DataRefreshDate = now.ToString("dd-MM-yyyy"),
                    DataRefreshTime = now.ToString("HH:mm")
                };
                DateTime d = DateTime.ParseExact(item.PaidOn, "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);
                item.Payed_Month   = d.ToString("MMMM, yyyy");
                item.Sortable_Date = d.ToString("yyyy-MM-dd");
                db_Payments.AddPayments(item);
            }
            return 200;  // Success
        }
        catch
        {
            return 500;  // Network / parse error
        }
    }
}
```

**Status code convention**

| Code | Meaning |
|------|---------|
| `200` | Data fetched and saved successfully |
| `204` | No internet **or** API returned no records |
| `300` | API returned a non-"Success" message |
| `500` | Exception (network timeout, parse error, etc.) |

---

## Step-by-Step Migration

### Step 1 – Implement `HitServices.cs`

Replace the empty stub at `HitServices.cs` with the full class shown in the reference pattern above.  
Keep it in `namespace eBhugtan`.

```csharp
using Microsoft.Maui.Networking;
using Newtonsoft.Json.Linq;

namespace eBhugtan
{
    internal class HitServices
    {
        private const string BaseUrl =
            "https://himkosh.nic.in/ehpoltis/wcfServices/OtherPayment.svc/GetPayeeOtherPayments";

        public async Task<int> FetchPaymentsAsync(
            string accountNumber, int finYear,
            Database_Accounts db_Accounts, Database_Payments db_Payments,
            bool clearExisting = false)
        {
            // … (full body from reference pattern above)
        }
    }
}
```

---

### Step 2 – `LoginPage.xaml.cs`

**Before** (lines 42–120 in current file):
```csharp
// private async Task<string> ServiceGetRequest() { ... new HttpClient() ... }
```

**After** – delete `ServiceGetRequest()` entirely and replace the call site:

```csharp
private async void LoginBtn(object sender, EventArgs e)
{
    if (string.IsNullOrEmpty(AcNo.Text)) { ... return; }
    if (string.IsNullOrEmpty(Username.Text)) { ... return; }

    activity1.IsVisible = true;
    var svc    = new HitServices();
    int finYear = int.Parse(Picker_FinYear.Items[Picker_FinYear.SelectedIndex]);
    int status = await svc.FetchPaymentsAsync(AcNo.Text, finYear,
                     database_Accounts, database_Payments, clearExisting: true);
    activity1.IsVisible = false;

    switch (status)
    {
        case 200:
            // Save account and navigate
            database_Accounts.DeleteAccounts();
            database_Accounts.AddAccounts(new Accounts
            {
                AccountNumber  = AcNo.Text,
                Fin_Year       = finYear,
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
            await DisplayAlert("eBhugtan", "Account not found or invalid credentials.", "OK");
            break;
        default:
            await DisplayAlert("eBhugtan",
                "Unable to retrieve data from server.\nPlease try again.", "OK");
            break;
    }
}
```

Remove `using System.Net.Http;` from the top of the file once `HttpClient` is gone.

---

### Step 3 – `MonthWisePage.xaml.cs`

**Before** (lines 112–206):
```csharp
// private async Task<string> ServiceGetRequest() { ... new HttpClient() ... }
```

**After** – delete `ServiceGetRequest()` and update all three call sites:

```csharp
// Helper extracted for reuse across picker events
private async Task ExecuteServiceRequest()
{
    activity1.IsVisible = true;
    var svc    = new HitServices();
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
            Application.Current.MainPage = new NavigationPage(new TabViewPage("1"));
            break;

        case 204:
            await DisplayAlert("eBhugtan",
                "It seems you are offline. Please check your internet connection.", "OK");
            // Show cached data
            LoadCachedData();
            break;

        case 300:
            await DisplayAlert("eBhugtan", "Server returned an error. Please try again.", "OK");
            Picker_Account.SelectedItem = App.AccountNumber;
            Picker_fYear.SelectedItem   = App.Fin_Year;
            break;

        default:
            await DisplayAlert("eBhugtan",
                "Unable to retrieve data from server.\nPlease try again.", "OK");
            break;
    }
}

// Replace every direct await ServiceGetRequest() with:
//   await ExecuteServiceRequest();

private void LoadCachedData()
{
    var data = database_Payments.GetPayments(
        $"SELECT *,(Payed_Month||' ('|| count(BillID)|| ')')FreeCell1," +
        $"('₹ '|| sum(NetAmount))FreeCell2 from Payments " +
        $"where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year} " +
        $"GROUP by Payed_Month order by Sortable_Date DESC");
    DetailedList.ItemsSource = data;
    if (data.Any())
    {
        Refresh_Line.Text = $"Data Refresh {data.First().DataRefreshDate} at {data.First().DataRefreshTime}";
        var bottom = database_Payments.GetPayments(
            $"SELECT sum(NetAmount)FreeCell3,count(DDOCode)FreeCell4 from Payments " +
            $"where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year}");
        Total_Line.Text = $"Total Amount : ₹ {bottom.First().FreeCell3} For {bottom.First().FreeCell4} Bills";
    }
}
```

---

### Step 4 – `DDOWisePage.xaml.cs`

Same pattern as `MonthWisePage`. Delete `ServiceGetRequest()` (lines 94–188) and create:

```csharp
private async Task ExecuteServiceRequest()
{
    activity1.IsVisible = true;
    var svc    = new HitServices();
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
            await DisplayAlert("eBhugtan", "Server returned an error.", "OK");
            Picker_Account.SelectedItem = App.AccountNumber;
            Picker_fYear.SelectedItem   = App.Fin_Year;
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
        Refresh_Line.Text = $"Data Refresh {data.First().DataRefreshDate} at {data.First().DataRefreshTime}";
        var bottom = database_Payments.GetPayments(
            $"SELECT sum(NetAmount)FreeCell3,count(DDOCode)FreeCell4 from Payments " +
            $"where AccountNumber = '{App.AccountNumber}' and Fin_Year = {App.Fin_Year}");
        Total_Line.Text = $"Total Amount : ₹ {bottom.First().FreeCell3} For {bottom.First().FreeCell4} Bills";
    }
}
```

---

### Step 5 – `MorePage.xaml.cs`

`MorePage` only **validates** the account before adding it — no full data sync. Wrap with a lightweight call:

```csharp
// Delete ServiceGetRequest() (lines 135–190) and replace Submit_Btn's call site:

private async Task AddAccountServiceRequest()
{
    activity1.IsVisible = true;
    var svc     = new HitServices();
    int finYear = int.Parse(Picker_FinYear.Items[Picker_FinYear.SelectedIndex]);
    int status  = await svc.FetchPaymentsAsync(AcNo.Text, finYear,
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
            Application.Current.MainPage = new NavigationPage(new TabViewPage("sync"));
            break;

        case 204:
            await DisplayAlert("eBhugtan",
                "It seems you are offline. Please check your internet connection.", "OK");
            break;

        case 300:
            await DisplayAlert("eBhugtan",
                "No Records Found or invalid credentials. Please check and try again.", "OK");
            break;

        default:
            await DisplayAlert("eBhugtan",
                "Unable to retrieve data from server.\nPlease try again.", "OK");
            break;
    }
}

// In Submit_Btn, change:
//   await ServiceGetRequest();
// To:
//   await AddAccountServiceRequest();
```

---

## Cleanup Checklist

After all pages are migrated:

- [ ] Remove `using System.Net.Http;` from `LoginPage.xaml.cs`
- [ ] Remove `using System.Net.Http;` from `MonthWisePage.xaml.cs`
- [ ] Remove `using System.Net.Http;` from `DDOWisePage.xaml.cs`
- [ ] Remove `using System.Net.Http;` from `MorePage.xaml.cs`
- [ ] Verify `HitServices.cs` has `using System.Net.Http;` at the top
- [ ] Build the project and fix any residual compiler errors
- [ ] Test on a device/emulator – login, account add, year/account picker changes

---

## Architecture After Migration

```
ContentPages (LoginPage / MonthWisePage / DDOWisePage / MorePage)
        │
        │  calls  Task<int> FetchPaymentsAsync(...)
        ▼
   HitServices.cs          ← single place for BaseUrl, HttpClient, JSON parser
        │
        │  reads/writes
        ▼
   Database_Payments / Database_Accounts   (SQLite)
```

All HTTP knowledge lives in **one file**. Pages only react to an integer result code.
