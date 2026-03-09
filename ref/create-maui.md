---
description: Create a new .NET MAUI project in the ref folder
---

1. Go to the ref directory
// turbo
2. Create a new MAUI project
```
dotnet new maui -n eBhugtanClient --force
```

# eBhugtan Client Development Log

- [x] Create Mock Flask API (`ref/tstprj/app.py`)
- [x] Initialize .NET MAUI Project (`ref/eBhugtanClient`)
- [x] Configure `App.xaml.cs` (NavigationPage + LoginPage)
- [x] Create Solution File (`eBhugtanClient.sln`)
- [x] Port Core Data Models
- [x] Implement Database Layer (SQLite)
    - [x] Port `ISQLite.cs`.
    - [x] Port `Database_Accounts.cs`.
    - [x] Port `Database_Payments.cs`.
- [x] Implement API Service Layer
    - [x] Port `HitServices.cs`.
    - [x] Configure `BaseUrl` for Mock API.
- [x] Build UI Pages
    - [x] Full `LoginPage` implementation.
    - [x] `TabViewPage` with `MonthWisePage` and `DDOWisePage`.
    - [x] `MorePage` for account management.
- [x] Fix Compilation Errors (Namespace & XAML)

# Implementation Walkthrough

### 1. Navigation and LoginPage Setup
To provide a smooth onboarding experience, I decided to bypass the default MAUI Shell navigation and instead use a `NavigationPage`. This allows us to have a dedicated stack for the login flow.

**Creating the Solution File (Step-by-Step):**
Since .NET 10 uses the new `.slnx` format by default, I initialized the solution with the following command in the project folder:
```powershell
dotnet new sln
```
Then, I linked the project to the new solution file:
```powershell
dotnet sln eBhugtanClient.slnx add eBhugtanClient.csproj
```

**Edits in `App.xaml.cs` (Step-by-Step):**
1.  **Inheritance:** In the `App` class, which inherits from the base `Application` class, I first maintained the default constructor.
2.  **Resources:** The constructor calls `InitializeComponent()` to load the XAML resources defined in `App.xaml`.
3.  **Method Override:** I then overrode the `CreateWindow` method. This is the modern entry point for Windows in MAUI 9.0+.
4.  **Root Definition:** Inside `CreateWindow`, I returned a new `Window` instance.
5.  **Direct Navigation:** Instead of assigning `new AppShell()`, I wrapped our first page in a `NavigationPage`.
6.  **Login Entry:** Finally, I passed a new instance of `LoginPage` as the root of the `NavigationPage`, ensuring it's the very first screen users see.

### 2. Data Models
I ported the primary data structures to define how information is stored and moved:

**Porting `Accounts.cs` (Step-by-Step):**
1.  **Directives:** I added `using SQLite;` at the top to leverage attributes for our database schema.
2.  **Primary Key:** I defined the `IDs` property and decorated it with `[PrimaryKey, AutoIncrement]`. This ensures every account has a unique, self-managed identifier.
3.  **Account Metadata:** Added properties for `AccountNumber` and `Fin_Year` to track which account and which financial period the data belongs to.
4.  **State Management:** Finally, I included a `SelectedAccount` property. This allows us to keep track of which account is currently active in the UI.

**Porting `Payments.cs` (Step-by-Step):**
1.  **Storage Attributes:** I included `using SQLite;` to prepare the class for local caching.
2.  **Detail Mapping:** I defined properties like `BillID`, `BillType`, `DDOCode`, and `Treasury` to capture every detail return by the Himkosh API.
3.  **Amount Formatting:** I added `GrossAmount` and `NetAmount` as strings to match the JSON response format, making binding straightforward.
4.  **Audit Trail:** I included `DataRefreshDate` and `DataRefreshTime` fields to show the user when the information was last synced.
5.  **Flexible Binding:** I preserved the `FreeCell1` to `FreeCell5` properties for dynamic UI label mapping.

### 3. Database Layer Implementation
For persistence, I integrated SQLite using the `sqlite-net-pcl` library. I organized this into a dedicated `Database` folder:

**Implementing `Database_Accounts.cs` (Step-by-Step):**
1.  **Path Resolution:** Inside the constructor, I used `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)` to find a safe spot for the file.
2.  **FileName:** I combined that path with a static `App.Dbname` ("eBhugtanClient.db").
3.  **Connection Setup:** I instantiated a new `SQLiteConnection` using the full path.
4.  **Table Deployment:** I called `CreateTable<Accounts>()`, which creates the table if it doesn't already exist.
5.  **Listing Logic:** The `GetAccounts` method uses `conn.Query<Accounts>` to run raw SQL and return results as a List.
6.  **Insertion Logic:** The `AddAccounts` method uses `conn.Insert` to save new account profiles.

