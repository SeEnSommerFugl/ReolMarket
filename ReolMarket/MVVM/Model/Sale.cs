namespace ReolMarket.MVVM.Model
{
    public class Sale
    {
        public Guid SaleID { get; set; } = Guid.NewGuid();
        public Guid ShoppingCartID { get; set; }
        public Guid PaymentID { get; set; }
        public DateTime SaleDate { get; set; }
    }
}
