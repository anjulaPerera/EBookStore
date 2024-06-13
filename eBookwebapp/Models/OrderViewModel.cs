namespace eBookwebapp.Models
{
    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
    }

}
