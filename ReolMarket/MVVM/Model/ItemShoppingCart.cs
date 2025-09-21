namespace ReolMarket.MVVM.Model
{
    public class ItemShoppingCart
    {
        public Guid ItemID { get; set; }
        public Guid ShoppingCartID { get; set; }
        public readonly record struct ItemShoppingCartKey(Guid ItemId, Guid CartId);
    }
}
