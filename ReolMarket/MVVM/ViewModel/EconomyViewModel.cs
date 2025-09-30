using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;
using ReolMarket.MVVM.Model.HelperModels;
using ReolMarket.MVVM.View;

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

        private readonly DispatcherTimer _refreshDebounce = new() { Interval = TimeSpan.FromMilliseconds(200) };

        private SearchModeItem? _selectedSearchMode;

        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<Customer> Customers => _customerRepo.Items;
        public ObservableCollection<Sale> Sales => _saleRepo.Items;
        public ICollectionView EconomyBoard { get; }
        public ICollectionView BoothView { get; }
        public ICollectionView CustomerView { get; }
        public ICollectionView SalesView { get; }
        public CollectionViewGroup test { get; }
        public ICollectionView QuickRangesComboBox { get; }
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
        public SearchModeItem? SelectedSearchMode
        {
            get => _selectedSearchMode;
            set
            {
                if (SetProperty(ref _selectedSearchMode, value))
                    EconomyBoard?.Refresh();

            }
        }
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    RequestRefresh(); // ✅ Instant feedback
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

            BoothView = CollectionViewSource.GetDefaultView(Booths);
            CustomerView = CollectionViewSource.GetDefaultView(Customers);
            SalesView = CollectionViewSource.GetDefaultView(Sales);




            _refreshDebounce.Tick += (_, __) => { _refreshDebounce.Stop(); EconomyBoard.Refresh(); };

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

        private bool FilterCustomer(object obj)
        {
            if (obj is not Customer customer)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText) || SelectedSearchMode == null)
                return true;

            var s = SearchText.Trim();

            return SelectedSearchMode.SearchMode switch
            {
                SearchMode.All =>
                    customer.CustomerName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    customer.PhoneNumber.Contains(s) ||
                    customer.Email.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    customer.Address.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    customer.PostalCode.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    Booths.Any(b => b.CustomerID == customer.CustomerID &&
                                    b.BoothNumber.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)),

                SearchMode.BoothNumber =>
                    Booths.Any(b => b.CustomerID == customer.CustomerID &&
                                    b.BoothNumber.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)),

                SearchMode.CustomerName =>
                    customer.CustomerName.Contains(s, StringComparison.OrdinalIgnoreCase),

                SearchMode.CustomerPhone =>
                    customer.PhoneNumber.Contains(s),

                SearchMode.CustomerEmail =>
                    customer.Email.Contains(s, StringComparison.OrdinalIgnoreCase),

                _ => true
            };
        }

        private void RequestRefresh()
        {
            _refreshDebounce.Stop();
            _refreshDebounce.Start();
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
