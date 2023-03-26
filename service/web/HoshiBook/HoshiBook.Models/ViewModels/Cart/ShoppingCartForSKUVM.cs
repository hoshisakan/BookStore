namespace HoshiBook.Models.ViewModels.Cart
{
    public class ShoppingCartForSKUVM
    {
        public string SKU { get; set; } = default!;
        public ShoppingCart ShoppingCart { get; set; } = default!;
    }
}