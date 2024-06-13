using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace eBookwebapp.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookID { get; set; }  // Primary key

        [Required]
        [StringLength(255)]
        public string Title { get; set; } 

        [Required]
        [StringLength(255)]
        public string Author { get; set; } 

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [StringLength(450)]
        public string Description { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [StringLength(255)]
        public string ImagePath { get; set; }

        [AllowNull]
        public virtual ICollection<Review> Reviews { get; set; } // Add this line


        public Book() {

            Reviews = new List<Review>();

        }
    }
}
