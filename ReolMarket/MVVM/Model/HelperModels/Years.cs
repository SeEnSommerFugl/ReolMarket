using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReolMarket.MVVM.Model.HelperModels
{
    public sealed class Years
    {
        public IReadOnlyList<int> Items { get; }

        public Years()
        {
            int currentYear = DateTime.Now.Year;   // 2025 today
            int start = currentYear - 10;          // 2015
            int end = currentYear + 10;          // 2035

            Items = Enumerable.Range(start, end - start + 1)
                              .OrderByDescending(y => y) // newest first
                              .ToList();
        }
    }
}
