using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.MVVM.Model;         // Booth, Customer
using ReolMarket.Data.Repository;    // BoothDbRepository, CustomerDbRepository

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel for assigning a booth to a customer (creating a lease).
    /// MVP version: prepares data and validates input; no persistence yet.
    /// </summary>
    internal class LeaseCreateViewModel : BaseViewModel
    {
        // Repositories (sync)
        private readonly BoothDbRepository _boothRepo;
        private readonly CustomerDbRepository _customerRepo;

        // Backing fields
        private Booth? _selectedBooth;
        private Customer? _selectedCustomer;
        private DateTime _startDate = DateTime.Today;
        private decimal _monthlyPrice = 850m;
        private decimal _commissionPercent = 10m;

        /// <summary>
        /// All booths available to choose from.
        /// Bind this to a list or ComboBox.
        /// </summary>
        public ObservableCollection<Booth> AvailableBooths { get; } = new();

        /// <summary>
        /// All customers available to choose from.
        /// Bind this to a list or ComboBox.
        /// </summary>
        public ObservableCollection<Customer> Customers { get; } = new();

        /// <summary>
        /// The booth the user selected for this lease.
        /// </summary>
        public Booth? SelectedBooth
        {
            get => _selectedBooth;
            set { if (SetProperty(ref _selectedBooth, value)) Validate(); }
        }

        /// <summary>
        /// The customer the user selected for this lease.
        /// </summary>
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set { if (SetProperty(ref _selectedCustomer, value)) Validate(); }
        }

        /// <summary>
        /// Start date of the lease. Defaults to today.
        /// </summary>
        public DateTime StartDate
        {
            get => _startDate;
            set { if (SetProperty(ref _startDate, value)) Validate(); }
        }

        /// <summary>
        /// Monthly rent. Must be greater than or equal to zero.
        /// </summary>
        public decimal MonthlyPrice
        {
            get => _monthlyPrice;
            set { if (SetProperty(ref _monthlyPrice, value)) Validate(); }
        }

        /// <summary>
        /// Commission percentage taken on sales (0–100).
        /// </summary>
        public decimal CommissionPercent
        {
            get => _commissionPercent;
            set { if (SetProperty(ref _commissionPercent, value)) Validate(); }
        }

        /// <summary>
        /// Name of the staff member creating the lease.
        /// </summary>
        public string StaffName { get; } = Environment.UserName;

        /// <summary>
        /// Saves the lease (placeholder in MVP).
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Cancels the dialog.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Raised when the dialog should close.
        /// True = saved, False = cancelled.
        /// </summary>
        public event Action<bool>? RequestClose;

        /// <summary>
        /// Creates the view model, loads data, and wires up commands.
        /// </summary>
        public LeaseCreateViewModel()
        {
            Title = "Assign Booth to Customer";

            _boothRepo = new BoothDbRepository();
            _customerRepo = new CustomerDbRepository();

            SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));

            Load();
        }

        /// <summary>
        /// Loads booths and customers from repositories and validates the form.
        /// </summary>
        public void Load()
        {
            RunBusy(() =>
            {
                AvailableBooths.Clear();
                foreach (var b in _boothRepo.GetAll().OrderBy(b => b.BoothNumber))
                    AvailableBooths.Add(b);

                Customers.Clear();
                foreach (var c in _customerRepo.GetAll().OrderBy(c => c.CustomerName))
                    Customers.Add(c);

                Validate();
            }, "Loading lease data…");
        }

        /// <summary>
        /// Validates user input and updates command states.
        /// </summary>
        protected override void Validate()
        {
            ClearErrors(nameof(SelectedBooth));
            if (SelectedBooth == null)
                AddError(nameof(SelectedBooth), "Booth must be selected.");

            ClearErrors(nameof(SelectedCustomer));
            if (SelectedCustomer == null)
                AddError(nameof(SelectedCustomer), "Customer must be selected.");

            ClearErrors(nameof(StartDate));
            if (StartDate > DateTime.Today)
                AddError(nameof(StartDate), "Start date cannot be in the future.");

            ClearErrors(nameof(MonthlyPrice));
            if (MonthlyPrice < 0)
                AddError(nameof(MonthlyPrice), "Monthly price must be ≥ 0.");

            ClearErrors(nameof(CommissionPercent));
            if (CommissionPercent < 0 || CommissionPercent > 100)
                AddError(nameof(CommissionPercent), "Commission must be between 0 and 100.");

            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Returns true if the form is valid and not busy.
        /// </summary>
        private bool CanSave() => !IsBusy && !HasErrors;

        /// <summary>
        /// Placeholder save handler for MVP.
        /// Emits a close event with success = true.
        /// </summary>
        private void ExecuteSave()
        {
            if (!CanSave()) return;

            // Future: create and persist a lease record.
            RequestClose?.Invoke(true);
        }
    }
}
