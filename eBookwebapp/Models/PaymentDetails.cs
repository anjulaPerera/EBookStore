namespace eBookwebapp.Models
{
    public class PaymentDetails
    {
        public int PaymentDetailsID { get; set; }
        public int OrderID { get; set; }
        public string CardHolderName { get; set; }
        public string CardNumberLast4 { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsSuccessful { get; set; }

        public Order Order { get; set; }
    }

}
