using eBhugtanClient.Database;
using eBhugtanClient.Models;
using System.Collections.ObjectModel;

namespace eBhugtanClient;

public partial class MonthWisePage : ContentPage
{
    private Database_Payments _dbPayments;
    public ObservableCollection<Payments> PaymentsList { get; set; } = new ObservableCollection<Payments>();

    public MonthWisePage()
    {
        InitializeComponent();
        _dbPayments = new Database_Payments();
        PaymentsListView.ItemsSource = PaymentsList;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private void LoadData()
    {
        AccountLabel.Text = $"A/c: {App.AccountNumber} ({App.Fin_Year})";
        
        var data = _dbPayments.GetPayments($"SELECT * FROM Payments WHERE AccountNumber = '{App.AccountNumber}' AND Fin_Year = {App.Fin_Year}");
        
        PaymentsList.Clear();
        foreach (var p in data)
        {
            PaymentsList.Add(p);
        }
    }
}
