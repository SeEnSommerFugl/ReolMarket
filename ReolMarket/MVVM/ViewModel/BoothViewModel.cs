using System.Collections.ObjectModel;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.Data.Repository;     // BoothDbRepository
using ReolMarket.MVVM.Model;          // Booth, BoothStatus

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel that lists and manages booths.
    /// Uses in-memory filtering and synchronous repositories.
    /// </summary>
    internal class BoothViewModel : BaseViewModel
    {
        // Repository (sync)
        private readonly BoothDbRepository _boothRepo;
        private readonly CustomerDbRepository _customerRepo;

        // Backing fields
        private Booth? _selectedBooth;
        private string? _searchText;
        private bool _onlyFree;
        private BoothStatus? _statusFilter;

        // Cache of all booths and customers (for client-side filtering)
        private List<Booth> _allBooths = new List<Booth>();
        private List<Customer> _allCustomers = new List<Customer>();

        /// <summary>
        /// Collection bound to the UI. Holds the filtered booths.
        /// </summary>
        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<Customer> Customers => _customerRepo.Items;

        /// <summary>
        /// The booth currently selected in the UI.
        /// </summary>
        public Booth? SelectedBooth
        {
            get => _selectedBooth;
            set
            {
                if (SetProperty(ref _selectedBooth, value))
                {
                    OnPropertyChanged(nameof(CustomerName));
                    RefreshCommands();
                }
            }
        }

        /// <summary>
        /// Free-text search. Matches the booth number as text.
        /// </summary>
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilters();
            }
        }

        /// <summary>
        /// If true, show only booths with status "Ledig".
        /// </summary>
        public bool OnlyFree
        {
            get => _onlyFree;
            set
            {
                if (SetProperty(ref _onlyFree, value))
                    ApplyFilters();
            }
        }

        /// <summary>
        /// Optional filter for a specific booth status.
        /// </summary>
        public BoothStatus? StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (SetProperty(ref _statusFilter, value))
                    ApplyFilters();
            }
        }
        private string? _customerName;
        public string? CustomerName
        {
            get
            {
                if (SelectedBooth != null)
                    return GetCustomerName(SelectedBooth.CustomerID);
                return null;
            }
            set
            {
                if (_customerName != value)
                {
                    _customerName = value;
                    OnPropertyChanged();
                }
            }
        }


        public string? GetCustomerName(Guid? customerId)
        {
            if (customerId is Guid id)
                return Customers.FirstOrDefault(c => c.CustomerID == id)?.CustomerName;

            return null;
        }

        /// <summary>
        /// Command that reloads booths from the repository.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Command that creates a new booth with default values.
        /// </summary>
        public ICommand AddBoothCommand { get; }

        /// <summary>
        /// Command that edits the selected booth.
        /// </summary>
        public ICommand EditBoothCommand { get; }

        /// <summary>
        /// Command that deletes the selected booth.
        /// </summary>
        public ICommand DeleteBoothCommand { get; }

        /// <summary>
        /// Creates a new instance and loads data.
        /// </summary>
        public BoothViewModel()
        {
            Title = "Booths";
            _boothRepo = new BoothDbRepository();
            _customerRepo = new CustomerDbRepository();

            RefreshCommand = new RelayCommand(_ => Load());
            AddBoothCommand = new RelayCommand(_ => AddBooth(), _ => !IsBusy);
            EditBoothCommand = new RelayCommand(_ => EditBooth(), _ => !IsBusy && SelectedBooth != null);
            DeleteBoothCommand = new RelayCommand(_ => DeleteBooth(), _ => !IsBusy && SelectedBooth != null);

            Load();
        }

        /// <summary>
        /// Loads all booths from the repository and applies the current filters.
        /// </summary>
        private void Load()
        {
            RunBusy(() =>
            {
                _allCustomers = _customerRepo.GetAll().ToList();
                var customersById = _allCustomers.ToDictionary(c => c.CustomerID);
                _allBooths = _boothRepo.GetAll().ToList();
                // Attach matching Customer objects so XAML can bind Customer.CustomerName
                foreach (var booth in _allBooths)
                {
                    if (booth.CustomerID.HasValue &&
                        customersById.TryGetValue(booth.CustomerID.Value, out var cust))
                        booth.Customer = cust;
                    else
                        booth.Customer = null;
                }
                ApplyFilters();
            }, "Loading booths…");
        }

        /// <summary>
        /// Applies text, status, and "only free" filters to the cached list.
        /// Updates the UI collection.
        /// </summary>
        private void ApplyFilters()
        {
            var query = _allBooths.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim();
                query = query.Where(b => b.BoothNumber.ToString().Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            if (StatusFilter.HasValue)
                query = query.Where(b => b.Status == StatusFilter.Value);

            if (OnlyFree)
                query = query.Where(b => b.Status == BoothStatus.Ledig);

            var result = query.OrderBy(b => b.BoothNumber).ToArray();

            Booths.Clear();
            foreach (var booth in result)
                Booths.Add(booth);
        }

        /// <summary>
        /// Adds a new booth with simple default values and saves it.
        /// </summary>
        private void AddBooth()
        {
            RunBusy(() =>
            {
                var nextNo = _allBooths.Count == 0 ? 1 : _allBooths.Max(x => x.BoothNumber) + 1;
                var b = new Booth
                {
                    BoothID = Guid.NewGuid(),
                    BoothNumber = nextNo,
                    NumberOfShelves = 6,
                    HasHangerBar = false,
                    IsRented = false,
                    Status = BoothStatus.Ledig,
                    CustomerID = null
                };
                _boothRepo.Add(b);
                Load();
            }, "Adding booth…");
        }

        /// <summary>
        /// Edits the selected booth (simple toggle example) and saves it.
        /// </summary>
        private void EditBooth()
        {
            if (SelectedBooth == null) return;

            RunBusy(() =>
            {
                SelectedBooth.HasHangerBar = !SelectedBooth.HasHangerBar;
                _boothRepo.Update(SelectedBooth);
                Load();
            }, "Saving booth…");
        }

        /// <summary>
        /// Deletes the selected booth and reloads the list.
        /// </summary>
        private void DeleteBooth()
        {
            if (SelectedBooth == null) return;

            RunBusy(() =>
            {
                _boothRepo.Delete(SelectedBooth.BoothID);
                Load();
            }, "Deleting booth…");
        }

        /// <summary>
        /// Updates command states after selection changes.
        /// </summary>
        private void RefreshCommands()
        {
            (AddBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (EditBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
