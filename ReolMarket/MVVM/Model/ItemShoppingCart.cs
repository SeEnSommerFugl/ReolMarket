namespace ReolMarket.MVVM.Model
{
    class ItemShoppingCart
    {
        public Guid ItemID { get; set; }
        public Guid ShoppingCartID { get; set; }
        public readonly record struct ItemShoppingCartKey(Guid ItemId, Guid CartId);
    }
}
