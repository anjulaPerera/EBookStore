namespace eBookwebapp.Models
{
    public class OrderAdminViewModel
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }  // Add this property
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
