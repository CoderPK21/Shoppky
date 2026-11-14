using Shoppky.Data;
using Shoppky.Models;
using System.Security.Cryptography;
using System.Text;

namespace Shoppky.Data
{
    public static class DbSeeder
    {
        public static void SeedAdminUser(ShoppkyDbContext context)
        {
            // Seed AdminUsers
            if (!context.AdminUsers.Any())
            {
                var defaultAdmin = new AdminUser
                {
                    Username = "admin",
                    Email = "admin@shoppky.com",
                    FullName = "Administrator",
                    PasswordHash = HashPassword("admin123"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.AdminUsers.Add(defaultAdmin);
                context.SaveChanges();
            }

            // Seed Products
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Asin = "B08N5WRWNW",
                        Title = "Apple iPad Air (4th generation)",
                        Brand = "Apple",
                        Description = "Supercharged by the Apple M1 chip. 10.9-inch Liquid Retina display. Support for Apple Pencil and Magic Keyboard.",
                        Price = 49900.00m,
                        Currency = "INR",
                        Url = "https://www.amazon.in/",
                        AffiliateUrl = "https://www.amazon.in/",
                        ImageUrl = "https://via.placeholder.com/300x300?text=iPad+Air",
                        Rating = 4.7m,
                        ReviewCount = 2514,
                        Category = "Electronics",
                        LastSyncedAt = DateTime.UtcNow,
                        InStock = true
                    },
                    new Product
                    {
                        Asin = "B0BSHF7NNT",
                        Title = "Sony WH-CH720 Wireless Headphones",
                        Brand = "Sony",
                        Description = "Wireless headphones with up to 35 hours of battery life. Lightweight design perfect for all-day comfort.",
                        Price = 8150.00m,
                        Currency = "INR",
                        Url = "https://www.amazon.in/",
                        AffiliateUrl = "https://www.amazon.in/",
                        ImageUrl = "https://via.placeholder.com/300x300?text=Sony+Headphones",
                        Rating = 4.5m,
                        ReviewCount = 8926,
                        Category = "Electronics",
                        LastSyncedAt = DateTime.UtcNow,
                        InStock = true
                    },
                    new Product
                    {
                        Asin = "B09NPVVDKB",
                        Title = "Anker PowerCore 26800mAh Portable Charger",
                        Brand = "Anker",
                        Description = "Dual USB ports with PowerIQ technology. Fast charging for multiple devices simultaneously.",
                        Price = 4199.00m,
                        Currency = "INR",
                        Url = "https://www.amazon.in/",
                        AffiliateUrl = "https://www.amazon.in/",
                        ImageUrl = "https://via.placeholder.com/300x300?text=Power+Bank",
                        Rating = 4.6m,
                        ReviewCount = 15234,
                        Category = "Electronics",
                        LastSyncedAt = DateTime.UtcNow,
                        InStock = true
                    },
                    new Product
                    {
                        Asin = "B08BHXQG4B",
                        Title = "Samsung 65-inch QLED Smart TV",
                        Brand = "Samsung",
                        Description = "4K resolution with QLED technology. Smart TV with built-in streaming apps.",
                        Price = 66500.00m,
                        Currency = "INR",
                        Url = "https://www.amazon.in/",
                        AffiliateUrl = "https://www.amazon.in/",
                        ImageUrl = "https://via.placeholder.com/300x300?text=Samsung+TV",
                        Rating = 4.8m,
                        ReviewCount = 5421,
                        Category = "Electronics",
                        LastSyncedAt = DateTime.UtcNow,
                        InStock = true
                    },
                    new Product
                    {
                        Asin = "B0BFVGXR7P",
                        Title = "Instant Pot Duo Plus 9-in-1 Pressure Cooker",
                        Brand = "Instant Pot",
                        Description = "Multi-use pressure cooker and slow cooker. 6-liter capacity for family meals.",
                        Price = 10799.00m,
                        Currency = "INR",
                        Url = "https://www.amazon.in/",
                        AffiliateUrl = "https://www.amazon.in/",
                        ImageUrl = "https://via.placeholder.com/300x300?text=Instant+Pot",
                        Rating = 4.6m,
                        ReviewCount = 32156,
                        Category = "Kitchen",
                        LastSyncedAt = DateTime.UtcNow,
                        InStock = true
                    }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
