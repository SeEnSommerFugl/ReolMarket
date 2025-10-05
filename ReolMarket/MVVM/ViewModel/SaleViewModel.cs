using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;
using ReolMarket.MVVM.Model.HelperModels;
using ReolMarket.MVVM.View;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel for SaleView: shows sale
    /// </summary>
    internal class SaleViewModel : BaseViewModel
    {
        private readonly IBaseRepository<Sale, Guid> _saleRepo;
        private readonly IBaseRepository<Booth, Guid> _boothRepo;
        private readonly IBaseRepository<Item, Guid> _itemRepo;
        private readonly IBaseRepository<ShoppingCart, Guid> _shoppingCartRepo;
        private readonly IBaseRepository<ItemShoppingCart, ItemShoppingCart.ItemShoppingCartKey> _itemShoppingCartRepo;
        private readonly IBaseRepository<Payment, Guid> _paymentRepo;
      

        public ObservableCollection<Sale> Sales => _saleRepo.Items;
        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ObservableCollection<Payment> PaymentMethod => _paymentRepo.Items;
        public ICollectionView SaleView { get; set; }
        public Years Years { get; } = new Years();
        public IReadOnlyList<Month> Months { get; } =
            Enum.GetValues(typeof(Month)).Cast<Month>().ToList();


        public SaleViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Sale, Guid> saleRepo, IBaseRepository<Item, Guid> itemRepo,
            IBaseRepository<ShoppingCart, Guid> shoppingCartRepo, IBaseRepository<ItemShoppingCart, ItemShoppingCart.ItemShoppingCartKey> itemShoppingCartRepo, IBaseRepository<Payment, Guid> paymentRepo)
        {
            _boothRepo = boothRepo;
            _saleRepo = saleRepo;
            _itemRepo = itemRepo;
            _shoppingCartRepo = shoppingCartRepo;
            _itemShoppingCartRepo = itemShoppingCartRepo;
            _paymentRepo = paymentRepo;

          
            _selectedYear = DateTime.Now.Year;
            _selectedMonth = (Month)DateTime.Now.Month;

            RegisterSaleCommand = new RelayCommand(_ => RegisterSale(), _ => CanRegisterSale());
            SaleView = CollectionViewSource.GetDefaultView(Sales);
        }


        private string? _itemName;
        public string? ItemName
        {
            get => _itemName;
            set
            {
                if(SetProperty(ref _itemName, value))
                    ((RelayCommand)RegisterSaleCommand).RaiseCanExecuteChanged();
            }
        }

        private decimal? _itemPrice;
        public decimal? ItemPrice
        {
            get => _itemPrice;
            set
            {
                if(SetProperty(ref _itemPrice, value))
                    ((RelayCommand)RegisterSaleCommand).RaiseCanExecuteChanged();
            }
        }

        private int? _boothNumber;
        public int? BoothNumber
        {
            get => _boothNumber;
            set
            {
                if(SetProperty(ref _boothNumber, value))
                    ((RelayCommand)RegisterSaleCommand).RaiseCanExecuteChanged();
            }
        }

        private int? _quantity;
        public int? Quantity {
            get => _quantity;
            set {
                if(SetProperty(ref _quantity, value))
                    ((RelayCommand)RegisterSaleCommand).RaiseCanExecuteChanged();
            }
        }

        private Payment? _selectedPayment;
        public Payment? SelectedPayment 
            {
            get => _selectedPayment;
            set 
            {
                if(SetProperty(ref _selectedPayment, value))
                    ((RelayCommand)RegisterSaleCommand).RaiseCanExecuteChanged();
            }
        }

        
        private Month _selectedMonth;
        public Month SelectedMonth {
            get => _selectedMonth;
            set {
                if (SetProperty(ref _selectedMonth, value))
                    SaleView.Refresh();

            }
        }

        private int _selectedYear = DateTime.Now.Year;
        public int SelectedYear {
            get => _selectedYear;
            set {
                if (SetProperty(ref _selectedYear, value))
                    SaleView.Refresh();

            }
        }

        public ICommand RegisterSaleCommand { get; }

        

        private void RegisterSale()
        {
            var booth = Booths.FirstOrDefault(b => b.BoothNumber == BoothNumber);

            var newItem = new Item
            {
                ItemID = Guid.NewGuid(),
                ItemName = ItemName,
                ItemPrice = (decimal)ItemPrice,
                BoothID = booth.BoothID,
                Booth = booth
            };
            _itemRepo.Add(newItem);

            var cart = new ShoppingCart 
            {
                ShoppingCartID = Guid.NewGuid()
            };
            _shoppingCartRepo.Add(cart);

            var cartItem = new ItemShoppingCart
            {
                ItemID = newItem.ItemID,
                ShoppingCartID = cart.ShoppingCartID,
                Quantity = Quantity ?? 1,
                UnitPrice = newItem.ItemPrice
            };
            _itemShoppingCartRepo.Add(cartItem);

            var sale = new Sale
            {
                SaleID = Guid.NewGuid(),
                SaleDate = DateTime.Now,
                ShoppingCartID = cart.ShoppingCartID,
                PaymentID = SelectedPayment.PaymentID,
                TotalPrice = cartItem.Quantity * cartItem.UnitPrice
            };
            _saleRepo.Add(sale);

            ItemName = string.Empty;
            ItemPrice = null;
            Quantity = null;
            BoothNumber = null;
        }

        private bool CanRegisterSale()
        {
            return !string.IsNullOrWhiteSpace(ItemName)
                && ItemPrice > 0
                && Quantity > 0
                && BoothNumber.HasValue
                && SelectedPayment != null;
        }
    }
}
