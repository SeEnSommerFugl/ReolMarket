using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    public class BoothsViewModel : BaseViewModel
    {
        private readonly IBaseRepository<Booth, Guid> _boothRepo;
        public ObservableCollection<Booth> Booths => _boothRepo.Items;

        private readonly ListCollectionView _rentedSearchView;            // middle grid
        public ICollectionView RentedSearchView => _rentedSearchView;

        private readonly ListCollectionView _boothsForCustomerView;       // bottom-right
        public ICollectionView BoothsForSelectedCustomerView => _boothsForCustomerView;

        private Func<Booth, bool>? _externalFilterFromParent;             // set by parent
        private Guid? _currentCustomerId;

        private Booth? _selectedBooth;
        public Booth? SelectedBooth
        {
            get => _selectedBooth;
            set => SetProperty(ref _selectedBooth, value);
        }

        // optional lookup to quickly link Booth.Customer
        private Dictionary<Guid, Customer> _byId = new();

        public BoothsViewModel(IBaseRepository<Booth, Guid> boothRepo)
        {
            _boothRepo = boothRepo;
            _boothRepo.GetAll();

            _rentedSearchView = new ListCollectionView(Booths);
            _rentedSearchView.Filter = FilterRentedSearch;
            _rentedSearchView.SortDescriptions.Add(new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));

            _boothsForCustomerView = new ListCollectionView(Booths);
            _boothsForCustomerView.Filter = FilterBoothsForCustomer;
            _boothsForCustomerView.SortDescriptions.Add(new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));

            Booths.CollectionChanged += (_, __) => RefreshViews();
        }

        public void ProvideCustomers(IEnumerable<Customer> customers)
        {
            _byId = customers.ToDictionary(c => c.CustomerID);
            foreach (var b in Booths)
                b.Customer = (b.CustomerID.HasValue && _byId.TryGetValue(b.CustomerID.Value, out var c)) ? c : null;
            RefreshViews();
        }

        public void SetExternalFilter(Func<Booth, bool>? filter)
        {
            _externalFilterFromParent = filter;
            // do not refresh here; parent debounces and calls RefreshViews()
        }

        public void SetCurrentCustomer(Customer? c)
        {
            _currentCustomerId = c?.CustomerID;
            _boothsForCustomerView.Refresh();
        }

        public void RefreshViews()
        {
            using (_rentedSearchView.DeferRefresh()) ;
            using (_boothsForCustomerView.DeferRefresh()) ;
        }

        private bool FilterRentedSearch(object o)
        {
            if (o is not Booth b) return false;
            if (!b.IsRented) return false;                    // ← middle grid shows ONLY rented
            return _externalFilterFromParent?.Invoke(b) ?? true;
        }

        private bool FilterBoothsForCustomer(object o)
        {
            if (o is not Booth b) return false;
            if (!_currentCustomerId.HasValue) return false;
            return b.CustomerID.HasValue && b.CustomerID.Value == _currentCustomerId.Value;
        }
    }
}
