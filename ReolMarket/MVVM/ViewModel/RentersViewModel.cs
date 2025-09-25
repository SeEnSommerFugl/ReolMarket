using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.Data.Repository;
using ReolMarket.MVVM.Model;
using ReolMarket.MVVM.Model.HelperModels;
using ReolMarket.MVVM.View;

namespace ReolMarket.MVVM.ViewModel;

/// <summary>
/// Clean, synchronous ViewModel that uses repository collections as single source of truth.
/// </summary>
public class RentersViewModel : BaseViewModel
{
    private readonly IBaseRepository<Booth, Guid> _boothRepo;
    private readonly IBaseRepository<Customer, Guid> _customerRepo;

    private readonly DispatcherTimer _refreshDebounce = new() { Interval = TimeSpan.FromMilliseconds(200) };


    // Backing fields
    //private Booth? _selectedBooth;
    private string? _searchText;
    private bool _onlyFree;
    private BoothStatus? _statusFilter;
    private SearchModeItem? _selectedSearchMode;

    // ICollectionView for filtering without data duplication
    private readonly ICollectionView _boothsView;
    private readonly ICollectionView _customersView;

    // ✅ Micro-cache to speed up name lookups / linking (rebuilt on refresh)
    //private Dictionary<Guid, Customer>? _customerById;

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
    public ICollectionView CustomersView => _customersView;