**Implementing `Database_Payments.cs` (Step-by-Step):**
1.  **Initialization:** The constructor mirrors the account setup to establish the connection and create the `Payments` table.
2.  **Selective Deletion:** I implemented a specialized `DeletePayments` method. It takes `accountNumber` and `finYear`. If provided, it runs a specific `DELETE` query with a `WHERE` clause to avoid wiping data from other accounts.
3.  **Data Fetching:** The `GetPayments` method executes a raw SQL query, allowing the UI to filter by specific criteria.

### 4. API Service Layer
I implemented the `HitServices` class to handle remote communication with the mock Flask API:

**Troubleshooting Connectivity (Android Emulator):**
If you see a "Server or Network error" on the Android emulator:
1.  **Cleartext Traffic:** I enabled `android:usesCleartextTraffic="true"` in `Platforms/Android/AndroidManifest.xml` because modern Android blocks non-HTTPS traffic by default.
2.  **Mock API Binding:** I updated `app.py` to use `app.run(host='0.0.0.0')` to ensure it listens for external requests coming from the emulator's virtual network bridge.

**Implementing `HitServices.cs` (Step-by-Step):**
1.  **Environment Detection:** I used `Microsoft.Maui.Devices.DeviceInfo.Platform` to set the `BaseUrl`. It points to `10.0.2.2` for Android Emulators and `localhost` for local Windows testing.
2.  **Network Client:** I created a new `HttpClient` with a 30-second timeout.
3.  **URL Formatting:** I constructed the full GET URL by appending the `payeeCode`, `bankAccNo`, and `paymentYear` as query strings.
4.  **Server Call:** I awaited `client.GetAsync(url)` and verified success with `response.IsSuccessStatusCode`.
5.  **JSON Logic:** I parsed the response into a `JObject` using `Newtonsoft.Json`.
6.  **Validation:** I checked the `message` and `status` fields to confirm the server actually found records.
7.  **Auto-Persistence:** If successful:
    *   I converted the `otherPayments` JSON array into a List of `Payments` objects.
    *   I called `DeletePayments` in the DB to clear old stale data for that account.
    *   I looped through the list and called `AddPayments` to save every record into the local SQLite DB.
8.  **Output:** Returned 200 (Success) or 204 (No Data) to the calling UI page.

### 5. LoginPage & Navigation Logic
I've completed the entry point of the application, ensuring it securely validates and fetches initial data:

**Implementing `LoginPage.xaml.cs` (Step-by-Step):**
1.  **Condition Check:** When "Login" is clicked, I verify that the account number entry is not empty and a year is selected.
2.  **Loading State:** I set `LoadingOverlay.IsVisible = true` to show the spinner and disabled the button to prevent multiple requests.
3.  **Task Execution:** I instantiated `HitServices` and awaited `FetchPaymentsAsync`. I used the mandatory `payeeCode` of `IP99-99999` to satisfy the mock API rules.
4.  **Local Storage:** On a successful API response, I added the account details to the `Accounts` table.
5.  **Global Update:** I updated the static `App.AccountNumber` and `App.Fin_Year` properties.
6.  **Transition:** Finally, I called `Navigation.PushAsync(new TabViewPage())` to enter the main app dashboard.

### 6. Tabbed Interface & Session Persistence
I've implemented the main workspace and ensured the app remembers the user across restarts:

**Implementing `MonthWisePage` (Step-by-Step):**
1.  **Setup:** In the constructor, I linked the `PaymentsListView` to an `ObservableCollection` for automatic UI updates.
2.  **LifeCycle Hook:** I overrode `OnAppearing` to trigger the data load every time the page becomes visible.
3.  **DB Query:** I called our `Database_Payments` with a query filtering by the current global `App.AccountNumber` and `App.Fin_Year`.
4.  **Display:** I cleared the old items and populated the collection with the fresh results from the local database.

**Implementing `App.xaml.cs` Session Logic (Step-by-Step):**
1.  **Background Check:** In the `App` constructor, I query the accounts table immediately.
2.  **Session Restore:** If accounts are found, I assign the first one to the static global fields to restore the session.
3.  **Smart Routing:** Inside `CreateWindow`, I check if any accounts exist. If yes, the app starts at `TabViewPage`. If no, it starts at `LoginPage`.

### 7. Account Management (`MorePage`)
**Implementing `MorePage.xaml.cs` (Step-by-Step):**
1.  **Listing:** On appearing, I fetch every record from the `Accounts` table to show all registered profiles.
2.  **Deletion Logic:** When the "Delete" button is pressed:
    *   I show a confirmation `DisplayAlert`.
    *   If confirmed, I remove the account from the `Accounts` table and wipe its specific data from the `Payments` table.
3.  **State Reset:** If the active account was deleted, I set the global variables to null. If it was the only account, I reset the entire `MainPage` to a new `LoginPage`.
