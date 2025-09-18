using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.Data.Repository;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// Clean, synchronous ViewModel that uses repository collections as single source of truth.
    /// </summary>
    internal class BoothViewModel : BaseViewModel
    {
        private readonly BoothDbRepository _boothRepo;
        private readonly CustomerDbRepository _customerRepo;

        // Backing fields
        private Booth? _selectedBooth;
        private string? _searchText;
        private bool _onlyFree;
        private BoothStatus? _statusFilter;

        // ICollectionView for filtering without data duplication
        private readonly ICollectionView _boothsView;

        // ✅ Micro-cache to speed up name lookups / linking (rebuilt on refresh)
        private Dictionary<Guid, Customer>? _customerById;

        /// <summary>
        /// Direct access to repository's ObservableCollection - single source of truth.
        /// </summary>
        public ObservableCollection<Booth> Booths => _boothRepo.Items;

        /// <summary>
        /// Direct access to repository's ObservableCollection.
        /// </summary>
        public ObservableCollection<Customer> Customers => _customerRepo.Items;

        /// <summary>
        /// Filtered view of booths for UI binding.
        /// </summary>
        public ICollectionView BoothsView => _boothsView;

        /// <summary>
        /// The booth currently selected in the UI.
        /// </summary>
        public Booth? SelectedBooth
        {
            get => _selectedBooth;
            set
            {
                if (SetProperty(ref _selectedBooth, value))
                    RefreshCommands(); // ✅ Keep buttons in sync
            }
        }

        /// <summary>
        /// Free-text search. Matches the booth number or customer name.
        /// </summary>
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    _boothsView.Refresh(); // ✅ Instant feedback
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
                    _boothsView.Refresh();
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
                    _boothsView.Refresh();
            }
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand AddBoothCommand { get; }
        public ICommand EditBoothCommand { get; }
        public ICommand DeleteBoothCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        /// <summary>
        /// Creates a new instance and initializes the view model.
        /// </summary>
        public BoothViewModel()
        {
            Title = "Booths";

            // ⚠️ Consider DI: inject repositories instead of `new` for testability.
            _boothRepo = new BoothDbRepository();
            _customerRepo = new CustomerDbRepository();

            // ✅ Keep the view in ascending booth number order
            _boothsView = CollectionViewSource.GetDefaultView(Booths);
            _boothsView.Filter = FilterBooth;
            _boothsView.SortDescriptions.Add(
                new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));

            // ✅ If the underlying collection changes, refresh the view
            Booths.CollectionChanged += OnBoothsChanged;

            // Initialize commands (synchronous implementations)
            RefreshCommand = new RelayCommand(_ => RefreshData(), _ => CanModifyData());
            AddBoothCommand = new RelayCommand(_ => AddBooth(), _ => CanModifyData());
            EditBoothCommand = new RelayCommand(_ => EditBooth(), _ => CanEditBooth());
            DeleteBoothCommand = new RelayCommand(_ => DeleteBooth(), _ => CanDeleteBooth());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

            // Initial load
            RefreshData();
        }

        private void OnBoothsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // ✅ Minimal work: just refresh the view so sorting/filtering stays correct
            _boothsView.Refresh();
        }

        /// <summary>
        /// Refreshes data from repositories and ensures customer relationships are set.
        /// Synchronous version (no async/await).
        /// </summary>
        private void RefreshData()
        {
            if (IsBusy) return;             // ✅ Guard to avoid re-entry
            IsBusy = true;                   // ✅ Manual busy handling since we're sync now
            try
            {
                // ✅ Let repositories handle their own loading (sync variants)
                _customerRepo.GetAll();     // ⚠️ Make sure your repo exposes sync API
                _boothRepo.GetAll();

                // ✅ Rebuild lookup + relink references (fast, O(n))
                LinkCustomersToBooths();

                // ✅ Apply current filters/sorts
                _boothsView.Refresh();
            }
            finally
            {
                IsBusy = false;
                RefreshCommands();           // ✅ Update buttons after busy change
            }
        }

        /// <summary>
        /// Links Customer objects to Booths for easy binding (Booth.Customer.CustomerName).
        /// </summary>
        private void LinkCustomersToBooths()
        {
            _customerById = Customers.Count > 0
                ? Customers.ToDictionary(c => c.CustomerID)
                : new Dictionary<Guid, Customer>();

            foreach (var booth in Booths)
            {
                if (booth.CustomerID.HasValue &&
                    _customerById.TryGetValue(booth.CustomerID.Value, out var customer))
                {
                    booth.Customer = customer; // ✅ Strong ref for direct binding
                }
                else
                {
                    booth.Customer = null;
                }
            }
        }

        /// <summary>
        /// Helper method to get customer name for a booth without binding the Customer object.
        /// </summary>
        public string GetCustomerName(Guid? customerID)
        {
            if (!customerID.HasValue || _customerById is null)
                return string.Empty;

            return _customerById.TryGetValue(customerID.Value, out var c)
                ? c.CustomerName ?? string.Empty
                : string.Empty;
        }

        /// <summary>
        /// Filter predicate for the ICollectionView.
        /// </summary>
        private bool FilterBooth(object obj)
        {
            if (obj is not Booth booth)
                return false;

            // ✅ Search by booth number OR customer name
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim();

                // ✅ Avoid culture pitfalls when stringifying numbers
                var matchesBoothNumber = booth.BoothNumber.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    .IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;

                var customerName = booth.Customer?.CustomerName ?? string.Empty;
                var matchesCustomerName = customerName.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;

                if (!matchesBoothNumber && !matchesCustomerName)
                    return false;
            }

            // Status filter
            if (StatusFilter.HasValue && booth.Status != StatusFilter.Value)
                return false;

            // Only show free
            if (OnlyFree && booth.Status != BoothStatus.Ledig)
                return false;

            return true;
        }

        //        BINDINGS(plads) :
        //              - ComboBox.ItemsSource  -> {Binding SearchModes}
        //              - ComboBox.SelectedItem -> {Binding SelectedSearchMode}
        //-TextBox.Text-> { Binding SearchQuery, UpdateSourceTrigger = PropertyChanged}
        //-->

        /// <summary>
        /// Clears all active filters efficiently.
        /// </summary>
        private void ClearFilters()
        {
            // ✅ Batch refreshes so the view only refreshes once
            using (_boothsView.DeferRefresh())
            {
                SearchText = null;   // setters still run, but DeferRefresh prevents multiple refreshes
                OnlyFree = false;
                StatusFilter = null;
            }
        }

        /// <summary>
        /// Adds a new booth (synchronous).
        /// </summary>
        private void AddBooth()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var nextNumber = Booths.Count == 0 ? 1 : Booths.Max(b => b.BoothNumber) + 1;

                var newBooth = new Booth
                {
                    BoothID = Guid.NewGuid(),
                    BoothNumber = nextNumber,
                    NumberOfShelves = 6,
                    HasHangerBar = false,
                    IsRented = false,
                    Status = BoothStatus.Ledig,
                    CustomerID = null,
                    Customer = null
                };

                _boothRepo.Add(newBooth);   // ✅ Use sync repo method
                SelectedBooth = newBooth;   // ✅ Focus newly created row
            }
            finally
            {
                IsBusy = false;
                RefreshCommands();
            }
        }

        /// <summary>
        /// Edits the selected booth (synchronous).
        /// </summary>
        private void EditBooth()
        {
            if (IsBusy || SelectedBooth is null) return;
            IsBusy = true;
            try
            {
                // 💡 Example edit: toggle hanger bar (replace with real edit flow / dialog)
                SelectedBooth.HasHangerBar = !SelectedBooth.HasHangerBar;
                if (SelectedBooth.HasHangerBar == true)
                {
                    SelectedBooth.NumberOfShelves = 3;
                }

                SelectedBooth.IsRented = !SelectedBooth.IsRented;
                if (SelectedBooth.IsRented == false)
                {
                    SelectedBooth.Status = BoothStatus.Ledig;
                    SelectedBooth.CustomerID = null;
                    SelectedBooth.Customer = null;
                }

                _boothRepo.Update(SelectedBooth); // ✅ Sync save
                // If Booth implements INotifyPropertyChanged, UI updates automatically.
            }
            finally
            {
                IsBusy = false;
                RefreshCommands();
            }
        }

        /// <summary>
        /// Deletes the selected booth (synchronous).
        /// </summary>
        private void DeleteBooth()
        {
            if (IsBusy || SelectedBooth is null) return;

            // ✅ Prevent accidental delete of rented booths (extra guard; CanExecute already checks)
            if (SelectedBooth.IsRented) return;

            IsBusy = true;
            try
            {
                var toDelete = SelectedBooth;
                SelectedBooth = null;

                _boothRepo.Delete(toDelete.BoothID); // ✅ Sync delete
                // Items collection is repository-owned; view/listen refresh keeps UI in sync
            }
            finally
            {
                IsBusy = false;
                RefreshCommands();
            }
        }

        #region Command Can Execute Methods

        private bool CanModifyData() => !IsBusy;

        private bool CanEditBooth() => !IsBusy && SelectedBooth != null;

        private bool CanDeleteBooth()
            => !IsBusy && SelectedBooth != null && !SelectedBooth.IsRented;

        #endregion

        /// <summary>
        /// Updates command states after selection or state changes.
        /// </summary>
        private void RefreshCommands()
        {
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (EditBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Override to refresh commands when IsBusy changes.
        /// </summary>
        //protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        //{
        //    base.OnPropertyChanged(propertyName);

        //    // ✅ Central place to react to busy changes
        //    if (propertyName == nameof(IsBusy))
        //    {
        //        RefreshCommands();
        //    }
        //}
    }
}
