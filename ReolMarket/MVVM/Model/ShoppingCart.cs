using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReolMarket.MVVM.Model
{
    class ShoppingCart
    {
        public Guid ShoppingCartId { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
