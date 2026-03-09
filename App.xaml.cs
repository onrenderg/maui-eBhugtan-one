using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace eBhugtan
{
    public partial class App : Application
    {
        Database_Accounts database_Accounts;
        public static string Sync;
        public static string AccountNumber;
        public static int Fin_Year;
        public static string SelectedAccount;
        public static System.Collections.IList YearsList;
        public static IEnumerable<Accounts> Accounts_List;
        public static string Dbname = "eBhugtan.db";
        public App()
        {
            InitializeComponent();
            database_Accounts = new Database_Accounts();
            YearsList = new List<string>();
            var year = DateTime.Now.AddDays(-84).Year;
            for (int i = year; i >= 2016; i--)
            {
                YearsList.Add(i.ToString());
            }
            Accounts_List = database_Accounts.GetAccounts("select * from Accounts");
            if (Accounts_List.Any())
            {
                MainPage = new NavigationPage(new TabViewPage(null));
            }else
            {
                MainPage = new NavigationPage(new LoginPage());
            }
        }
        protected override void OnStart()
        {
            // Handle when your app starts
        }
        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }
        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
