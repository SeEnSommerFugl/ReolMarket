using System.Windows.Threading;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;
using ReolMarket.MVVM.Model.HelperModels;

namespace ReolMarket.MVVM.ViewModel
{
    public class RentersViewModel : BaseViewModel
    {
        public BoothsViewModel BoothsVM { get; }
        public CustomersViewModel CustomersVM { get; }

        // Search UI (parent owns this)
        public List<SearchModeItem> SearchModes { get; }
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) RequestRefresh(); }
        }

        private SearchModeItem? _selectedSearchMode;
        public SearchModeItem? SelectedSearchMode
        {
            get => _selectedSearchMode;
            set { if (SetProperty(ref _selectedSearchMode, value)) RequestRefresh(); }
        }

        // Debounce to avoid thrashing filters while typing
        private readonly DispatcherTimer _refreshDebounce = new() { Interval = TimeSpan.FromMilliseconds(200) };

        public RentersViewModel(IBaseRepository<Booth, Guid> boothRepo,
                                IBaseRepository<Customer, Guid> customerRepo)
        {
            BoothsVM = new BoothsViewModel(boothRepo);
            CustomersVM = new CustomersViewModel(customerRepo);

            // keep strong Booth.Customer refs (optional but speeds up filtering on names)
            BoothsVM.ProvideCustomers(CustomersVM.Customers);
            CustomersVM.Customers.CollectionChanged += (_, __) => BoothsVM.ProvideCustomers(CustomersVM.Customers);

            // select booth => show its customer
            BoothsVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BoothsViewModel.SelectedBooth))
                {
                    CustomersVM.SelectedCustomer = BoothsVM.SelectedBooth?.Customer;
                    BoothsVM.SetCurrentCustomer(CustomersVM.SelectedCustomer);
                }
            };

            // select customer (e.g., from details) => filter their booths
            CustomersVM.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CustomersViewModel.SelectedCustomer))
                    BoothsVM.SetCurrentCustomer(CustomersVM.SelectedCustomer);
            };

            // search options
            SearchModes = new()
        {
            new() { DisplayName = "Alle",          SearchMode = SearchMode.All },
            new() { DisplayName = "Reol nummer",   SearchMode = SearchMode.BoothNumber },
            new() { DisplayName = "Kunde navn",    SearchMode = SearchMode.CustomerName },
            new() { DisplayName = "Kunde telefon", SearchMode = SearchMode.CustomerPhone },
            new() { DisplayName = "Kunde email",   SearchMode = SearchMode.CustomerEmail }
        };
            SelectedSearchMode = SearchModes.First();

            _refreshDebounce.Tick += (_, __) => { _refreshDebounce.Stop(); ApplySearch(); };
            ApplySearch(); // initial
        }

        private void RequestRefresh()
        {
            _refreshDebounce.Stop();
            _refreshDebounce.Start();
        }

        // Compile the search inputs into a predicate and push it to BoothsVM
        private void ApplySearch()
        {
            var txt = SearchText?.Trim();
            var mode = SelectedSearchMode?.SearchMode ?? SearchMode.All;

            Func<Booth, bool>? predicate = null;
            if (!string.IsNullOrEmpty(txt))
            {
                predicate = b => mode switch
                {
                    SearchMode.All => b.BoothNumber.ToString().Contains(txt, StringComparison.OrdinalIgnoreCase)
                                   || (b.Customer != null && (
                                         b.Customer.CustomerName.Contains(txt, StringComparison.OrdinalIgnoreCase)
                                      || b.Customer.PhoneNumber.Contains(txt)
                                      || b.Customer.Email.Contains(txt, StringComparison.OrdinalIgnoreCase))),
                    SearchMode.BoothNumber => b.BoothNumber.ToString().Contains(txt),
                    SearchMode.CustomerName => b.Customer?.CustomerName?.Contains(txt, StringComparison.OrdinalIgnoreCase) == true,
                    SearchMode.CustomerPhone => b.Customer?.PhoneNumber?.Contains(txt) == true,
                    SearchMode.CustomerEmail => b.Customer?.Email?.Contains(txt, StringComparison.OrdinalIgnoreCase) == true,
                    _ => true
                };
            }

            BoothsVM.SetExternalFilter(predicate);  // parent pushes filter
            BoothsVM.RefreshViews();                // single debounced refresh point
        }
    }
}
