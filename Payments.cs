using SQLite;

namespace eBhugtan
{
    public class Payments
    {
        [PrimaryKey, AutoIncrement]
        public int IDs { get; set; }
        public string BillID { get; set; }
        public string BillType { get; set; }
        public string DDOCode { get; set; }
        public string GrossAmount { get; set; }
        public string NetAmount { get; set; }
        public string PaidOn { get; set; }
        public string Payed_Month { get; set; }
        public string Sortable_Date { get; set; }
        public string PayeeCode { get; set; }
        public string Remarks { get; set; }
        public string Treasury { get; set; }
        public string DataRefreshDate { get; set; }
        public string DataRefreshTime { get; set; }
        public string AccountNumber { get; set; }
        public int Fin_Year { get; set; }
        public string FreeCell1 { get; set; }
        public string FreeCell2 { get; set; }
        public string FreeCell3 { get; set; }
        public string FreeCell4 { get; set; }
        public string FreeCell5 { get; set; }
    }
}
