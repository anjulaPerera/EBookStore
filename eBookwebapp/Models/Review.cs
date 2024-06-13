using Microsoft.AspNetCore.Identity;

namespace eBookwebapp.Models
{
    public class Review
    {
        public int ReviewID { get; set; }
        public int BookID { get; set; }
        public string CustomerID { get; set; }
        public int OrderID { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; } // Rating out of 5
        public DateTime ReviewDate { get; set; }

        public virtual Book Book { get; set; }
        public IdentityUser Customer { get; set; }
        public Order Order { get; set; }
    }

}
