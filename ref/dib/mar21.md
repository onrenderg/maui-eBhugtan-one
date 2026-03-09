═══════════════════════════════════════════════════════════════════
  private async Task<string> ServiceGetRequest()
═══════════════════════════════════════════════════════════════════
  ▸ async   → can use await inside (won't freeze the UI thread)
  ▸ Task<string> → returns a string wrapped in a Task (async result)
  ▸ Called from: constructor, Picker_Account changed, Picker_fYear changed
───────────────────────────────────────────────────────────────────

        │
        ▼
if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
│
│  ▸ Connectivity.Current  → singleton provided by MAUI, reads device network state
│  ▸ NetworkAccess.Internet → enum value meaning "full internet reachable"
│  ▸ This check happens BEFORE any HTTP call to avoid wasting time
│
        │                              │
══════ YES ═══════════════     ══════ NO (offline) ═══════════════
        │                              │
        ▼                              ▼
activity1.IsVisible = true;    activity1.IsVisible = false;
│                              │
│  ▸ Shows the spinner/        │  ▸ Hides spinner (nothing loading)
│    loading overlay on        │
│    screen so user knows      ▼
│    a request is in           DisplayAlert("It seems that you
│    progress                   are offline...")
│                              │
│                              │  ▸ Pops up a modal alert.
│                              │    User must tap OK to dismiss.
│                              │
│                              ▼
│                              var MyListData = database_Payments
│                                .GetPayments(
│                                  "SELECT *,
│                                   (substr(BillID,1,5)||' - '
│                                    ||DDOCode||' ('
│                                    ||count(DDOCode)||')')FreeCell1,
│                                   ('₹ '||sum(NetAmount))FreeCell2
│                                   FROM Payments
│                                   WHERE AccountNumber='{App.AccountNumber}'
│                                     AND Fin_Year={App.Fin_Year}
│                                   GROUP BY DDOCode
│                                   ORDER BY Sortable_Date DESC"
│                                )
│                              │
│                              │  ▸ Reads cached payments from local
│                              │    SQLite DB (no internet needed).
│                              │  ▸ FreeCell1 = formatted label like
│                              │    "B0012 - HP123 (3)"
│                              │  ▸ FreeCell2 = "₹ 14400"
│                              │  ▸ GROUP BY DDOCode = one row per DDO
│                              │
│                              ▼
│                              DetailedList.ItemsSource = MyListData;
│                              │
│                              │  ▸ Binds the list of cached rows
│                              │    to the ListView on screen.
│                              │    ListView re-renders with this data.
│                              │
│                              ▼
│                         if (MyListData.Any())
│                              │
│                              │  ▸ .Any() = true if list has ≥1 item
│                              │  ▸ Guard: don't access [0] if empty
│                              │
│                         YES  │   NO → skip inner block
│                              ▼
│                         [inner TRY]
│                         Refresh_Line.Text =
│                           $"Data Refresh
│                             {MyListData.ElementAt(0).DataRefreshDate}
│                             at
│                             {MyListData.ElementAt(0).DataRefreshTime}"
│                              │
│                              │  ▸ ElementAt(0) = first row in list
│                              │  ▸ Shows when data was last fetched
│                              │    e.g. "Data Refresh 04-03-2026 at 14:30"
│                              │
│                              ▼
│                         var BottomData = database_Payments.GetPayments(
│                           "SELECT sum(NetAmount)FreeCell3,
│                                   count(DDOCode)FreeCell4
│                            FROM Payments
│                            WHERE AccountNumber='{App.AccountNumber}'
│                              AND Fin_Year={App.Fin_Year}"
│                         )
│                              │
│                              │  ▸ Separate query for totals only
│                              │  ▸ FreeCell3 = grand total amount
│                              │  ▸ FreeCell4 = total number of bills
│                              │
│                              ▼
│                         Total_Line.Text =
│                           $"Total Amount : ₹ {FreeCell3}
│                             For {FreeCell4} Bills"
│                              │
│                              │  ▸ e.g. "Total Amount : ₹ 48000
│                              │          For 10 Bills"
│                              │
│                         [inner CATCH]
│                         DisplayAlert("Exception", ey.Message, "OK")
│                              │  ▸ If .ElementAt(0) crashes for any
│                              │    reason, show the raw error text
│                              │
│                              └──────────────────────┐
│                                                     │
│                                               (falls through)
│
│══════════════════════════════════════════════════════════════════
│  ONLINE PATH — TRY BLOCK
│══════════════════════════════════════════════════════════════════
        │
        ▼
HttpResponseMessage responce;
│  ▸ Declares variable to hold the HTTP response object.
│    Not assigned yet — just declared here.

        ▼
var client = new HttpClient();
│  ▸ Creates a new HTTP client object.
│    This is the object that actually makes the network call.

        ▼
responce = await client.GetAsync(
  $"https://himkosh.nic.in/.../GetPayeeOtherPayments
    ?payeeCode=IP99-99999
    &paymentMonth=
    &paymentYear={PageFinYear}       ← e.g. 2024
    &bankAccNo={PageAcountNumber}"   ← e.g. "SB-12345"
)
│  ▸ await → pauses this method until response arrives,
│    but does NOT freeze the UI (other code can still run).
│  ▸ .GetAsync() → sends HTTP GET request to that URL.
│  ▸ responce = the full HTTP response (headers + body).

        ▼
var OtherPaymnetJson = await responce.Content.ReadAsStringAsync();
│  ▸ .Content       → the response body object
│  ▸ .ReadAsStringAsync() → reads body bytes as a UTF-8 string
│  ▸ Result is the raw JSON string:
│    '{"message":{"message":"Success","status":"true"},
│      "otherPayments":[{...},{...}]}'

        ▼
if (!string.IsNullOrEmpty(OtherPaymnetJson))
│  ▸ Guard: only parse if the server actually sent something.
│  ▸ Empty body would crash JObject.Parse.
│
        │               │
       YES              NO → do nothing, fall through to end
        │
        ▼
JObject parsed = JObject.Parse(OtherPaymnetJson);
│  ▸ JObject = Newtonsoft.Json type for a JSON object/dictionary
│  ▸ .Parse() → converts raw JSON string into a traversable object
│  ▸ parsed now behaves like:
│      parsed["message"]       → inner JObject
│      parsed["otherPayments"] → JArray (list)

        ▼
foreach (var pair in parsed)
│  ▸ Iterates over top-level key-value pairs in parsed.
│  ▸ pair.Key   = the key name   (string)
│  ▸ pair.Value = the value      (JToken — could be object or array)
│
╔══════════════════════════════════════════════════════════════╗
║  ITERATION 1  →  pair.Key = "message"                       ║
║                  pair.Value = {"message":"...","status":"..."}║
╚══════════════════════════════════════════════════════════════╝
        │
        ▼
if (pair.Key == "message")  →  TRUE
        │
        ▼
var nodesM = pair.Value;
│  ▸ Stores the inner object { "message":"...", "status":"..." }
│  ▸ nodesM is a JToken (acts like a dictionary)

        ▼
string Message = nodesM["message"].ToString();
│  ▸ nodesM["message"] → looks up key "message" inside inner object
│  ▸ .ToString()       → converts JToken → plain C# string
│  ▸ Result: Message = "Success"
│       ──or── Message = "Unable to get Data."

        ▼
if (Message != "Success")
        │                         │
      YES (error)                 NO (success)
        │                         │
        ▼                         ▼
activity1.IsVisible = false   App.AccountNumber = PageAcountNumber;
        │                         │
        │  ▸ Stop spinner          │  ▸ Picker value → global app state
        │                         │
        ▼                         ▼
DisplayAlert(                 App.Fin_Year = PageFinYear;
  "eBhugtan",                 │
  Message,                    │  ▸ Picker year value → global app state
  "OK");                      │
        │                         ▼
        │  ▸ Shows the exact   database_Accounts.Custom(
        │    server error        "UPDATE Accounts
        │    string to user       SET
        │                          Fin_Year =
        ▼                           CASE WHEN AccountNumber =
Picker_Account.SelectedItem           '{App.AccountNumber}'
  = App.AccountNumber;              THEN {App.Fin_Year}
        │                           ELSE Fin_Year END,
        │  ▸ Reset account        SelectedAccount =
        │    picker back to        CASE WHEN AccountNumber =
        │    last valid value        '{App.AccountNumber}'
        │                          THEN 1
        ▼                          ELSE 0 END"
Picker_fYear.SelectedItem     )
  = App.Fin_Year;                   │
        │                         │  ▸ For chosen account:
        │  ▸ Reset year              sets Fin_Year + marks active (1)
        │    picker back to        │  ▸ All other accounts:
        │    last valid value        leaves Fin_Year + marks inactive (0)
        │
        ▼
return null;     ◄── EXITS entire method here
                     loop stops, "otherPayments" block never runs

╔══════════════════════════════════════════════════════════════╗
║  ITERATION 2  →  pair.Key = "otherPayments"                 ║
║  (only reached when message WAS "Success")                   ║
║                  pair.Value = [ {...}, {...}, ... ]           ║
╚══════════════════════════════════════════════════════════════╝
        │
        ▼
if (pair.Key == "otherPayments")  →  TRUE
        │
        ▼
database_Payments.Custom(
  "DELETE FROM payments
   WHERE AccountNumber = {App.AccountNumber}
     AND Fin_Year      = {App.Fin_Year}"
)
│  ▸ Wipes out old payment rows for this account+year
│    before inserting the fresh data from API.
│  ▸ Prevents duplicates.

        ▼
var nodes = pair.Value;
│  ▸ nodes = the JArray (list) of payment dictionaries
│  ▸ Each element looks like:
│      { "BillID":"B001", "NetAmount":"4800",
│        "PaidOn":"15-01-2024", "DDOCode":"HP123", ... }

        ▼
var item = new Payments();
│  ▸ Creates ONE reusable Payments model object.
│  ▸ Properties will be overwritten on each loop iteration.

        ▼
item.DataRefreshDate = DateTime.Now.ToString("dd-MM-yyyy");
item.DataRefreshTime = DateTime.Now.ToString("HH:mm");
│  ▸ Stamps WHEN this data was fetched.
│  ▸ e.g. "05-03-2026" and "11:35"
│  ▸ Same timestamp used for ALL rows in this batch.

        ▼
foreach (var node in nodes)   ← one node = one payment record
│
│  node = { "BillID":"B001", "BillType":"S",
│            "DDOCode":"HP123", "GrossAmount":"5000",
│            "NetAmount":"4800", "PaidOn":"15-01-2024",
│            "PayeeCode":"P99", "Remarks":"Salary",
│            "Treasury":"SML" }
│
        ▼
item.BillID      = node["BillID"].ToString();
│  ▸ node["BillID"] → looks up "BillID" key in this payment dict
│  ▸ .ToString()    → JToken → string
│  ▸ Result: "B001"

item.BillType    = node["BillType"].ToString();      → "S"
item.DDOCode     = node["DDOCode"].ToString();       → "HP123"
item.GrossAmount = node["GrossAmount"].ToString();   → "5000"
item.NetAmount   = node["NetAmount"].ToString();     → "4800"
item.PaidOn      = node["PaidOn"].ToString();        → "15-01-2024"

        ▼
DateTime AsofDate = DateTime.ParseExact(
  item.PaidOn,                   → "15-01-2024"
  "dd-MM-yyyy",                  → format pattern to parse by
  CultureInfo.InvariantCulture   → don't use device locale
);
│  ▸ Converts the string "15-01-2024" into a real DateTime object
│  ▸ AsofDate = {2024, January, 15}

        ▼
item.Payed_Month   = AsofDate.ToString("MMMM, yyyy");
│  ▸ "MMMM" = full month name
│  ▸ Result: "January, 2024"
│  ▸ Used to GROUP payments by month in the ListView.

item.Sortable_Date = AsofDate.ToString("yyyy-MM-dd");
│  ▸ Result: "2024-01-15"
│  ▸ ISO format so ORDER BY on this column sorts correctly.
│    ("15-01-2024" would sort wrong alphabetically.)

item.PayeeCode   = node["PayeeCode"].ToString();     → "P99"
item.Remarks     = node["Remarks"].ToString();       → "Salary"
item.Treasury    = node["Treasury"].ToString();      → "SML"
item.AccountNumber = App.AccountNumber;  → "SB-12345" (global)
item.Fin_Year      = App.Fin_Year;       → 2024       (global)

        ▼
database_Payments.AddPayments(item);
│  ▸ Inserts this fully-populated Payments object as a new row
│    in the local SQLite Payments table.
│  ▸ This repeats for every payment in the array.

        │   (loop repeats for next node)
        │
        ▼  (after all nodes processed)

App.Accounts_List =
  database_Accounts.GetAccounts("select * from accounts");
│  ▸ Refreshes the in-memory accounts list from DB.
│  ▸ Other pages use App.Accounts_List to populate their Pickers.

        ▼
Application.Current.MainPage =
  new NavigationPage(new TabViewPage("2"));
│  ▸ Replaces the entire app's page with a fresh TabViewPage.
│  ▸ "2" = a flag telling TabViewPage which tab to open by default.
│  ▸ This is the navigation after a successful data load.


═══════════════════════════════════════════════════════════════════
  CATCH BLOCK  (if anything inside try threw an exception)
═══════════════════════════════════════════════════════════════════
catch (Exception ey)
│  ▸ ey.Message → the error text (e.g. "No such host is known")
│  ▸ Catches: network timeout, bad JSON, null refs, etc.
│
        ▼
activity1.IsVisible = false;
│  ▸ Stop spinner even on error.

        ▼
DisplayAlert("eBhugtan",
  "Unable to Retrive Data from Server \n Please Try again", "OK");
│  ▸ Generic error message (hides raw exception from user).


═══════════════════════════════════════════════════════════════════
  AFTER if/else (both paths fall here)
═══════════════════════════════════════════════════════════════════
        ▼
activity1.IsVisible = false;
│  ▸ Final safety: ensure spinner is always hidden when done.

        ▼
return "ok";
│  ▸ Returns the string "ok".
│  ▸ Callers do:  await ServiceGetRequest();
│    They NEVER check this return value — it is
│    completely ignored at every call site.
│  ▸ (This is why we changed to int status codes.)
