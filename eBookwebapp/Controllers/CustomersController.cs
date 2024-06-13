using eBookwebapp.Data;
using eBookwebapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eBookwebapp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CustomersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            
            _userManager = userManager;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Customer;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer
                
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }


        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Get the currently logged-in user ID
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Find the existing customer entry for the logged-in user
            var existingCustomer = await _context.Customer
                                                 .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Initialize a new customer model if none exists
            var customer = existingCustomer ?? new Customer { UserId = user.Id };

            // Pass UserID and any other data needed to the view
            ViewData["UserID"] = customer.UserId;

            // Render the form with the customer data
            return View(customer);
        }
        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNew([Bind("Id,Name,Address,PhoneNumber,BirthDay,UserId")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Check if customer already exists (update) or create a new one
                var existingCustomer = await _context.Customer
                                                     .FirstOrDefaultAsync(c => c.UserId == customer.UserId);

                if (existingCustomer == null)
                {
                    _context.Add(customer);
                }
                else
                {
                    // Update the existing customer's information
                    existingCustomer.Name = customer.Name;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.PhoneNumber = customer.PhoneNumber;
                    existingCustomer.BirthDay = customer.BirthDay;
                    _context.Update(existingCustomer);
                }

                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Account Updated Successfully!";
                return RedirectToAction("Index", "Home");
            }

            ViewData["UserID"] = customer.UserId;
            return View("Create", customer);

        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", customer.UserId);
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,PhoneNumber,BirthDay,UserId")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", customer.UserId);
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer
                
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customer.FindAsync(id);
            if (customer != null)
            {
                _context.Customer.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customer.Any(e => e.Id == id);
        }
    }
}
