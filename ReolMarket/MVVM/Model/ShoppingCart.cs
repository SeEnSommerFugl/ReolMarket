namespace ReolMarket.MVVM.Model
{
    public class ShoppingCart
    {
        public Guid ShoppingCartId { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
