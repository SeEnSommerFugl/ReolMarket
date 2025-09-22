using System.Collections.ObjectModel;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.Data.Repository;     // CustomerDbRepository
using ReolMarket.MVVM.Model;          // Customer

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel that lists and manages renters (customers).
    /// Supports simple filtering, selection, and basic CRUD.
    /// </summary>
    internal class RentersViewModelOLD : BaseViewModel
    {
        // Repository (sync)
        private readonly CustomerDbRepository _customerRepo;

        // Backing fields
        private Customer? _selectedCustomer;
        private string? _searchText;

        // Cache of unfiltered data
        private Customer[] _allCustomers = Array.Empty<Customer>();

        /// <summary>
        /// Customers shown in the UI after filtering.
        /// </summary>
        public ObservableCollection<Customer> Renters { get; } = new();

        /// <summary>
        /// The customer currently selected in the list.
        /// </summary>
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                    RefreshCommands();
            }
        }

        /// <summary>
        /// Free-text search across name, email, and phone.
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
        /// Reloads customers from the repository.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Adds a new placeholder customer.
        /// </summary>
        public ICommand AddRenterCommand { get; }

        /// <summary>
        /// Saves a small change to the selected customer.
        /// </summary>
        public ICommand EditRenterCommand { get; }

        /// <summary>
        /// Deletes the selected customer.
        /// </summary>
        public ICommand DeleteRenterCommand { get; }

        /// <summary>
        /// Creates the ViewModel and loads initial data.
        /// </summary>
        public RentersViewModelOLD()
        {
            Title = "Renters";
            _customerRepo = new CustomerDbRepository();

            RefreshCommand = new RelayCommand(_ => Load());
            AddRenterCommand = new RelayCommand(_ => AddRenter(), _ => !IsBusy);
            EditRenterCommand = new RelayCommand(_ => EditRenter(), _ => !IsBusy && SelectedCustomer != null);
            DeleteRenterCommand = new RelayCommand(_ => DeleteRenter(), _ => !IsBusy && SelectedCustomer != null);

            Load();
        }

        /// <summary>
        /// Loads all customers and applies current filters.
        /// </summary>
        private void Load()
        {
            RunBusy(() =>
            {
                _allCustomers = _customerRepo.GetAll().ToArray();
                ApplyFilters();
            }, "Loading renters…");
        }

        /// <summary>
        /// Applies the text filter to the cached list
        /// and updates the UI collection.
        /// </summary>
        private void ApplyFilters()
        {
            var query = _allCustomers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim();
                query = query.Where(c =>
                    (!string.IsNullOrWhiteSpace(c.CustomerName) && c.CustomerName.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(c.PhoneNumber) && c.PhoneNumber.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            var result = query
                .OrderBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Renters.Clear();
            foreach (var c in result)
                Renters.Add(c);
        }

        /// <summary>
        /// Adds a simple placeholder customer and saves it.
        /// </summary>
        private void AddRenter()
        {
            RunBusy(() =>
            {
                var c = new Customer
                {
                    CustomerID = Guid.NewGuid(),
                    CustomerName = "New customer",
                    Email = "example@email.com",
                    PhoneNumber = "00000000",
                    Address = "",
                    PostalCode = ""
                };
                _customerRepo.Add(c);
                Load();
            }, "Adding renter…");
        }

        /// <summary>
        /// Makes a small visible change to the selected customer and saves it.
        /// </summary>
        private void EditRenter()
        {
            if (SelectedCustomer == null) return;

            RunBusy(() =>
            {
                SelectedCustomer.CustomerName = (SelectedCustomer.CustomerName ?? string.Empty).Trim() + " *";
                _customerRepo.Update(SelectedCustomer);
                Load();
            }, "Saving renter…");
        }

        /// <summary>
        /// Deletes the selected customer and reloads the list.
        /// </summary>
        private void DeleteRenter()
        {
            if (SelectedCustomer == null) return;

            RunBusy(() =>
            {
                _customerRepo.Delete(SelectedCustomer.CustomerID);
                Load();
            }, "Deleting renter…");
        }

        /// <summary>
        /// Updates command states after selection changes.
        /// </summary>
        private void RefreshCommands()
        {
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddRenterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (EditRenterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteRenterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
