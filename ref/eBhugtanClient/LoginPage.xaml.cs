using eBhugtanClient.Database;
using eBhugtanClient.Models;
using eBhugtanClient.Services;

namespace eBhugtanClient;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

	private async void LoginBtn_Clicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(AccountNumberEntry.Text) || YearPicker.SelectedIndex == -1)
		{
			await DisplayAlert("Error", "Please enter account number and select year", "OK");
			return;
		}

		string accountNo = AccountNumberEntry.Text;
		int year = int.Parse(YearPicker.SelectedItem.ToString());

		LoadingOverlay.IsVisible = true;
		LoginBtn.IsEnabled = false;

		try
		{
			var service = new HitServices();
			// Hardcoded payeeCode as per requirement: IP99-99999
			int result = await service.FetchPaymentsAsync("IP99-99999", accountNo, year);

			if (result == 200 || result == 204)
			{
				// Save account to local DB
				var db = new Database_Accounts();
				db.AddAccounts(new Accounts 
				{ 
					AccountNumber = accountNo, 
					Fin_Year = year,
					SelectedAccount = 1 
				});

				App.AccountNumber = accountNo;
				App.Fin_Year = year;

				await Navigation.PushAsync(new TabViewPage());
			}
			else if (result == 300)
			{
				await DisplayAlert("Alert", "Invalid Input or No Data Found", "OK");
			}
			else
			{
				await DisplayAlert("Error", "Server or Network error", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "OK");
		}
		finally
		{
			LoadingOverlay.IsVisible = false;
			LoginBtn.IsEnabled = true;
		}
	}
}
