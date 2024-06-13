using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eBookwebapp.Data;
using eBookwebapp.Models;
using System.Web;
using System.IO;



namespace eBookwebapp.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public JsonResult SearchSuggestions(string query)
        {
            var suggestions = string.IsNullOrEmpty(query) ?
                           new List<object>() :
                           _context.Book
                               .Where(b => b.Title.Contains(query) || b.Author.Contains(query))
                               .Take(5)
                               .Select(b => new { b.BookID, b.Title, b.Author })
                               .ToList<object>();
            return Json(suggestions);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var books = string.IsNullOrEmpty(query) ?
                new List<Book>() :
                _context.Book
                    .Where(b => b.Title.Contains(query) || b.Author.Contains(query))
                    .ToList();
            return View(books);
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {


            return View(await _context.Book.ToListAsync());
        }


        public async Task<IActionResult> AllBooks()
        {
            return View(await _context.Book.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            var book = await _context.Book
         .Include(b => b.Reviews)
         .ThenInclude(r=>r.Customer)
         .FirstOrDefaultAsync(m => m.BookID == id);

            if (id == null)
            {
                return NotFound();
            }

            var model = new BookViewModel
            {
                BookID = book.BookID,
                Title = book.Title,
                Description = book.Description,
                ImagePath = book.ImagePath,
                AverageRating = book.Reviews.Average(r => (double?)r.Rating) ?? 0,
                Reviews = book.Reviews.ToList(),
                StockQuantity = book.StockQuantity,
                Author = book.Author,
                Price = book.Price,
            };
            if (book == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookID,Title,Description,Author,Price,StockQuantity,ImagePath")] Book book)
        {


            if (Request.Form.Files.Count > 0)
            {

                var file = Request.Form.Files[0];

                if (file != null && file.Length > 0)
                {
                    var relativePath = "images/Books/";
                    string webRootPath = _webHostEnvironment.WebRootPath;

                    
                    var fileName = System.IO.Path.GetFileName(file.FileName);


                    using (FileStream stream = new FileStream(Path.Combine(webRootPath, relativePath, fileName), FileMode.Create))
                    {
                        file.CopyTo(stream);
                        TempData["Message"] = "File uploaded successfully.";
                    }


                    // Set the ImagePath property to the relative path
                    book.ImagePath = relativePath + fileName;
                }

            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                string errorMessage = String.Join(", ", errors);
                ViewBag.ErrorMessage = errorMessage; // Pass the error message to the view via ViewBag
                return View(book);
            }


            if (ModelState.IsValid)
            {
               



                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        //  

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookID,Title,Description,Author,Price,StockQuantity,ImagePath")] Book book)
        {

            // Validate that the provided book ID matches the ID of the book being edited
            if (id != book.BookID)
            {
                return NotFound();
            }


            // Check if a file is uploaded with the form data
            if (Request.Form.Files.Count > 0)
            {

                var file = Request.Form.Files[0];

                if (file != null && file.Length > 0)
                {
                    var relativePath = "images/Books/";
                    string webRootPath = _webHostEnvironment.WebRootPath;

                    // Generate a unique filename
                    var fileName = System.IO.Path.GetFileName(file.FileName);


                    using (FileStream stream = new FileStream(Path.Combine(webRootPath, relativePath, fileName), FileMode.Create))
                    {
                        file.CopyTo(stream);
                        TempData["Message"] = "File uploaded successfully.";
                    }


                    // Set the ImagePath property to the relative path
                    book.ImagePath = relativePath + fileName;
                }

            }


            // Validate the model state and update the book details in the database
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookID))
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
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.BookID == id);
        }
    }
}
