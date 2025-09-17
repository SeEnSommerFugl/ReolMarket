using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.MVVM.Model;
using ReolMarket.Data.Repository;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel for creating or editing a renter (customer).
    /// Uses synchronous repository calls.
    /// </summary>
    internal class RenterDetailViewModel : BaseViewModel
    {
        private readonly CustomerDbRepository _repo;
        private Guid? _id;

        private string? _name;
        private string? _email;
        private string? _phone;
        private string? _address;
        private int _postalCode;

        /// <summary>
        /// Raised when the dialog should close.
        /// True = saved, False = cancelled.
        /// </summary>
        public event Action<bool>? RequestClose;

        /// <summary>
        /// Customer's full name. Required.
        /// </summary>
        public string? CustomerName
        {
            get => _name;
            set { if (SetProperty(ref _name, value)) Validate(); }
        }

        /// <summary>
        /// Customer's email. Optional, but validated if set.
        /// </summary>
        public string? Email
        {
            get => _email;
            set { if (SetProperty(ref _email, value)) Validate(); }
        }

        /// <summary>
        /// Customer's phone number. Optional.
        /// </summary>
        public string? PhoneNumber
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        /// <summary>
        /// Postal address. Optional.
        /// </summary>
        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        /// <summary>
        /// Postal code (1000–9999). Validated.
        /// </summary>
        public int PostalCode
        {
            get => _postalCode;
            set { if (SetProperty(ref _postalCode, value)) Validate(); }
        }

        /// <summary>
        /// Saves the customer (create or update).
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Cancels editing and closes the dialog.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Creates the ViewModel and wires up commands.
        /// </summary>
        public RenterDetailViewModel()
        {
            Title = "Customer Details";
            _repo = new CustomerDbRepository();

            SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
        }

        /// <summary>
        /// Loads an existing customer for edit, or prepares defaults for a new one.
        /// </summary>
        /// <param name="customerId">Existing customer id, or null to create new.</param>
        public void Load(Guid? customerId = null)
        {
            _id = customerId;

            if (customerId.HasValue)
            {
                var c = _repo.GetById(customerId.Value);
                if (c != null)
                {
                    CustomerName = c.CustomerName;
                    Email = c.Email;
                    PhoneNumber = c.PhoneNumber;
                    Address = c.Address;
                    PostalCode = c.PostalCode;
                }
            }
            else
            {
                CustomerName = "";
                Email = "";
                PhoneNumber = "";
                Address = "";
                PostalCode = 1000;
            }

            Validate();
        }

        /// <summary>
        /// Validates user input and updates error state.
        /// </summary>
        protected override void Validate()
        {
            ClearErrors(nameof(CustomerName));
            if (string.IsNullOrWhiteSpace(CustomerName))
                AddError(nameof(CustomerName), "Name is required.");

            ClearErrors(nameof(Email));
            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
                AddError(nameof(Email), "Invalid email format.");

            ClearErrors(nameof(PostalCode));
            if (PostalCode < 1000 || PostalCode > 9999)
                AddError(nameof(PostalCode), "Postal code must be between 1000 and 9999.");

            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Returns true when the form has no errors and the VM is not busy.
        /// </summary>
        private bool CanSave() => !IsBusy && !HasErrors;

        /// <summary>
        /// Creates or updates the customer and closes with success.
        /// </summary>
        private void ExecuteSave()
        {
            if (!CanSave()) return;

            RunBusy(() =>
            {
                if (_id.HasValue)
                {
                    var customer = _repo.GetById(_id.Value);
                    if (customer != null)
                    {
                        customer.CustomerName = CustomerName!.Trim();
                        customer.Email = Email?.Trim() ?? "";
                        customer.PhoneNumber = PhoneNumber?.Trim() ?? "";
                        customer.Address = Address?.Trim() ?? "";
                        customer.PostalCode = PostalCode;

                        _repo.Update(customer);
                    }
                }
                else
                {
                    var newCustomer = new Customer
                    {
                        CustomerID = Guid.NewGuid(),
                        CustomerName = CustomerName!.Trim(),
                        Email = Email?.Trim() ?? "",
                        PhoneNumber = PhoneNumber?.Trim() ?? "",
                        Address = Address?.Trim() ?? "",
                        PostalCode = PostalCode
                    };

                    _repo.Add(newCustomer);
                }

                RequestClose?.Invoke(true);
            }, "Saving customer…");
        }

        /// <summary>
        /// Returns true if the email has a simple valid format.
        /// </summary>
        /// <param name="email">Email to check.</param>
        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }
    }
}
