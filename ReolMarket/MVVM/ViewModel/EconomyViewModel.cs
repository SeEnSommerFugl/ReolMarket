using System.Collections.ObjectModel;
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

        private DateTime _periodStart = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _periodEnd = DateTime.Today;

        /// <summary>
        /// Start date for the settlement period.
        /// </summary>
        public DateTime PeriodStart
        {
            get => _periodStart;
            set => SetProperty(ref _periodStart, value);
        }

        /// <summary>
        /// End date for the settlement period.
        /// </summary>
        public DateTime PeriodEnd
        {
            get => _periodEnd;
            set => SetProperty(ref _periodEnd, value);
        }

        /// <summary>
        /// The settlement results. One row per item sold.
        /// </summary>
        //public ObservableCollection<SettlementLineVm> SettlementLines { get; } = new();
        public ObservableCollection<CustomerSettlementVm> CustomerSettlements { get; } = new();

        /// <summary>
        /// Command that generates the settlement list.
        /// </summary>
        public ICommand GenerateCommand { get; }

        /// <summary>
        /// Creates the view model and sets default dates.
        /// </summary>
        //public EconomyViewModel()
        //{
        //    Title = "Monthly Settlement";
        //    GenerateCommand = new RelayCommand(_ => ExecuteGenerate(), _ => !IsBusy);
        //}
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
                foreach (var customer in _customerRepo.GetAll())
                {
                    var booths = _boothRepo.GetAll().Where(b => b.CustomerID == customer.CustomerID).ToList();
                    int boothCount = booths.Count();

                    decimal rentPerBooth = boothCount switch
                    {
                        0 => 0m,
                        1 => 850m,
                        2 or 3 => 825,
                        _ => 800m
                    };

                    decimal totalRent = boothCount * rentPerBooth;

                    decimal salesTotal = 0m; //TODO: Hent salgsdata
                    decimal comissionPercent = 10m;
                    decimal comission = Math.Round(salesTotal * (comissionPercent / 100m), 2);
                    decimal payout = salesTotal - comission - totalRent;

                    CustomerSettlements.Add(new CustomerSettlementVm
                    {
                        CustomerName = customer.CustomerName,
                        BoothCount = boothCount,
                        RentPerBooth = rentPerBooth,
                        TotalRent = totalRent,
                        SalesTotal = salesTotal,
                        CommissionPercent = comissionPercent,
                        CommissionAmount = comission,
                        FinalPayout = payout

                    });
                }
            }, "Generating settlement…");
        }
    }

    /// <summary>
    /// A simple settlement line used in the settlement list.
    /// Shows item info, price, commission and payout.
    /// </summary>
    //internal sealed class SettlementLineVm
    //{
    //    /// <summary>
    //    /// Name of the customer who sold the item.
    //    /// </summary>
    //    public string CustomerName { get; set; } = string.Empty;

    //    /// <summary>
    //    /// Name of the item sold.
    //    /// </summary>
    //    public string ItemName { get; set; } = string.Empty;

    //    /// <summary>
    //    /// Date the item was sold.
    //    /// </summary>
    //    public DateTime SaleDate { get; set; } = DateTime.MinValue;

    //    /// <summary>
    //    /// Sale price of the item.
    //    /// </summary>
    //    public decimal ItemPrice { get; set; }

    //    /// <summary>
    //    /// Percent of the sale taken as commission.
    //    /// </summary>
    //    public decimal CommissionPercent { get; set; } = 10m;

    //    /// <summary>
    //    /// Commission amount taken from the sale.
    //    /// </summary>
    //    public decimal CommissionAmount => Math.Round(ItemPrice * (CommissionPercent / 100m), 2);

    //    /// <summary>
    //    /// Money left for the customer after commission.
    //    /// </summary>
    //    public decimal AmountToPay => Math.Round(ItemPrice - CommissionAmount, 2);
    //}

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
