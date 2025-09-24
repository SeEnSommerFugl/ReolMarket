namespace ReolMarket.MVVM.Model
{
    public class Payment
    {
        public Guid PaymentID { get; set; } = Guid.NewGuid();
        public string PaymentMethod { get; set; }
    }
}
