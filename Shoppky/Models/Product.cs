namespace Shoppky.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Asin { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Url { get; set; }
        public string? AffiliateUrl { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
        public bool InStock { get; set; } = true;
    }
}
