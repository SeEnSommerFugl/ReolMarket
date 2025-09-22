using System.Collections.ObjectModel;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    public class CustomersViewModel : BaseViewModel
    {
        private readonly IBaseRepository<Customer, Guid> _customerRepo;
        public ObservableCollection<Customer> Customers => _customerRepo.Items;

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public CustomersViewModel(IBaseRepository<Customer, Guid> customerRepo)
        {
            _customerRepo = customerRepo;
            _customerRepo.GetAll();
        }
    }
}

