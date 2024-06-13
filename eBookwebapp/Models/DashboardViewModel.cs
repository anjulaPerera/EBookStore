namespace eBookwebapp.Models
{
    public class DashboardViewModel
    {

        public List<IncomeReport> IncomeReport { get; set; }
        public List<OrderAdminViewModel> NewOrders { get; set; }

        public List<MostSoldBooks> SoldBooks { get; set; }

    }
    public class IncomeReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalIncome { get; set; }
    }

    public class MostSoldBooks
    {
        public string BookTitle { get; set; }
        public int NoSold { get; set; }

    }

}
