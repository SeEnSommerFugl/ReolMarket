using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReolMarket.MVVM.Model
{
    class Sale
    {
        public Guid SaleID { get; set; } = Guid.NewGuid();
        public Guid ShoppingCartID { get; set; }
        public Guid PaymentID { get; set; }
    }
}
