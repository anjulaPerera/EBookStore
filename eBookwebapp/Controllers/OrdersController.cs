using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eBookwebapp.Data;
using eBookwebapp.Models;
using eBookwebapp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace eBookwebapp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailService _emailService;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Order.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderID,CustomerID,OrderDate,TotalAmount")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,CustomerID,OrderDate,TotalAmount")] Order order)
        {
            if (id != order.OrderID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }



        [Authorize]
        public IActionResult AddToCart(int bookId, int quantity)
        {
            var book = _context.Book.Find(bookId);
            if (book == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(c => c.BookID == bookId);
            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    BookID = book.BookID,
                    Title = book.Title,
                    Author = book.Author,
                    Price = book.Price,
                    Quantity = quantity
                });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["StatusMessage"] = book.Title + "added to cart";
            return RedirectToAction("Index", "Home");
        }


        [Authorize]
        public IActionResult RemoveFromCart(int bookId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.BookID == bookId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToAction("Cart");
        }

        [Authorize]
        public IActionResult Checkout()
        {
            return View(new CheckoutViewModel());
        }


        [Authorize]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {

            if (ModelState.IsValid)
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
                var totalAmount = cart.Sum(item => item.Price * item.Quantity);

                // Save the order details in TempData for later use in payment
                var Order = new Order
                {
                    CustomerID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    OrderItems = cart.Select(item => new OrderItem
                    {
                        BookID = item.BookID,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    }).ToList()
                };

                HttpContext.Session.SetObjectAsJson("Order", Order);

                return RedirectToAction("Payment");
            }

            return View("Checkout", model);
        }


        [Authorize]
        public IActionResult Payment()
        {
            return View();
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(string cardHolderName, string cardNumber, string expiryDate, string cvv, bool isSuccessful)
        {
            var order = HttpContext.Session.GetObjectFromJson<Order>("Order");

            if (isSuccessful)
            {
                // Save order and payment details to database
                _context.Order.Add(order);
                _context.SaveChanges();

                var paymentDetails = new PaymentDetails
                {
                    OrderID = order.OrderID,
                    CardHolderName = cardHolderName,
                    CardNumberLast4 = cardNumber.Substring(cardNumber.Length - 4),
                    PaymentDate = DateTime.Now,
                    Amount = order.TotalAmount,
                    IsSuccessful = true
                };

                _context.PaymentDetails.Add(paymentDetails);

                foreach (var item in order.OrderItems)
                {
                    var book = _context.Book.FirstOrDefault(b => b.BookID == item.BookID);
                    if (book != null)
                    {
                        book.StockQuantity -= item.Quantity;
                    }
                }

                _context.SaveChanges();
                HttpContext.Session.Remove("Cart");
                TempData["SuccessMessage"] = "Order placed successfully!";
                var user = await _userManager.FindByIdAsync(order.CustomerID.ToString());
                if (user != null)
                {
                    var subject = "Order Confirmation";
                    var body = $"<h1>Thank you for your order!</h1>\r\n<p>Your order #{{order.OrderID}} has been placed successfully.</p>\r\n<ul>\r\n@foreach (var item in order.OrderItems)\r\n{{\r\n    <li>@item.Book.Title - <a href=\"@Url.Action(\"Review\", \"Books\", new {{ bookId = item.BookID, orderId = order.OrderID }})\">Review this book</a></li>\r\n}}\r\n</ul>";
                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
            }
            else
            {
                TempData["SuccessMessage"] = "Error With Payment. Please Try again";
            }

            return RedirectToAction("OrderResult");
        }

        public IActionResult OrderResult()
        {
            return View();
        }

        [Authorize]
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
            
        }


        public async Task<IActionResult> CustomerOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Order
                .Where(o => o.CustomerID == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .Select(o => new OrderViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        BookID = oi.BookID,
                        BookTitle = oi.Book.Title,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                }).ToListAsync();

            return View(orders);
        }
    }
}
