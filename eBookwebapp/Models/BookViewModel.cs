using Microsoft.Identity.Client;

namespace eBookwebapp.Models
{
    public class BookViewModel
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double AverageRating { get; set; }

        public string ImagePath { get; set; }

        public string Author { get; set; }
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
