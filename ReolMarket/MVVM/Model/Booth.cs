namespace ReolMarket.MVVM.Model
{
    internal class Booth
    {
        public Guid BoothID { get; set; } = Guid.NewGuid();
        public int BoothNumber { get; set; }
        public int NumberOfShelves { get; set; }
        public bool HasHangerBar { get; set; }
        public bool IsRented { get; set; } = false;
        public BoothStatus Status { get; set; } = BoothStatus.Ledig;
        public Customer? Renter { get; set; }

    }
}
