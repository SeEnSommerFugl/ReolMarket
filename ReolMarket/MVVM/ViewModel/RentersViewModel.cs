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
    private string? _searchText;
    private bool _onlyFree;
    private BoothStatus? _statusFilter;
    private SearchModeItem? _selectedSearchMode;

    // ICollectionView for filtering without data duplication
    private readonly ICollectionView _boothsView;
    private readonly ICollectionView _customersView;

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
    public ICommand SaveCustomerCommand { get; }

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

    }

    private void RequestRefresh()
    {
        _refreshDebounce.Stop();
        _refreshDebounce.Start();
    }

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
}