namespace ReolMarket.MVVM.Model
{
    public class Booth
    {
        public Guid BoothID { get; set; } = Guid.NewGuid();
        public int BoothNumber { get; set; }
        public int NumberOfShelves { get; set; }
        public bool HasHangerBar { get; set; }
        public bool IsRented { get; set; } = false;
        public BoothStatus Status { get; set; } = BoothStatus.Ledig;
        public Guid? CustomerID { get; set; } // Foreign Key to Customer, can be unassigned(null)
        public Customer? Customer { get; set; }

    }
}
