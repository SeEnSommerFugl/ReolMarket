namespace ReolMarket.MVVM.Model
{
    internal class Item
    {
        public Guid ItemID { get; set; } = Guid.NewGuid();
        public required string ItemName { get; set; }
        public decimal ItemPrice { get; set; }
        public Guid BoothID { get; set; } // Foreign key to BoothID, not nullable. Items must belong to a Booth
        public Booth Booth { get; set; }
    }
}
