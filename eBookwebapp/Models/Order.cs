using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eBookwebapp.Models
{
    public class Order
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }

        [Required]
            public string CustomerID { get; set; }
        [Required]
            public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }

        [ForeignKey("CustomerID")]
        public Customer Customer { get; set; }

    }
}
