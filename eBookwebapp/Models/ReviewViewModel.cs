using System.ComponentModel.DataAnnotations;

namespace eBookwebapp.Models
{
    public class ReviewViewModel
    {
        public int BookID { get; set; }
        public int OrderID { get; set; }

        public string CustomerID { get; set; }

        public string BookTitle { get; set; }
        [Required]
        public string ReviewText { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
    }
}
