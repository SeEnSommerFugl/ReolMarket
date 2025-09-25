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

            _boothView = CollectionViewSource.GetDefaultView(Booths);
            _boothView.Filter = Filter;
            _boothView.SortDescriptions.Add
                (new SortDescription(nameof(Booth.BoothNumber), ListSortDirection.Ascending));

            SaveCommand = new RelayCommand(_ => AddOrEditRenter(), _ => CanSave());


        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    _renterComboBox.Refresh();
                    //RefreshCommands();
                }
            }
        }

        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    _renterComboBox.Refresh();
                }
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
                    _renterComboBox.Refresh();
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
                    _renterComboBox.Refresh();
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
                    _renterComboBox.Refresh();
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
                    _renterComboBox.Refresh();
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
                    _renterComboBox.Refresh();
                }
            }
        }




        private bool Filter(object obj)
        {
            if (IsEditing == true)
                return false;
            if (obj is not Booth booth)
                return false;

            return booth.CustomerID == null && booth.Status == BoothStatus.Ledig && booth.IsRented == false;

        }

        private void AddOrEditRenter()
        {
            if (!CanSave()) return;

            RunBusy(() =>
            {
                if (IsEditing == true)
                {
                    if (SelectedCustomer != null)
                    {
                        SelectedCustomer.CustomerName = CustomerName!.Trim();
                        SelectedCustomer.Email = Email!.Trim();
                        SelectedCustomer.PhoneNumber = PhoneNumber!.Trim();
                        SelectedCustomer.Address = Address!.Trim();
                        SelectedCustomer.PostalCode = PostalCode!.Trim();

                        _customerRepo.Update(SelectedCustomer);
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
                    _customerRepo.Add(customer);
                }
                _boothView.Refresh();
            });
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
            if (!string.IsNullOrWhiteSpace(PhoneNumber))
                AddError(nameof(PhoneNumber), "Indtast venligst telefon nummer.");

            ClearErrors(nameof(Address));
            if (!string.IsNullOrWhiteSpace(Address))
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
