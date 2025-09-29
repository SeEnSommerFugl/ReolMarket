using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    internal class AdminPopUpViewModel : BaseViewModel
    {
        private readonly IBaseRepository<Booth, Guid> _boothRepo;
        private readonly IBaseRepository<Customer, Guid> _customerRepo;

        private readonly ICollectionView _renterComboBox;
        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<Customer> Customers => _customerRepo.Items;
        public ICollectionView BoothView { get; }
        public ICollectionView RenterComboBox => _renterComboBox;

        // Track selections using HashSet - super fast lookups!
        private HashSet<Guid> _selectedBoothIds = new HashSet<Guid>();

        public ICommand SaveCommand { get; }
        public ICommand NewRenterCommand { get; }
        public ICommand EditRenterCommand { get; }

        public AdminPopUpViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Customer, Guid> customerRepo)
        {
            _boothRepo = boothRepo;
            _customerRepo = customerRepo;

            _renterComboBox = CollectionViewSource.GetDefaultView(Customers);
            _renterComboBox.SortDescriptions.Add(
                new SortDescription(nameof(Customer.CustomerName), ListSortDirection.Ascending));

            BoothView = CollectionViewSource.GetDefaultView(Booths);
            BoothView.Filter = BoothFilter;
            BoothView.SortDescriptions.Add(new SortDescription(nameof(Booth.IsRented), ListSortDirection.Descending));
            BoothView.SortDescriptions.Add(new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));

            Booths.CollectionChanged += (_, __) => BoothView.Refresh();

            SaveCommand = new RelayCommand(_ => AddOrEditRenter(), _ => CanSave());
            NewRenterCommand = new RelayCommand(_ => NewRenter(), _ => CanRun());
            EditRenterCommand = new RelayCommand(_ => EditRenter(), _ => CanRun());

            //_boothView.Refresh();
        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    LoadAvailableBooths(); // This will handle pre-selection

                    // Auto-populate form fields when a customer is selected
                    if (_selectedCustomer != null && IsEditing)
                    {
                        CustomerName = _selectedCustomer.CustomerName;
                        Email = _selectedCustomer.Email;
                        PhoneNumber = _selectedCustomer.PhoneNumber;
                        Address = _selectedCustomer.Address;
                        PostalCode = _selectedCustomer.PostalCode;
                    }

                }
            }
        }
        #region Properties

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                SetProperty(ref _isEditing, value);
            }
        }

        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set
            {
                SetProperty(ref _customerName, value); // Remove the empty 'if' statement
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value); // Remove the empty 'if' statement
            }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                SetProperty(ref _phoneNumber, value); // Remove the empty 'if' statement
            }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                SetProperty(ref _address, value); // Remove the empty 'if' statement
            }
        }

        private string _postalCode;
        public string PostalCode
        {
            get => _postalCode;
            set
            {
                SetProperty(ref _postalCode, value); // Remove the empty 'if' statement
            }
        }

        public void NewRenter()
        {
            IsEditing = false;
            SelectedCustomer = null;

            CustomerName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Address = string.Empty;
            PostalCode = string.Empty;
        }

        public void EditRenter()
        {
            IsEditing = true;

        }
        #endregion

        public bool IsBoothSelected(Guid boothId) => _selectedBoothIds.Contains(boothId);


        public void SetBoothSelection(Guid boothId, bool isSelected)
        {
            if (isSelected)
                _selectedBoothIds.Add(boothId);    // Add to HashSet
            else
                _selectedBoothIds.Remove(boothId); // Remove from HashSet
        }

        public void ToggleBoothSelection(Guid boothId)
        {
            if (_selectedBoothIds.Contains(boothId))
                _selectedBoothIds.Remove(boothId);
            else
                _selectedBoothIds.Add(boothId);
        }
        private void AddOrEditRenter()
        {
            if (!CanSave()) return;

            RunBusy(() =>
            {
                if (IsEditing == true)
                {
                    var customerToUpdate = SelectedCustomer;
                    if (customerToUpdate != null)
                    {
                        customerToUpdate.CustomerName = CustomerName!.Trim();
                        customerToUpdate.Email = Email!.Trim();
                        customerToUpdate.PhoneNumber = PhoneNumber!.Trim();
                        customerToUpdate.Address = Address!.Trim();
                        customerToUpdate.PostalCode = PostalCode!.Trim();

                        Validate();
                        UpdateBoothAssignments();
                        _customerRepo.Update(customerToUpdate);
                    }
                }
                else
                {
                    var customer = new Customer()
                    {
                        CustomerID = Guid.NewGuid(),
                        CustomerName = _customerName,
                        Email = _email,
                        PhoneNumber = _phoneNumber,
                        Address = _address,
                        PostalCode = _postalCode,
                    };
                    Validate();
                    _customerRepo.Add(customer);
                    SelectedCustomer = customer;
                    UpdateBoothAssignments();
                }


            });
        }

        private void UpdateBoothAssignments()
        {

            if (SelectedCustomer == null) return;

            var updatedBooths = new List<Booth>();
            var now = DateTime.Now;
            var endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            // Single pass through Booths to update assignments
            foreach (var booth in Booths)
            {
                bool isSelected = _selectedBoothIds.Contains(booth.BoothID);
                bool isCurrentCustomerBooth = booth.CustomerID == SelectedCustomer.CustomerID;

                if (isSelected && !isCurrentCustomerBooth)
                {
                    // Assign booth to customer
                    booth.CustomerID = SelectedCustomer.CustomerID;
                    booth.IsRented = true;
                    booth.Status = BoothStatus.Optaget;
                    booth.StartDate = now;
                    booth.EndDate = null;
                    updatedBooths.Add(booth);
                }
                else if (!isSelected && isCurrentCustomerBooth)
                {
                    // Unassign booth
                    booth.CustomerID = null;
                    booth.IsRented = false;
                    booth.Status = BoothStatus.Ledig;
                    booth.StartDate = null;
                    booth.EndDate = endOfMonth;
                    updatedBooths.Add(booth);
                }
            }

            if (updatedBooths.Any())
            {
                _boothRepo.UpdateRange(updatedBooths);
                BoothView.Refresh(); // Refresh only if updates occurred
            }
        }


        private bool BoothFilter(object obj)
        {
            if (obj is not Booth b) return false;

            bool isFree = b.CustomerID == null && !b.IsRented && b.Status == BoothStatus.Ledig;

            if (!IsEditing)
                return isFree;                // <-- Add mode: only free booths

            var cid = SelectedCustomer?.CustomerID;
            if (!cid.HasValue)
                return isFree;                // <-- Edit mode but no customer: still only free

            bool isCustomers =
                b.CustomerID == cid.Value
                && (b.IsRented || b.Status == BoothStatus.Optaget);

            return isCustomers || isFree;    // <-- Edit mode with customer: customer's booths OR free
        }

        private void LoadAvailableBooths()
        {

            // Only clear selections for the current customer, then rebuild them from database state
            if (_selectedCustomer != null)
            {
                foreach (var boothId in Booths)
                {
                    _selectedBoothIds.Remove(boothId.BoothID);
                }

                // Re-add based on actual database state (IsRented = true)
                foreach (var booth in Booths.Where(b =>
                    b.CustomerID == _selectedCustomer.CustomerID && b.IsRented))
                {
                    _selectedBoothIds.Add(booth.BoothID);
                }
            }
            else
            {
                // If no customer selected, clear all selections (for "Add New" mode)
                _selectedBoothIds.Clear();
            }
            BoothView.Refresh();
        }



        #region Validation

        protected override void Validate()
        {
            ClearErrors(nameof(CustomerName));
            if (string.IsNullOrWhiteSpace(CustomerName))
                AddError(nameof(CustomerName), "Indtast venligst navn.");

            ClearErrors(nameof(Email));
            if (string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
                AddError(nameof(Email), "Forkert email format.");

            ClearErrors(nameof(PhoneNumber));
            if (string.IsNullOrWhiteSpace(PhoneNumber))
                AddError(nameof(PhoneNumber), "Indtast venligst telefonnummer.");
            else if (PhoneNumber.Any(ch => !char.IsDigit(ch)) || PhoneNumber.Length != 8)
                AddError(nameof(PhoneNumber), "Telefonnummer skal være 8 cifre.");

            ClearErrors(nameof(Address));
            if (string.IsNullOrWhiteSpace(Address))
                AddError(nameof(Address), "Indtast venligst en adresse.");

            ClearErrors(nameof(PostalCode));
            int code = ParsePostalCode(PostalCode);
            if (code < 1000 || code > 9999)
                AddError(nameof(PostalCode), "Postnummeret skal være mellem 1000 og 9999.");

            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private int ParsePostalCode(string? postalCode)
        {
            if (int.TryParse(postalCode, out int code))
                return code;
            return -1; // Return an invalid code if parsing fails
        }

        private bool CanSave() => !IsBusy && !HasErrors;
        private bool CanRun() => !IsBusy;

    }
    #endregion
}
