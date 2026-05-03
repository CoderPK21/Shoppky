namespace Shoppky.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new();

        public void AddItem(Product product, int quantity = 1)
        {
            var existingItem = Items.FirstOrDefault(x => x.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                Items.Add(new CartItem { ProductId = product.Id, Product = product, Quantity = quantity });
            }
        }

        public void RemoveItem(int productId)
        {
            Items.RemoveAll(x => x.ProductId == productId);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    RemoveItem(productId);
                else
                    item.Quantity = quantity;
            }
        }

        public decimal GetTotalPrice()
        {
            return Items.Sum(x => (x.Product?.Price ?? 0) * x.Quantity);
        }

        public int GetTotalItems()
        {
            return Items.Sum(x => x.Quantity);
        }
    }
}
