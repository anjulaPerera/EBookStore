namespace eBookwebapp.Models
{
    public class OrderItemViewModel
    {
        public int BookID { get; set; }
        public string BookTitle { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
