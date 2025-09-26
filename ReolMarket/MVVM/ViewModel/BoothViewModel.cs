using ReolMarket.Core;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    public class BoothViewModel : BaseViewModel
    {
        private bool _isSelected;
        public Booth Booth { get; }

        public BoothViewModel(Booth booth)
        {
            Booth = booth;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value)) ;
            }
        }

        public int BoothNumber => Booth.BoothNumber;
        public Guid? CustomerID => Booth.CustomerID;
        public bool IsRented => Booth.IsRented;
        public BoothStatus Status => Booth.Status;
        public DateTime? StartDate => Booth.StartDate;
        public DateTime? EndDate => Booth.EndDate;
    }
}
