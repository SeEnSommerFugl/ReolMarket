using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel for showing settlements per customer.
    /// For MVP only. No external services or sales data yet.
    /// </summary>
    internal class EconomyViewModel : BaseViewModel
    {
        private readonly IBaseRepository<Booth, Guid> _boothRepo;
        private readonly IBaseRepository<Customer, Guid> _customerRepo;
        private readonly IBaseRepository<Sale, Guid> _saleRepo;
        private readonly ICollectionView _quickRangesComboBox;

        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<Customer> Customer => _customerRepo.Items;
        public ObservableCollection<Sale> Sales => _saleRepo.Items;
        public ICollectionView EconomyView { get; }
        public ICollectionView QuickRangesComboBox => _quickRangesComboBox;
        public ObservableCollection<CustomerSettlementVm> CustomerSettlements { get; } = new();

        /// <summary>
        /// Start date for the settlement period.
        /// </summary>
        private DateTime _periodStart = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTime PeriodStart
        {
            get => _periodStart;
            set => SetProperty(ref _periodStart, value);
        }

        /// <summary>
        /// End date for the settlement period.
        /// </summary>
        private DateTime _periodEnd = DateTime.Today;
        public DateTime PeriodEnd
        {
            get => _periodEnd;
            set => SetProperty(ref _periodEnd, value);
        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    if (_selectedCustomer != null)
                    {
                        CustomerName = _selectedCustomer.CustomerName;
                    }
                }
            }
        }

        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set
            {
                SetProperty(ref _customerName, value);
            }
        }

        /// <summary>
        /// Command that generates the settlement list.
        /// </summary>
        public ICommand GenerateCommand { get; }

        /// <summary>
        /// Creates the view model and sets default dates.
        /// </summary>
        public EconomyViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Customer, Guid> customerRepo, IBaseRepository<Sale, Guid> saleRepo)
        {
            _boothRepo = boothRepo;
            _customerRepo = customerRepo;
            _saleRepo = saleRepo;

            ExecuteGenerate();
        }

        /// <summary>
        /// Generates settlement data. (Clears list for now.)
        /// </summary>
        private void ExecuteGenerate()
        {
            RunBusy(() =>
            {
                CustomerSettlements.Clear();

                var customersWithBooths = _customerRepo.Items
                    .Where(c => _boothRepo.Items.Any(b => b.CustomerID == c.CustomerID && (b.IsRented || b.Status == BoothStatus.Optaget)))
                    .ToList();

                foreach (var customer in customersWithBooths)
                {
                    var booths = _boothRepo.Items.Where(b => b.CustomerID == customer.CustomerID).ToList();
                    int boothCount = booths.Count();

                    decimal rentPerBooth = boothCount switch
                    {
                        0 => 0m,
                        1 => 850m,
                        2 or 3 => 825,
                        _ => 800m
                    };

                    decimal salesTotal = 0m;
                    decimal comissionPercent = 10m;
                    decimal comission = Math.Round(salesTotal * (comissionPercent / 100m), 2);
                    decimal payout = salesTotal - comission - (boothCount * rentPerBooth);

                    var settlement = new CustomerSettlementVm
                    {
                        CustomerName = customer.CustomerName,
                        BoothCount = boothCount,
                        RentPerBooth = rentPerBooth,
                        TotalRent = boothCount * rentPerBooth,
                        SalesTotal = salesTotal,
                        CommissionPercent = comissionPercent,
                        CommissionAmount = comission,
                        FinalPayout = payout
                    };

                    CustomerSettlements.Add(settlement);
                }
            }, "Generating settlement…");
        }


        private bool FilterBooth(object obj, object obj2)
        {
            if (obj is not Booth booth || obj2 is not Customer customer)
                return false;

            var customerId = SelectedCustomer?.CustomerID;
            var boothCustomerID = booth.CustomerID;

            return customerId == boothCustomerID;
        }
    }
    internal sealed class CustomerSettlementVm
    {
        public string CustomerName { get; set; } = string.Empty;
        public int BoothCount { get; set; }
        public decimal RentPerBooth { get; set; }
        public decimal TotalRent { get; set; }

        public decimal SalesTotal { get; set; }
        public decimal CommissionPercent { get; set; }
        public decimal CommissionAmount { get; set; }

        public decimal FinalPayout { get; set; }
    }
}
