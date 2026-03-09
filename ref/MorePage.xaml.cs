using eTransfers.Models;

namespace eTransfers
{
    public partial class MorePage : ContentPage
    {
        public MorePage(bool showReset = false)
        {
            InitializeComponent();
            ResetFrame.IsVisible = showReset;
        }

        private async void Deptt_Call(object sender, EventArgs e)
        {
            try
            {
                PhoneDialer.Open("9015222725");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Unable to make call: {ex.Message}", "OK");
            }
        }

        private async void deptt_WebSite(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new Uri("https://genpmis.hp.nic.in"));
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Unable to open website: {ex.Message}", "OK");
            }
        }

        private async void Deptt_email(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new Uri("mailto:hrms-support@nic.in"));
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Unable to open email: {ex.Message}", "OK");
            }
        }

        private async void policytapped(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new Uri("https://mobileappshp.nic.in/assets/pdf/mobile-app-privacy-policy/eTransfers.html")); // Replace with actual URL
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Unable to open privacy policy: {ex.Message}", "OK");
            }
        }

        private void btn_reset_Clicked(object sender, EventArgs e)
        {
            App.Current!.Windows[0].Page = new NavigationPage(new MainPage(preserveSelection: true));
        }

        private void Tab_Home_Tapped(object sender, EventArgs e)
        {
            // Navigate based on whether transfers data exists (same logic as App.xaml.cs)
            TransfersDatabase transfersDatabase = new TransfersDatabase();
            var transfers = transfersDatabase.GetRecords("SELECT * FROM TransfersDetails").ToList();
            if (transfers.Any())
            {
                App.Current!.Windows[0].Page = new NavigationPage(new TransfersPage());
            }
            else
            {
                App.Current!.Windows[0].Page = new NavigationPage(new MainPage());
            }
        }
    }
}