    public List<SearchModeItem> SearchModes { get; set; }

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

    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
            {
                _boothsView.Refresh();
                //RefreshCommands();
            }
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
                RequestRefresh(); // ✅ Instant feedback
        }
    }

    public SearchModeItem? SelectedSearchMode
    {
        get => _selectedSearchMode;
        set
        {
            if (SetProperty(ref _selectedSearchMode, value))
                _boothsView?.Refresh();

        }
    }

    // Commands
    public ICommand NavigateAdminPopUpCommand { get; }
    //public ICommand RefreshCommand { get; }
    //public ICommand AddBoothCommand { get; }
    //public ICommand EditBoothCommand { get; }
    //public ICommand DeleteBoothCommand { get; }
    //public ICommand ClearFiltersCommand { get; }
    public ICommand SaveCustomerCommand { get; }
    //public ICommand DeleteCustomerCommand { get; }

    /// <summary>
    /// Creates a new instance and initializes the view model.
    /// </summary>
    public RentersViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Customer, Guid> customerRepo)
    {
        Title = "Booths";
        // ⚠️ Consider DI: inject repositories instead of `new` for testability.
        _boothRepo = boothRepo;
        _customerRepo = customerRepo;
        InitializeSearchTypes();
        SelectedSearchMode = SearchModes.First(); // Default to "Alle"

        // ✅ Init Booths view (sorted by booth number)
        _boothsView = CollectionViewSource.GetDefaultView(Booths);
        _boothsView.Filter = FilterBooth;
        _boothsView.SortDescriptions.Add(
            new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));

        // ✅ Init Customers view (unique customers) and search on customers
        _customersView = CollectionViewSource.GetDefaultView(Customers);
        _customersView.Filter = FilterCustomer;
        _customersView.SortDescriptions.Add(
            new SortDescription(nameof(Customer.CustomerName), ListSortDirection.Ascending));

        _refreshDebounce.Tick += (_, __) => { _refreshDebounce.Stop(); _customersView.Refresh(); };



        //// ✅ If the underlying collection changes, refresh the view
        //Booths.CollectionChanged += OnBoothsChanged;

        // Initialize commands (synchronous implementations)
        NavigateAdminPopUpCommand = new RelayCommand(_ =>
        {
            Keyboard.ClearFocus();
            var adminVM = new AdminPopUpViewModel(new BoothDbRepository(), new CustomerDbRepository());
            var win = new AdminPopUpView
            {
                DataContext = adminVM,
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
        });
        //RefreshCommand = new RelayCommand(_ => RefreshData(), _ => CanModifyData());
        //AddBoothCommand = new RelayCommand(_ => AddBooth(), _ => CanModifyData());
        //EditBoothCommand = new RelayCommand(_ => EditBooth(), _ => CanEditBooth());
        //DeleteBoothCommand = new RelayCommand(_ => DeleteBooth(), _ => CanDeleteBooth());
        //ClearFiltersCommand = new RelayCommand(_ => ClearFilters(), _ => CanClearFilters());
        //SaveCustomerCommand = new RelayCommand(_ => SaveCustomer(), _ => CanSaveCustomer());
        //DeleteCustomerCommand = new RelayCommand(_ => ClearCustomer(), _ => CanClearCustomer());

        // Initial load
        //RefreshData();
    }

    //private void OnBoothsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    //{
    //    // ✅ Minimal work: just refresh the view so sorting/filtering stays correct
    //    _boothsView.Refresh();
    //}

    /// <summary>
    /// Refreshes data from repositories and ensures customer relationships are set.
    /// Synchronous version (no async/await).
    /// </summary>
    //private void RefreshData()
    //{
    //if (IsBusy) return;             // ✅ Guard to avoid re-entry
    //IsBusy = true;                   // ✅ Manual busy handling since we're sync now
    //try
    //{
    //    using (_boothsView.DeferRefresh())
    //    {
    //        // Preserve current selection
    //        var currentKey = SelectedBooth?.BoothID;

    //        // Load (ideally these update the same ObservableCollection instances)
    //        _customerRepo.GetAll();
    //        _boothRepo.GetAll();

    //        // Rebuild lookups/refs without creating new Booth instances if you can
    //        LinkCustomersToBooths();

    //        // Restore selection if item still exists
    //        if (currentKey != null)
    //            SelectedBooth = Booths.FirstOrDefault(b => b.BoothID == currentKey);
    //    }

    //}
    //finally
    //{
    //    IsBusy = false;
    //    RefreshCommands();           // ✅ Update buttons after busy change
    //}
    //}

    private void RequestRefresh()
    {
        _refreshDebounce.Stop();
        _refreshDebounce.Start();
    }

    //private void CustomerFromSelectedBooth()
    //{
    //    var c = SelectedBooth?.Customer;

    //    CustomerName = c?.CustomerName ?? string.Empty;
    //    CustomerEmail = c?.Email ?? string.Empty;
    //    CustomerPhone = c?.PhoneNumber ?? string.Empty;
    //    CustomerAddress = c?.Address ?? string.Empty;
    //    CustomerPostalCode = c is null ? string.Empty : c.PostalCode.ToString();
    //}

    /// <summary>
    /// Links Customer objects to Booths for easy binding (Booth.Customer.CustomerName).
    /// </summary>
    //private void LinkCustomersToBooths()
    //{
    ////_customerById = Customers.Count > 0
    ////    ? Customers.ToDictionary(c => c.CustomerID)
    ////    : new Dictionary<Guid, Customer>();

    //var byId = Customers.GroupBy(c => c.CustomerID).ToDictionary(g => g.Key, g => g.First()); // guards dupes
    //foreach (var booth in Booths)
    //{
    //    var id = booth.CustomerID;
    //    booth.Customer = (id.HasValue && byId.TryGetValue(id.Value, out var c)) ? c : null;
    //}
    //}

    private void SaveCustomer()
    {
        //if (SelectedBooth is null) return;

        //// If booth has no customer, then create one
        //if (SelectedBooth.Customer is null)
        //{
        //    var newCustomer = new Customer
        //    {
        //        CustomerID = Guid.NewGuid(),
        //        CustomerName = CustomerName ?? "",
        //        Email = CustomerEmail ?? "",
        //        PhoneNumber = CustomerPhone ?? "",
        //        Address = CustomerAddress ?? "",
        //        PostalCode = CustomerPostalCode ?? "",
        //    };

        //    _customerRepo.Add(newCustomer);        // ✅ persist in repo
        //    Customers.Add(newCustomer);            // ✅ add to observable

        //    SelectedBooth.Customer = newCustomer;
        //    SelectedBooth.CustomerID = newCustomer.CustomerID;
        //    SelectedBooth.IsRented = true;
        //    SelectedBooth.Status = BoothStatus.Optaget;
        //}
        //// If booth has customer, update the info
        //else
        //{
        //    var c = SelectedBooth.Customer;
        //    c.CustomerName = CustomerName ?? "";
        //    c.Email = CustomerEmail ?? "";
        //    c.PhoneNumber = CustomerPhone ?? "";
        //    c.Address = CustomerAddress ?? "";
        //    c.PostalCode = CustomerPostalCode ?? "";

        //    _customerRepo.Update(c);
        //}

        //_boothRepo.Update(SelectedBooth);
        //LinkCustomersToBooths();

        //BoothsView.Refresh();
    }


    //if (SelectedBooth is null) return;

    //var id = SelectedBooth.BoothID;

    //SelectedBooth.Customer = null;
    //SelectedBooth.CustomerID = null;
    //SelectedBooth.IsRented = false;
    //SelectedBooth.Status = BoothStatus.Ledig;

    //CustomerName = CustomerEmail = CustomerPhone = CustomerAddress = CustomerPostalCode = string.Empty;

    //_boothRepo.Update(SelectedBooth);
    //LinkCustomersToBooths();
    //BoothsView.Refresh();
    //RefreshCommands();
    //}

    /// <summary>
    /// Filter predicate for the ICollectionView.
    /// </summary>
    private bool FilterBooth(object obj)
    {
        if (obj is not Booth booth)
            return false;

        var customerId = SelectedCustomer?.CustomerID;

        return customerId.HasValue && booth.CustomerID == customerId.Value;
    }

    /// <summary>
    /// Filter predicate for the Customers ICollectionView (unique customers).
    /// Supports searching by customer fields and booth number.
    /// </summary>
    private bool FilterCustomer(object obj)
    {
        if (obj is not Customer customer)
            return false;

        if (string.IsNullOrWhiteSpace(SearchText) || SelectedSearchMode == null)
            return true;

        var s = SearchText.Trim();

        return SelectedSearchMode.SearchMode switch
        {
            SearchMode.All =>
                customer.CustomerName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                customer.PhoneNumber.Contains(s) ||
                customer.Email.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                customer.Address.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                customer.PostalCode.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                Booths.Any(b => b.CustomerID == customer.CustomerID &&
                                b.BoothNumber.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)),

            SearchMode.BoothNumber =>
                Booths.Any(b => b.CustomerID == customer.CustomerID &&
                                b.BoothNumber.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)),

            SearchMode.CustomerName =>
                customer.CustomerName.Contains(s, StringComparison.OrdinalIgnoreCase),

            SearchMode.CustomerPhone =>
                customer.PhoneNumber.Contains(s),

            SearchMode.CustomerEmail =>
                customer.Email.Contains(s, StringComparison.OrdinalIgnoreCase),

            _ => true
        };
    }

    // Initialize the ComboBox items with Danish text
    private void InitializeSearchTypes()
    {
        SearchModes = new List<SearchModeItem>
            {
                new SearchModeItem { DisplayName = "Alle", SearchMode = SearchMode.All },
                new SearchModeItem { DisplayName = "Reol nummer", SearchMode = SearchMode.BoothNumber },
                new SearchModeItem { DisplayName = "Kunde navn", SearchMode = SearchMode.CustomerName },
                new SearchModeItem { DisplayName = "Kunde telefon", SearchMode = SearchMode.CustomerPhone },
                new SearchModeItem { DisplayName = "Kunde email", SearchMode = SearchMode.CustomerEmail }
            };
    }

    /// <summary>
    /// Clears all active filters efficiently.
    /// </summary>
    //private void ClearFilters()
    //{
    //// ✅ Batch refreshes so the view only refreshes once
    //using (_boothsView.DeferRefresh())
    //{
    //    // set backing fields to avoid calling the setters' Refresh() logic
    //    SetProperty(ref _searchText, string.Empty);
    //    SetProperty(ref _onlyFree, false);
    //    SetProperty(ref _statusFilter, null);
    //}
    //}

    /// <summary>
    /// Adds a new booth (synchronous).
    /// </summary>
    //private void AddBooth()
    //{
    //if (IsBusy) return;
    //IsBusy = true;
    //try
    //{
    //    var nextNumber = Booths.Count == 0 ? 1 : Booths.Max(b => b.BoothNumber) + 1;

    //    var newBooth = new Booth
    //    {
    //        BoothID = Guid.NewGuid(),
    //        BoothNumber = nextNumber,
    //        NumberOfShelves = 6,
    //        HasHangerBar = false,
    //        IsRented = false,
    //        Status = BoothStatus.Ledig,
    //        CustomerID = null,
    //        Customer = null
    //    };
    //    using (BoothsView.DeferRefresh())
    //    {
    //        _boothRepo.Add(newBooth);   // ✅ Use sync repo method
    //        SelectedBooth = newBooth;   // ✅ Focus newly created row
    //        BoothsView.Refresh();
    //    }
    //}
    //finally
    //{
    //    IsBusy = false;
    //    RefreshCommands();
    //}
    //}

    /// <summary>
    /// Edits the selected booth (synchronous).
    /// </summary>
    //private void EditBooth()
    //{
    //if (IsBusy || SelectedBooth is null) return;
    //IsBusy = true;
    //try
    //{
    //    // 💡 Example edit: toggle hanger bar (replace with real edit flow / dialog)
    //    SelectedBooth.HasHangerBar = !SelectedBooth.HasHangerBar;
    //    if (SelectedBooth.HasHangerBar == true)
    //    {
    //        SelectedBooth.NumberOfShelves = 3;
    //    }

    //    SelectedBooth.IsRented = !SelectedBooth.IsRented;
    //    if (SelectedBooth.IsRented == false)
    //    {
    //        SelectedBooth.Status = BoothStatus.Ledig;
    //        SelectedBooth.CustomerID = null;
    //        SelectedBooth.Customer = null;
    //    }

    //    _boothRepo.Update(SelectedBooth); // ✅ Sync save
    //                                      // If Booth implements INotifyPropertyChanged, UI updates automatically.
    //}
    //finally
    //{
    //    IsBusy = false;
    //    RefreshCommands();
    //}
    //}

    /// <summary>
    /// Deletes the selected booth (synchronous).
    /// </summary>
    //private void DeleteBooth()
    //{
    //    //if (IsBusy || SelectedBooth is null) return;

    //// ✅ Prevent accidental delete of rented booths (extra guard; CanExecute already checks)
    //if (SelectedBooth.IsRented) return;

    //IsBusy = true;
    //try
    //{
    //    var toDelete = SelectedBooth;
    //    SelectedBooth = null;

    //    _boothRepo.Delete(toDelete.BoothID); // ✅ Sync delete
    //                                         // Items collection is repository-owned; view/listen refresh keeps UI in sync
    //}
    //finally
    //{
    //    IsBusy = false;
    //    RefreshCommands();
    //}
    //}

    #region Command Can Execute Methods

    //private bool CanModifyData() => !IsBusy;

    //private bool CanEditBooth() => !IsBusy && SelectedBooth != null;

    //private bool CanDeleteBooth()
    //    => !IsBusy && SelectedBooth != null && !SelectedBooth.IsRented;
    //private bool CanClearFilters()
    //    => SearchText is not null || OnlyFree is not false || StatusFilter is not null || SelectedSearchMode is not null;
    //private bool CanSaveCustomer()
    //    => !IsBusy && SelectedBooth != null && SelectedBooth.IsRented is false;
    //private bool CanClearCustomer()
    //    => !IsBusy && SelectedBooth?.Customer != null && SelectedBooth.IsRented is true;

    #endregion

    /// <summary>
    /// Updates command states after selection or state changes.
    /// </summary>
    //private void RefreshCommands()
    //{
    //    (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
    //    (AddBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
    //    (EditBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
    //    (DeleteBoothCommand as RelayCommand)?.RaiseCanExecuteChanged();
    //    (SaveCustomerCommand as RelayCommand)?.RaiseCanExecuteChanged();
    //    (DeleteCustomerCommand as RelayCommand)?.RaiseCanExecuteChanged();
    //}

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

