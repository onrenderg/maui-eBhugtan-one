using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Microsoft.Maui.Controls;

namespace eBhugtan
{
    public partial class AboutUsPage : ContentPage
    {
        Database_Accounts database_Accounts;
        Database_Payments database_Payments;
        public AboutUsPage()
        {
            InitializeComponent();
            database_Payments = new Database_Payments();
            database_Accounts = new Database_Accounts();
        }
        void LogoutBtn(object sender, System.EventArgs e)
        {
            database_Accounts.DeleteAccounts();
            database_Payments.DeletePayments();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
        void Close_Btn(object s, EventArgs e)
        {
            popupDetails.IsVisible = false;
        }
        void Submit_Btn(object s, EventArgs e)
        {
            //if (!string.IsNullOrEmpty(Picker_Year.SelectedItem.ToString()))
            //{
            //    database_LoginDetails.Custom($"UPDATE LoginDetails SET Fyear = '{Picker_Year.SelectedItem}' WHERE USERNAME NOTNULL");
            //    ////Console.WriteLine(Picker_Year.SelectedItem);
            //    App.fYear = Picker_Year.SelectedItem.ToString();
            //    Application.Current.MainPage = new NavigationPage(new TabViewPage(null));
            //}
            //else
            //{
            //    popupDetails.IsVisible = false;
            //}
        }
        void NIC_Call(object sender, EventArgs e)
        {
            Launcher.TryOpenAsync("tel:01772624045");
        }
        void NIC_Website(object sender, EventArgs e)
        {
            Launcher.TryOpenAsync("https://himachal.nic.in/en-IN/nichp.html");
        }
        void FinYearBtn(object sender, EventArgs e)
        {
            Picker_Year.Focus();
        }
    }
}
