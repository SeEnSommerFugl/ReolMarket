using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    internal class AdminPopUpViewModel : BaseViewModel
    {
        private readonly IBaseRepository<Booth, Guid> _boothRepo;
        private readonly IBaseRepository<Customer, Guid> _customerRepo;

        private readonly ICollectionView _newRenterView;
        private readonly ICollectionView _editRenterView;

        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<Customer> Customers => _customerRepo.Items;
        public ICollectionView NewRenterView => _newRenterView;
        public ICollectionView EditRenterView => _editRenterView;

        public AdminPopUpViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Customer, Guid> customerRepo)
        {
            _boothRepo = boothRepo;
            _customerRepo = customerRepo;

            _newRenterView = CollectionViewSource.GetDefaultView(Booths);
            _newRenterView.Filter = FilterBooth;
            _newRenterView.SortDescriptions.Add
                (new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));

            _editRenterView = CollectionViewSource.GetDefaultView(Booths);
            _editRenterView.Filter = FilterBooth;
            _editRenterView.SortDescriptions.Add
                (new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));
        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    _editRenterView.Refresh();
                    //RefreshCommands();
                }
            }
        }

        private bool _isEditing;
        public bool IsEdditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged();
                }
            }
        }


        private bool FilterBooth(object obj)
        {
            if (obj is not Booth booth)
                return false;

            var customerId = SelectedCustomer?.CustomerID;

            return customerId.HasValue && booth.CustomerID == customerId.Value;
        }



        /// <summary>
        /// The booth currently selected in the UI.
        /// </summary>
        //public Booth? SelectedBooth
        //{
        //    get => _selectedBooth;
        //    set
        //    {
        //        if (SetProperty(ref _selectedBooth, value))
        //        {
        //            //CustomerFromSelectedBooth();
        //            RefreshCommands(); // ✅ Keep buttons in sync

        //        }
        //    }
        //}

    }
}
