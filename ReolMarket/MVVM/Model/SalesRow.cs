namespace ReolMarket.MVVM.Model
{
    public sealed class SalesRow
    {
        public Guid CustomerID { get; init; }
        public string CustomerName { get; init; } = "";
        public Guid BoothID { get; init; }
        public int BoothNumber { get; init; }
        public Guid? ItemID { get; init; }
        public string? ItemName { get; init; }
        public decimal? ItemPrice { get; init; }
        public Guid? ShoppingCartID { get; init; }
        public Guid? SaleID { get; init; }
        public DateTime? SaleDate { get; init; }
        public decimal? SaleTotal { get; init; }
    }
}
