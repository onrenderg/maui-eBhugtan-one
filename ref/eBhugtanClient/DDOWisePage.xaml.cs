using eBhugtanClient.Database;
using eBhugtanClient.Models;
using System.Collections.ObjectModel;

namespace eBhugtanClient;

public partial class DDOWisePage : ContentPage
{
    private Database_Payments _dbPayments;
    public ObservableCollection<Payments> DDOList { get; set; } = new ObservableCollection<Payments>();

    public DDOWisePage()
    {
        InitializeComponent();
        _dbPayments = new Database_Payments();
        DDOListView.ItemsSource = DDOList;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private void LoadData()
    {
        // For simplicity, showing raw records. In real app, we'd group by DDO
        var data = _dbPayments.GetPayments($"SELECT * FROM Payments WHERE AccountNumber = '{App.AccountNumber}' AND Fin_Year = {App.Fin_Year}");
        
        DDOList.Clear();
        foreach (var p in data)
        {
            DDOList.Add(p);
        }
    }
}
