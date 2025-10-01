using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReolMarket.Data;

namespace ReolMarket.MVVM.Model.HelperModels
{
    public class SalesRowService
    {
        private readonly IBaseRepository<Customer, Guid> _cust;
        private readonly IBaseRepository<Booth, Guid> _booth;
        private readonly IBaseRepository<Item, Guid> _item;
        private readonly IBaseRepository<ItemShoppingCart, Guid> _itemShoppingCart;
        private readonly IBaseRepository<ShoppingCart, Guid> _shoppingCart;
        private readonly IBaseRepository<Sale, Guid> _sale;

        public SalesRowService(
            IBaseRepository<Customer, Guid> customer,
            IBaseRepository<Booth, Guid> booth,
            IBaseRepository<Item, Guid> item,
            IBaseRepository<ItemShoppingCart, Guid> itemShoppingCart,
            IBaseRepository<ShoppingCart, Guid> shoppingCart,
            IBaseRepository<Sale, Guid> sale)
        {
            _cust = customer;
            _booth = booth;
            _item = item;
            _itemShoppingCart = itemShoppingCart;
            _shoppingCart = shoppingCart;
            _sale = sale;
        }

        public IEnumerable<SalesRow> GetSalesRows()
        {
            return
                from b in _booth.Items
                join c in _cust.Items on b.CustomerID equals c.CustomerID
                join i in _item.Items on b.BoothID equals i.BoothID into boothItems
                from i in boothItems.DefaultIfEmpty()
                join ic in _itemShoppingCart.Items on i?.ItemID equals ic.ItemID into itemCarts
                from ic in itemCarts.DefaultIfEmpty()
                join sc in _shoppingCart.Items on ic?.ShoppingCartID equals sc.ShoppingCartID into carts
                from sc in carts.DefaultIfEmpty()
                join s in _sale.Items on sc?.ShoppingCartID equals s.ShoppingCartID into sales
                from s in sales.DefaultIfEmpty()
                select new SalesRow
                {
                    CustomerID = c.CustomerID,
                    CustomerName = c.CustomerName,
                    BoothID = b.BoothID,
                    BoothNumber = b.BoothNumber,
                    ItemID = i?.ItemID,
                    ItemName = i?.ItemName,
                    ItemPrice = i?.ItemPrice,
                    ShoppingCartID = sc?.ShoppingCartID,
                    SaleID = s?.SaleID,
                    SaleDate = s?.SaleDate
                };
        }
    }
}
