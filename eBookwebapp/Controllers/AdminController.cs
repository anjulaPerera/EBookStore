using DinkToPdf;
using DinkToPdf.Contracts;
using eBookwebapp.Data;
using eBookwebapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eBookwebapp.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConverter _converter;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IConverter converter )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _converter = converter;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            // Get data for charts
            var incomeReport = await _context.Order
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new IncomeReport
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalIncome = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            // New Orders
            var newOrders = await _context.Order
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new OrderAdminViewModel
                {
                    OrderID = o.OrderID,
                    CustomerName = o.Customer.Name,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            var mostSoldBooks = await _context.OrderItem
                .Include(oi => oi.Book) 
                .GroupBy(oi => oi.Book.Title)
                .Select(g => new MostSoldBooks
                    {
                        BookTitle = g.Key,
                        NoSold = g.Sum(oi => oi.Quantity)
                    })
                .OrderByDescending(mb => mb.NoSold)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                IncomeReport = incomeReport,
                NewOrders = newOrders,
                SoldBooks = mostSoldBooks
            };

            return View(viewModel);
        }


        public IActionResult ManageBooks()
        {

            return RedirectToAction("Index", "Books");

        }

        private async Task<List<string>> GetUserRoles(IdentityUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }


        public async Task<IActionResult> AddAdmin()
        {

            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (IdentityUser user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.Roles = await GetUserRoles(user);
                userRolesViewModel.Add(thisViewModel);
            }


            return View(userRolesViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ManageRoles(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (!result.Succeeded)
            {
                TempData["StatusMessage"] = "Cannot remove user existing roles";
                return RedirectToAction("Index", "Home");
            }

            result = await _userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded)
            {
                TempData["StatusMessage"] = "Cannot add selected role to user";

              
                return RedirectToAction("Index", "Home");
            }

            TempData["StatusMessage"] = "Added New Admin";

            return RedirectToAction("Index", "Home");
        }


        public IActionResult ManageCustomers()
        {

            return RedirectToAction("Index", "Customers");

        }



        public IActionResult ManageOrders()
        {

            return RedirectToAction("Index", "Orders");

        }


        public async Task<IActionResult> GeneratePdf()
        {
            var incomeReport = await _context.Order
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new IncomeReport
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalIncome = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            var orders = await _context.Order
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new OrderAdminViewModel
                {
                    OrderID = o.OrderID,
                    CustomerName = o.Customer.Name,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            var mostSoldBooks = await _context.OrderItem
                .Include(oi => oi.Book)
                .GroupBy(oi => oi.Book.Title)
                .Select(g => new MostSoldBooks
                {
                    BookTitle = g.Key,
                    NoSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(mb => mb.NoSold)
                .ToListAsync();




            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
            },
                Objects = {
                new ObjectSettings() {
                    PagesCount = true,
                    HtmlContent = GenerateHtmlContent(incomeReport, orders, mostSoldBooks),
                    WebSettings = { DefaultEncoding = "utf-8" },
                },
            }
            };

            var file = _converter.Convert(pdf);
            return File(file, "application/pdf", "IncomeReport.pdf");
        }



        private string GenerateHtmlContent(List<IncomeReport> incomeReport, List<OrderAdminViewModel> orders, List<MostSoldBooks> mostSoldBooks)
        {
            var styles = @"
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
        }
        h1, h2 {
            color: #333;
            text-align: center;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        table, th, td {
            border: 1px solid #ddd;
        }
        th, td {
            padding: 8px;
            text-align: left;
        }
        th {
            background-color: #f2f2f2;
            color: #333;
        }
        tr:nth-child(even) {
            background-color: #f9f9f9;
        }
    </style>";

            var html = $@"
    {styles}
    <h1>Income Report</h1>
    <table>
        <thead>
            <tr>
                <th>Month/Year</th>
                <th>Total Income</th>
            </tr>
        </thead>
        <tbody>";

            foreach (var report in incomeReport)
            {
                html += $"<tr><td>{report.Month}/{report.Year}</td><td>{report.TotalIncome.ToString("C")}</td></tr>";
            }

            html += @"
        </tbody>
    </table>
    <h2>New Orders</h2>
    <table>
        <thead>
            <tr>
                <th>Order ID</th>
                <th>Customer</th>
                <th>Order Date</th>
                <th>Total Amount</th>
            </tr>
        </thead>
        <tbody>";

            foreach (var order in orders)
            {
                html += $"<tr><td>{order.OrderID}</td><td>{order.CustomerName}</td><td>{order.OrderDate.ToShortDateString()}</td><td>{order.TotalAmount.ToString("C")}</td></tr>";
            }

            html += @"
        </tbody>
    </table>
    <h2>Most Sold Books</h2>
    <table>
        <thead>
            <tr>
                <th>Book Title</th>
                <th>No Sold</th>
            </tr>
        </thead>
        <tbody>";

            foreach (var book in mostSoldBooks)
            {
                html += $"<tr><td>{book.BookTitle}</td><td>{book.NoSold}</td></tr>";
            }

            html += @"
        </tbody>
    </table>";

            return html;
        }


    }
}
