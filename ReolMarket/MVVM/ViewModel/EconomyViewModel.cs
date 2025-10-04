using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;
using ReolMarket.MVVM.Model.HelperModels;

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
        private readonly IBaseRepository<Item, Guid> _itemRepo;
        private readonly IBaseRepository<ItemShoppingCart, ItemShoppingCart.ItemShoppingCartKey> _itemCartRepo;
        private readonly IBaseRepository<ShoppingCart, Guid> _shoppingCartRepo;
        private readonly Economy _economy = new Economy();


        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<Customer> Customers => _customerRepo.Items;
        public ObservableCollection<Sale> Sales => _saleRepo.Items;
        public ObservableCollection<Item> Items => _itemRepo.Items;
        public ICollectionView BoothView { get; }
        public ICollectionView CustomerView { get; }
        public ICollectionView SalesView { get; }

        /// <summary>
        /// Creates the view model and sets default dates.
        /// </summary>
        public EconomyViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Customer, Guid> customerRepo, IBaseRepository<Sale, Guid> saleRepo, IBaseRepository<Item, Guid> itemRepo,
            IBaseRepository<ItemShoppingCart, ItemShoppingCart.ItemShoppingCartKey> itemCartRepo, IBaseRepository<ShoppingCart, Guid> shoppingCartRepo)
        {
            _boothRepo = boothRepo;
            _customerRepo = customerRepo;
            _saleRepo = saleRepo;
            _itemRepo = itemRepo;
            _itemCartRepo = itemCartRepo;
            _shoppingCartRepo = shoppingCartRepo;

            BoothView = CollectionViewSource.GetDefaultView(Booths);
            CustomerView = CollectionViewSource.GetDefaultView(Customers);
            SalesView = CollectionViewSource.GetDefaultView(Sales);


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
        private int _monthlyRentedBooths;
        public int MonthlyRentedBooths
        {
            get => MonthlyRentedBoothsCalulator();
            set
            {
                SetProperty(ref _monthlyRentedBooths, value);
            }
        }
        private int _monthlyAvailableBooths;
        public int MonthlyAvailableBooths
        {
            get => MonthlyAvailableBoothsCalculator();
            set
            {
                SetProperty(ref _monthlyAvailableBooths, value);
            }
        }
        private decimal _monthlyRentIncome;
        public decimal MonthlyRentIncome
        {
            get => CalculateRentalIncome();
            set
            {
                SetProperty(ref _monthlyRentIncome, value);
            }
        }

        private decimal _monthlyComission;
        public decimal MonthlyComission
        {
            get => CalculateMonthlyCommission();
            set
            {
                SetProperty(ref _monthlyComission, value);
            }
        }

        private decimal _monthlySalesTotal;
        public decimal MonthlySalesTotal
        {
            get => CalculateMonthlySalesTotal();
            set
            {
                SetProperty(ref _monthlySalesTotal, value);
            }
        }

        private int _monthlySalesAmount;
        public int MonthlySalesAmount
        {
            get => CalculateMonthlySalesAmount();
            set
            {
                SetProperty(ref _monthlySalesAmount, value);
            }
        }





        public decimal Rent => _economy.Rent;
        public decimal Electricity => _economy.Electricity;
        public decimal Water => _economy.Water;
        public decimal Heating => _economy.Heating;
        public decimal Internet => _economy.Internet;
        public decimal JonasFixedSalary => _economy.JonasFixedSalary;
        public decimal SofieFixedSalary => _economy.SofieFixedSalary;
        public decimal MetteSalary => _economy.MetteSalary;
        public decimal MonthlyExpenses =>
            Rent + Electricity + Water + Heating + Internet +
            JonasFixedSalary + SofieFixedSalary + MetteSalary;
        public decimal MonthlyIncome => TotalIncome();
        public decimal MonthlyDifference => MonthlyIncome - MonthlyExpenses;


        public decimal TotalIncome()
        {

            decimal totalIncome = 0;

            foreach (var sale in Sales)
            {
                totalIncome += sale.TotalPrice;
            }

            foreach (var customer in Customers)
            {
                var customerRentalIncome = CalculateCustomerRentalIncome(customer.CustomerID);
                totalIncome += customerRentalIncome;
            }

            return totalIncome;
        }
        private decimal CalculateRentalIncome()
        {
            decimal totalRentalIncome = 0;
            foreach (var customer in Customers)
            {
                var customerRentalIncome = CalculateCustomerRentalIncome(customer.CustomerID);
                totalRentalIncome += customerRentalIncome;
            }
            return totalRentalIncome;
        }

        private decimal CalculateCustomerRentalIncome(Guid customerId)
        {
            // Count how many booths this specific customer has
            var customerBoothCount = Booths.Count(booth =>
                booth.CustomerID.HasValue && booth.CustomerID.Value == customerId);

            if (customerBoothCount == 0) return 0;

            // Calculate rental income based on number of booths this customer has
            decimal baseRentPrice = 850m; // Standard monthly rent per booth

            // Apply pricing tiers based on how many booths this customer rents
            decimal pricePerBooth = customerBoothCount switch
            {
                1 => baseRentPrice,
                2 or 3 => baseRentPrice = 825m,
                >= 4 => baseRentPrice = 800m,
                _ => baseRentPrice
            };

            return customerBoothCount * pricePerBooth;
        }

        private int MonthlyRentedBoothsCalulator()
        {
            int rentedBooths = 0;
            foreach (var booth in Booths)
            {
                if (booth.IsRented && (booth.EndDate == null || booth.EndDate.Value.Month != DateTime.Now.Month))
                {
                    rentedBooths++;
                }
            }
            return rentedBooths;
        }

        private int MonthlyAvailableBoothsCalculator()
        {
            int availableBooths = 0;
            foreach (var booth in Booths)
            {
                if (!booth.IsRented || (booth.EndDate != null && booth.EndDate.Value.Month == DateTime.Now.Month))
                {
                    availableBooths++;
                }
            }
            return availableBooths;
        }

        private decimal CalculateMonthlyCommission()
        {
            decimal totalCommission = 0;
            foreach (var sale in Sales)
            {
                // Assuming a flat 10% commission rate for simplicity
                totalCommission += Math.Round(sale.TotalPrice * 0.10m, 2);
            }
            return totalCommission;
        }

        private decimal CalculateMonthlySalesTotal()
        {
            decimal totalSales = 0;
            foreach (var sale in Sales)
            {
                totalSales += sale.TotalPrice;
            }
            return totalSales;
        }

        private int CalculateMonthlySalesAmount()
        {
            return Sales.Count;
        }

        private ObservableCollection<CustomerOutstanding> _montlyOutstandingPayments;
        public ObservableCollection<CustomerOutstanding> MonthlyOutstandingPayments
        {
            get => _montlyOutstandingPayments ??= CalculateMonthlyOutstandingPayments();
            set
            {
                SetProperty(ref _montlyOutstandingPayments, value);
            }
        }

        //TODO Find der hvor CustomerID == Booth.CustomerID, og så find alle sales der har itemID som er i den booth, og så sum totalprice af dem.Minus derefter comission og månedlig leje (se.
        private ObservableCollection<CustomerOutstanding> CalculateMonthlyOutstandingPayments()
        {
            var result = (from customer in Customers
                          let customerBooths = Booths.Where(b => b.CustomerID == customer.CustomerID)
                          where customerBooths.Any()
                          let boothIds = customerBooths.Select(b => b.BoothID).ToHashSet()
                          let customerItemIds = Items.Where(i => boothIds.Contains(i.BoothID))
                                                     .Select(i => i.ItemID)
                                                     .ToHashSet()
                          where customerItemIds.Any()
                          let relevantItemCarts = _itemCartRepo.Items.Where(isc => customerItemIds.Contains(isc.ItemID))
                          let relevantCartIds = relevantItemCarts.Select(isc => isc.ShoppingCartID).Distinct().ToHashSet()
                          where relevantCartIds.Any()
                          let relevantSales = Sales.Where(s => relevantCartIds.Contains(s.ShoppingCartID)
                                                    && s.SaleDate.Month == DateTime.Now.Month
                                                    && s.SaleDate.Year == DateTime.Now.Year)
                          let totalSales = relevantSales.Sum(s => s.TotalPrice)
                          let commission = Math.Round(totalSales * 0.10m, 2)
                          let rent = CalculateCustomerRentalIncome(customer.CustomerID)
                          let outstanding = totalSales - commission - rent
                          select new CustomerOutstanding
                          {
                              CustomerName = customer.CustomerName,
                              SalesTotal = totalSales,
                              Commission = commission,
                              Rent = rent,
                              Outstanding = outstanding
                          }).ToList();
            return new ObservableCollection<CustomerOutstanding>(result);
        }

        //Her er den kedelige og læsevenlige version af CalculateMonthlyOutstandingPayments
        //private ObservableCollection<CustomerOutstanding> CalculateMonthlyOutstandingPayments() {
        //    var results = new ObservableCollection<CustomerOutstanding>();

        //    foreach (var customer in Customers) {
        //        var customerBooths = Booths.Where(b => b.CustomerID == customer.CustomerID).ToList();
        //        if (!customerBooths.Any()) continue;

        //        var boothIds = customerBooths.Select(b => b.BoothID).ToHashSet();
        //        var customerItems = Items.Where(i => boothIds.Contains(i.BoothID)).Select(i => i.ItemID).ToHashSet();

        //        var relevantItemCarts = _itemCartRepo.Items
        //            .Where(isc => customerItems.Contains(isc.ItemID))
        //            .ToList();
        //        if (!relevantItemCarts.Any()) continue;

        //        var relevantCartIds = relevantItemCarts.Select(isc => isc.ShoppingCartID).Distinct().ToHashSet();

        //        var relevantSales = Sales.Where(s => relevantCartIds.Contains(s.ShoppingCartID) && s.SaleDate.Month == DateTime.Now.Month && s.SaleDate.Year == DateTime.Now.Year).ToList();

        //        var totalSales = relevantSales.Sum(s => s.TotalPrice);
        //        var commission = Math.Round(totalSales * 0.10m, 2);
        //        var rent = CalculateCustomerRentalIncome(customer.CustomerID);
        //        var outstanding = totalSales - commission - rent;

        //        results.Add(new CustomerOutstanding {
        //            CustomerName = customer.CustomerName,
        //            SalesTotal = totalSales,
        //            Commission = commission,
        //            Rent = rent,
        //            Outstanding = outstanding
        //        });
        //    }

        //    return results;
        //}
    }
}
