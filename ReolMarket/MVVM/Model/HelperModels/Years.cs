namespace ReolMarket.MVVM.Model.HelperModels
{
    public sealed class Years
    {
        public IReadOnlyList<int> Items { get; }

        public Years()
        {
            int currentYear = DateTime.Now.Year;   // 2025 today
            int start = currentYear - 10;          // 2015
            int end = currentYear;

            Items = Enumerable.Range(start, end - start + 1)
                              .OrderByDescending(y => y) // newest first
                              .ToList();
        }
    }
}
