using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;
using ReolMarket.MVVM.Model.HelperModels;
using ReolMarket.MVVM.View;
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

        public ObservableCollection<Sale> Sales => _saleRepo.Items;
        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ICollectionView SaleView { get; }
        public Years Years { get; } = new Years();
        public IReadOnlyList<Month> Months { get; } =
            Enum.GetValues(typeof(Month)).Cast<Month>().ToList();

        private string? _itemName;
        public string? ItemName
        {
            get => _itemName;
            set
            {
                SetProperty(ref _itemName, value);
            }
        }

        private decimal? _itemPrice;
        public decimal? ItemPrice
        {
            get => _itemPrice;
            set
            {
                SetProperty(ref _itemPrice, value);
            }
        }

        private int? _boothNumber;
        public int? BoothNumber
        {
            get => _boothNumber;
            set
            {
                SetProperty(ref _boothNumber, value);
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

        private int _selectedYear;
        public int SelectedYear {
            get => _selectedYear;
            set {
                if (SetProperty(ref _selectedYear, value))
                    SaleView.Refresh();

            }
        }

        public ICommand RegisterSaleCommand { get; }

        public SaleViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Sale, Guid> saleRepo, IBaseRepository<Item, Guid> itemRepo)
        {
            _boothRepo = boothRepo;
            _saleRepo = saleRepo;
            _itemRepo = itemRepo;

            _selectedYear = DateTime.Now.Year;
            _selectedMonth = (Month)DateTime.Now.Month;

            RegisterSaleCommand = new RelayCommand(_ => RegisterSale(), _ => CanRegisterSale());
        }

        private void RegisterSale()
        {
            var booth = Booths.FirstOrDefault(b => b.BoothNumber == BoothNumber);

            var newItem = new Item
            {
                ItemName = ItemName,
                ItemPrice = (decimal)ItemPrice,
                BoothID = booth.BoothID,
                Booth = booth
            };
            _itemRepo.Add(newItem);

            var cart = new ShoppingCart();
            var cartItem = new ItemShoppingCart
            {
                ItemID = newItem.ItemID,
                ShoppingCartID = cart.ShoppingCartID
            };

            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                ShoppingCartID = cart.ShoppingCartID,
                PaymentID = Guid.NewGuid() //TODO Her skal laves en enum til kort og kontant
            };
            _saleRepo.Add(sale);

            ItemName = string.Empty;
            ItemPrice = 0;
            BoothNumber = null;
        }

        private bool CanRegisterSale()
        {
            return !string.IsNullOrWhiteSpace(ItemName)
                && ItemPrice > 0
                && BoothNumber.HasValue;
        }
    }
}
