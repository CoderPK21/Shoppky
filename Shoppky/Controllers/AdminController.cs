using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoppky.Data;
using Shoppky.Models;
using System.Security.Cryptography;
using System.Text;

namespace Shoppky.Controllers
{
    public class AdminController : Controller
    {
        private readonly ShoppkyDbContext _db;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ShoppkyDbContext db, ILogger<AdminController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Helper: Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"));
        }

        // Helper: Get logged-in admin ID
        private int GetAdminId()
        {
            var adminIdStr = HttpContext.Session.GetString("AdminId");
            return int.TryParse(adminIdStr, out int adminId) ? adminId : 0;
        }

        // Helper: Hash password using SHA256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // ============ LOGIN ============
        [HttpGet]
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
                return RedirectToAction("Dashboard");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
                return View();
            }

            var passwordHash = HashPassword(password);
            var admin = await _db.AdminUsers
                .FirstOrDefaultAsync(a => a.Username == username && a.PasswordHash == passwordHash);

            if (admin == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View();
            }

            if (!admin.IsActive)
            {
                ModelState.AddModelError(string.Empty, "This account is inactive.");
                return View();
            }

            // Set session
            HttpContext.Session.SetString("AdminId", admin.Id.ToString());
            HttpContext.Session.SetString("AdminUsername", admin.Username);
            admin.LastLogin = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // ============ LOGOUT ============
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ============ DASHBOARD ============
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var productCount = await _db.Products.CountAsync();
            var activeProducts = await _db.Products.Where(p => p.InStock).CountAsync();

            ViewBag.TotalProducts = productCount;
            ViewBag.ActiveProducts = activeProducts;

            return View();
        }

        // ============ PRODUCTS - LIST ============
        public async Task<IActionResult> Products(int page = 1, string searchTerm = "")
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            const int pageSize = 10;
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Title.Contains(searchTerm) || p.Asin.Contains(searchTerm));

            var totalCount = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.LastSyncedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View(products);
        }

        // ============ PRODUCTS - ADD ============
        [HttpGet]
        public IActionResult AddProduct()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(product);

            try
            {
                product.LastSyncedAt = DateTime.UtcNow;
                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product '{product.Title}' added successfully!";
                return RedirectToAction("Products");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error adding product");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the product. The ASIN might already exist.");
                return View(product);
            }
        }

        // ============ PRODUCTS - EDIT ============
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (id != product.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(product);

            try
            {
                var existingProduct = await _db.Products.FindAsync(id);
                if (existingProduct == null)
                    return NotFound();

                existingProduct.Title = product.Title;
                existingProduct.Brand = product.Brand;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Category = product.Category;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Rating = product.Rating;
                existingProduct.ReviewCount = product.ReviewCount;
                existingProduct.InStock = product.InStock;
                existingProduct.LastSyncedAt = DateTime.UtcNow;

                _db.Products.Update(existingProduct);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product '{existingProduct.Title}' updated successfully!";
                return RedirectToAction("Products");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating product");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the product.");
                return View(product);
            }
        }

        // ============ PRODUCTS - DELETE ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                    return NotFound();

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product '{product.Title}' deleted successfully!";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["ErrorMessage"] = "An error occurred while deleting the product.";
            }

            return RedirectToAction("Products");
        }

        // ============ ADMIN USERS MANAGEMENT ============
        public async Task<IActionResult> Users()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var users = await _db.AdminUsers.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AdminUser user, string password)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(nameof(password), "Password is required.");
                return View(user);
            }

            if (!ModelState.IsValid)
                return View(user);

            var existingUser = await _db.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == user.Username || u.Email == user.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Username or email already exists.");
                return View(user);
            }

            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.UtcNow;

            _db.AdminUsers.Add(user);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Admin user '{user.Username}' created successfully!";
            return RedirectToAction("Users");
        }
    }
}
