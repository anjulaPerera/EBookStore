﻿namespace eBookwebapp.Models
{
    public class CartItem
    {
            public int BookID { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
       
    }
}
