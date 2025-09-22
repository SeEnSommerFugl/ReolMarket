namespace ReolMarket.MVVM.Model
{
    public class Customer
    {
        public Guid CustomerID { get; set; } = Guid.NewGuid();
        public required string CustomerName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
        public required string PostalCode { get; set; }

    }
}
