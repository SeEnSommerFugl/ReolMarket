using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReolMarket.MVVM.Model
{
    class Payment
    {
        public Guid PaymentID { get; set; } = Guid.NewGuid();
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
