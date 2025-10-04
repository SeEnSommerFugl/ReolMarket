using ReolMarket.MVVM.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReolMarket.MVVM.Model.HelperModels
{
    internal class CustomerOutstanding
    {
        public string CustomerName { get; set; } = "";
        public decimal SalesTotal { get; set; }
        public decimal Commission { get; set; }
        public decimal Rent { get; set; }
        public decimal Outstanding { get; set; }
    }
}
