using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
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
        private readonly IBaseRepository<Item, Guid> _itemRepo;

        public ObservableCollection<Sale> Sales => _saleRepo.Items;
        public ObservableCollection<Booth> Booths => _boothRepo.Items;
        public ICollectionView SaleView {  get; }

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

        public ICommand RegisterSaleCommand { get; }

        public SaleViewModel(IBaseRepository<Booth, Guid> boothRepo, IBaseRepository<Sale, Guid> saleRepo, IBaseRepository<Item, Guid> itemRepo)
        {
            _boothRepo = boothRepo;
            _saleRepo = saleRepo;
            _itemRepo = itemRepo;

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
                ShoppingCartID = cart.ShoppingCartId
            };

            var sale = new Sale {
                SaleDate = DateTime.Now,
                ShoppingCartID = cart.ShoppingCartId,
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
