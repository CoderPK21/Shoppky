namespace Shoppky.Models
{
    public class ProductFilterViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public string? SearchQuery { get; set; }
        public string? SelectedCategory { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public IEnumerable<string> Categories { get; set; } = new List<string>();
    }
}
