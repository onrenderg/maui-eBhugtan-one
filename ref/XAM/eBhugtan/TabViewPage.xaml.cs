using System;
using Plugin.Connectivity;
using Microsoft.Maui.Controls;

namespace eBhugtan
{
    public partial class TabViewPage : TabbedPage
    {
        Database_Accounts database_Accounts;
        public TabViewPage(string comesfrom)
        {
            InitializeComponent();
            database_Accounts = new Database_Accounts();
            //Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            Children.Add(new MonthWisePage(comesfrom));
            Children.Add(new DDOWisePage());
            Children.Add(new MorePage());
            try
            {
                CurrentPage = Children[int.Parse(comesfrom)-1];
            }catch
            {

            }
        }
        async void SyncNow(object sender, EventArgs e)
        {
            if (CrossConnectivity.Current.IsConnected == true)
            {
                Application.Current.MainPage = new NavigationPage(new TabViewPage("sync"));
            }
            else
            {
                await DisplayAlert("eBhugtan", "It seems that you are offline. Please check your internet connection", "OK");
            }
        }
    }
}
