using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eBookwebapp.Data;
using eBookwebapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;

namespace eBookwebapp.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(ApplicationDbContext context , UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reviews.Include(r => r.Book).Include(r => r.Customer).Include(r => r.Order);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.Customer)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(m => m.ReviewID == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Reviews/Create
        public IActionResult Create()
        {
            ViewData["BookID"] = new SelectList(_context.Book, "BookID", "Author");
            ViewData["CustomerID"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderID", "CustomerID");
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReviewID,BookID,CustomerID,OrderID,ReviewText,Rating,ReviewDate")] Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookID"] = new SelectList(_context.Book, "BookID", "Author", review.BookID);
            ViewData["CustomerID"] = new SelectList(_context.Users, "Id", "Id", review.CustomerID);
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderID", "CustomerID", review.OrderID);
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            ViewData["BookID"] = new SelectList(_context.Book, "BookID", "Author", review.BookID);
            ViewData["CustomerID"] = new SelectList(_context.Users, "Id", "Id", review.CustomerID);
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderID", "CustomerID", review.OrderID);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewID,BookID,CustomerID,OrderID,ReviewText,Rating,ReviewDate")] Review review)
        {
            if (id != review.ReviewID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewID))
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
            ViewData["BookID"] = new SelectList(_context.Book, "BookID", "Author", review.BookID);
            ViewData["CustomerID"] = new SelectList(_context.Users, "Id", "Id", review.CustomerID);
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderID", "CustomerID", review.OrderID);
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.Customer)
                .Include(r => r.Order)
                .FirstOrDefaultAsync(m => m.ReviewID == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewID == id);
        }


            public async Task<IActionResult> Review(int bookId, int orderId)
            {
                var userId = _userManager.GetUserId(User);
                var order = await _context.Order
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                    .FirstOrDefaultAsync(o => o.OrderID == orderId && o.CustomerID == userId);

                if (order == null)
                {
                return NotFound();
                }

                var book = order.OrderItems.FirstOrDefault(oi => oi.BookID == bookId)?.Book;
                if (book == null)
                {
                return NotFound();
                }

                var model = new ReviewViewModel
                {
                    BookID = bookId,
                    OrderID = orderId,
                    BookTitle = book.Title,
                    CustomerID = userId
                };

                return View(model);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> SubmitReview(ReviewViewModel model)
            {
                if (ModelState.IsValid)
                {
                    var userId = _userManager.GetUserId(User);

                    var review = new Review
                    {
                        BookID = model.BookID,
                        OrderID = model.OrderID,
                        CustomerID = userId,
                        ReviewText = model.ReviewText,
                        Rating = model.Rating,
                        ReviewDate = DateTime.Now
                    };

                    _context.Reviews.Add(review);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("OrderResult", "Orders");
                }

                return View("Review", model);
            }
        
    }
}
