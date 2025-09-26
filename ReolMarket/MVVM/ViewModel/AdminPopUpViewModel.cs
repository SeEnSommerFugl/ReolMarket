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

        private readonly ICollectionView _boothView;
        private readonly ICollectionView _renterComboBox;

        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<BoothViewModel> AvailableBooths { get; set; }
        public ObservableCollection<Customer> Customers => _customerRepo.Items;
        public ICollectionView BoothView => _boothView;
        public ICollectionView RenterComboBox => _renterComboBox;

        public ICommand SaveCommand { get; }
        public ICommand NewRenterCommand { get; }
        public ICommand EditRenterCommand { get; }

        public AdminPopUpViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Customer, Guid> customerRepo)
        {
            _boothRepo = boothRepo;
            _customerRepo = customerRepo;
            AvailableBooths = new ObservableCollection<BoothViewModel>();



            _renterComboBox = CollectionViewSource.GetDefaultView(Customers);
            _renterComboBox.SortDescriptions.Add(
                new SortDescription(nameof(Customer.CustomerName), ListSortDirection.Ascending));

            _boothView = CollectionViewSource.GetDefaultView(AvailableBooths);
            _boothView.Filter = BoothFilter;
            _boothView.GroupDescriptions.Add(
                new PropertyGroupDescription(nameof(Booth.IsRented)));
            _boothView.SortDescriptions.Add
                (new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));

            SaveCommand = new RelayCommand(_ => AddOrEditRenter(), _ => CanSave());
            NewRenterCommand = new RelayCommand(_ => NewRenter(), _ => CanRun());
            EditRenterCommand = new RelayCommand(_ => EditRenter(), _ => CanRun());

            LoadAvailableBooths();

        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    // Auto-populate form fields when a customer is selected
                    if (value != null && IsEditing)
                    {
                        CustomerName = value.CustomerName;
                        Email = value.Email;
                        PhoneNumber = value.PhoneNumber;
                        Address = value.Address;
                        PostalCode = value.PostalCode;
                    }
                    _boothView.Refresh();
                    //RefreshCommands();
                }
            }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value)) ;
                _boothView.Refresh();
            }
        }

        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (SetProperty(ref _customerName, value))
                {
                    Validate();
                }
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    Validate();
                }
            }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (SetProperty(ref _phoneNumber, value))
                {
                    Validate();
                }
            }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                {
                    Validate();
                }
            }
        }

        private string _postalCode;
        public string PostalCode
        {
            get => _postalCode;
            set
            {
                if (SetProperty(ref _postalCode, value))
                {
                    Validate();
                }
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

                        _customerRepo.Update(customerToUpdate);
                    }
                    SelectedCustomer = customerToUpdate;
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
                    _customerRepo.Add(customer);
                    SelectedCustomer = customer;
                }
                UpdateBoothAssignments();
                _boothView.Refresh();
            });
        }

        private void UpdateBoothAssignments()
        {
            if (SelectedCustomer == null) return;

            var selectedBooths = AvailableBooths.Where(bvm => bvm.IsSelected).Select(bvm => bvm.Booth).ToList();
            var unselectedBooths = AvailableBooths.Where(bvm => !bvm.IsSelected && bvm.Booth.CustomerID == SelectedCustomer.CustomerID)
                                                    .Select(bvm => bvm.Booth).ToList();

            // Assign selected booths to customer
            foreach (var booth in selectedBooths)
            {
                booth.CustomerID = SelectedCustomer.CustomerID;
                booth.IsRented = true;
                booth.Status = BoothStatus.Optaget;
            }

            // Remove assignments from unselected booths
            foreach (var booth in unselectedBooths)
            {
                booth.CustomerID = null;
                booth.IsRented = false;
                booth.Status = BoothStatus.Ledig;
            }

            // Update booths in repository
            _boothRepo.UpdateRange(selectedBooths.Concat(unselectedBooths));
        }

        private bool BoothFilter(object obj)
        {
            if (obj is not BoothViewModel bvm) return false;  // ✅ Cast to BoothViewModel

            var b = bvm.Booth;  // ✅ Get the underlying Booth

            bool isFree = b.CustomerID == null && !b.IsRented && b.Status == BoothStatus.Ledig;

            if (!IsEditing)
                return isFree;                // <-- Add mode: only free booths

            var cid = SelectedCustomer?.CustomerID;
            if (!cid.HasValue)
                return isFree;                // <-- Edit mode but no customer: still only free

            bool isCustomers =
                b.CustomerID == cid.Value
                && (b.IsRented || b.Status == BoothStatus.Optaget);

            return isCustomers || isFree;     // <-- Edit mode with customer: customer's booths OR free
        }

        private void LoadAvailableBooths()
        {
            AvailableBooths.Clear();

            // Use your existing repository approach
            var availableBooths = Booths;

            // Create ViewModels only for UI selection purposes
            foreach (var booth in availableBooths)
            {
if (_selectedCustomer != null && booth.CustomerID == _selectedCustomer.CustomerID)
        {
            boothVm.IsSelected = true;  // Pre-check this customer's booths
        }
                AvailableBooths.Add(new BoothViewModel(booth));
            }
            _boothView.Refresh();
        }

        protected override void Validate()
        {
            ClearErrors(nameof(CustomerName));
            if (string.IsNullOrWhiteSpace(CustomerName))
                AddError(nameof(CustomerName), "Indtast venligst navn.");

            ClearErrors(nameof(Email));
            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
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
