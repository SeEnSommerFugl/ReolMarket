namespace ReolMarket.MVVM.Model
{
    public class Sale
    {
        public Guid SaleID { get; set; } = Guid.NewGuid();
        public DateTime SaleDate { get; set; }
        public Guid ShoppingCartID { get; set; }
        public Guid PaymentID { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
