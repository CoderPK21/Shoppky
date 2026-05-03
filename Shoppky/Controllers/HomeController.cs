using Microsoft.AspNetCore.Mvc;
using Shoppky.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Shoppky.Controllers
{
    public class HomeController : Controller
    {
        private const string CartSessionKey = "ShoppingCart";

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Products(string? searchQuery, string? category, decimal? minPrice, decimal? maxPrice, decimal? minRating, int page = 1)
        {
            // Mock data - Replace with database query
            var allProducts = GetMockProducts();

            var filtered = allProducts.AsEnumerable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                filtered = filtered.Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) 
                    || p.Description?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrEmpty(category))
            {
                filtered = filtered.Where(p => p.Category == category);
            }

            if (minPrice.HasValue)
            {
                filtered = filtered.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                filtered = filtered.Where(p => p.Price <= maxPrice.Value);
            }

            if (minRating.HasValue)
            {
                filtered = filtered.Where(p => p.Rating >= minRating.Value);
            }

            var pageSize = 12;
            var totalCount = filtered.Count();
            var products = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new ProductFilterViewModel
            {
                Products = products,
                SearchQuery = searchQuery,
                SelectedCategory = category,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinRating = minRating,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c)
            };

            return View(viewModel);
        }

        public IActionResult ProductDetail(int id)
        {
            var products = GetMockProducts();
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        public IActionResult Cart()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var products = GetMockProducts();
            var product = products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
                return NotFound();

            var cart = GetCart();
            cart.AddItem(product, quantity);
            SaveCart(cart);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = $"{product.Title} added to cart", cartCount = cart.GetTotalItems() });
            }

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveItem(productId);
            SaveCart(cart);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, cartCount = cart.GetTotalItems(), total = cart.GetTotalPrice() });
            }

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public IActionResult UpdateCartQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            SaveCart(cart);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
                return Json(new
                {
                    success = true,
                    cartCount = cart.GetTotalItems(),
                    total = cart.GetTotalPrice(),
                    itemTotal = item?.Product != null ? item.Quantity * item.Product.Price : 0
                });
            }

            return RedirectToAction(nameof(Cart));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private ShoppingCart GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return new ShoppingCart();

            var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new();
            var products = GetMockProducts();

            foreach (var item in cartItems)
            {
                item.Product = products.FirstOrDefault(p => p.Id == item.ProductId);
            }

            return new ShoppingCart { Items = cartItems };
        }

        private void SaveCart(ShoppingCart cart)
        {
            var cartJson = JsonSerializer.Serialize(cart.Items.Select(i => new { i.Id, i.ProductId, i.Quantity, i.AddedAt }));
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        private List<Product> GetMockProducts()
        {
            return new List<Product>
            {
                new() { Id = 1, Asin = "B001", Title = "Premium Wireless Headphones", Brand = "AudioTech", Description = "High-quality wireless headphones with noise cancellation", Price = 79.99m, Category = "Electronics", Rating = 4.5m, ReviewCount = 256, ImageUrl = "https://via.placeholder.com/300x300?text=Headphones", InStock = true },
                new() { Id = 2, Asin = "B002", Title = "USB-C Fast Charger", Brand = "PowerHub", Description = "65W USB-C charger compatible with most devices", Price = 49.99m, Category = "Electronics", Rating = 4.7m, ReviewCount = 512, ImageUrl = "https://via.placeholder.com/300x300?text=Charger", InStock = true },
                new() { Id = 3, Asin = "B003", Title = "Laptop Stand", Brand = "DeskPro", Description = "Adjustable laptop stand for ergonomic setup", Price = 34.99m, Category = "Office", Rating = 4.3m, ReviewCount = 128, ImageUrl = "https://via.placeholder.com/300x300?text=Stand", InStock = true },
                new() { Id = 4, Asin = "B004", Title = "Mechanical Keyboard", Brand = "KeyMaster", Description = "RGB mechanical keyboard with custom switches", Price = 119.99m, Category = "Electronics", Rating = 4.8m, ReviewCount = 384, ImageUrl = "https://via.placeholder.com/300x300?text=Keyboard", InStock = true },
                new() { Id = 5, Asin = "B005", Title = "Portable SSD 1TB", Brand = "StorageMax", Description = "Ultra-fast portable SSD with 1TB capacity", Price = 99.99m, Category = "Electronics", Rating = 4.6m, ReviewCount = 198, ImageUrl = "https://via.placeholder.com/300x300?text=SSD", InStock = true },
                new() { Id = 6, Asin = "B006", Title = "Desk Lamp LED", Brand = "LightFlow", Description = "Adjustable LED desk lamp with USB charging", Price = 29.99m, Category = "Office", Rating = 4.4m, ReviewCount = 87, ImageUrl = "https://via.placeholder.com/300x300?text=Lamp", InStock = true },
                new() { Id = 7, Asin = "B007", Title = "Ergonomic Mouse", Brand = "ComfortClick", Description = "Wireless ergonomic mouse with precision tracking", Price = 39.99m, Category = "Electronics", Rating = 4.2m, ReviewCount = 156, ImageUrl = "https://via.placeholder.com/300x300?text=Mouse", InStock = true },
                new() { Id = 8, Asin = "B008", Title = "4K Webcam", Brand = "FramePerfect", Description = "Professional 4K USB webcam with auto-focus", Price = 89.99m, Category = "Electronics", Rating = 4.5m, ReviewCount = 224, ImageUrl = "https://via.placeholder.com/300x300?text=Webcam", InStock = true },
            };
        }
    }
}
