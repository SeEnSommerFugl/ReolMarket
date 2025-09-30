using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel for SaleView: shows sale
    /// </summary>
    internal class SaleViewModel : BaseViewModel
    {
        private readonly IBaseRepository<Sale, Guid> _saleRepo;
        private readonly IBaseRepository<Booth, Guid> _boothRepo;
        private readonly IBaseRepository<Customer, Guid> _customerRepo;

        public ObservableCollection<Sale> Sales => _saleRepo.Items;
        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<Customer> Customers => _customerRepo.Items;

        public ICollectionView SaleView {  get; }

        /// <summary>
        /// Start date for the sale period.
        /// </summary>

        private DateTime _periodStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTime PeriodStart
        {
            get => _periodStart;
            set => SetProperty(ref _periodStart, value); 
        }

        /// <summary>
        /// end date for the sale period.
        /// </summary>
        
        private DateTime _periodEnd = DateTime.Today;
        public DateTime PeriodEnd
        {
            get => _periodEnd;
            set => SetProperty(ref _periodEnd, value);
        }

        public SaleViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Customer, Guid> customerRepo, IBaseRepository<Sale, Guid> saleRepo)
        {
            _boothRepo = boothRepo;
            _customerRepo = customerRepo;
            _saleRepo = saleRepo;

            //ExecuteGenerate();
        }
    }
}
