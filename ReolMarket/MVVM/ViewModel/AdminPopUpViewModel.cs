using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    internal class AdminPopUpViewModel : BaseViewModel
    {
        private IBaseRepository<Booth, Guid> _boothRepo;
        private IBaseRepository<Customer, Guid> _customerRepo;

        public AdminPopUpViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Customer, Guid> customerRepo)
        {
            _boothRepo = boothRepo;
            _customerRepo = customerRepo;
        }
    }
}
