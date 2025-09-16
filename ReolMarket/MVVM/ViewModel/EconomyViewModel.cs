using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ReolMarket.Core;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel for showing settlements per customer.
    /// For MVP only. No external services or sales data yet.
    /// </summary>
    internal class EconomyViewModel : BaseViewModel
    {
        private DateTime _periodStart = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _periodEnd = DateTime.Today;

        /// <summary>
        /// Start date for the settlement period.
        /// </summary>
        public DateTime PeriodStart
        {
            get => _periodStart;
            set => SetProperty(ref _periodStart, value);
        }

        /// <summary>
        /// End date for the settlement period.
        /// </summary>
        public DateTime PeriodEnd
        {
            get => _periodEnd;
            set => SetProperty(ref _periodEnd, value);
        }

        /// <summary>
        /// The settlement results. One row per item sold.
        /// </summary>
        public ObservableCollection<SettlementLineVm> SettlementLines { get; } = new();

        /// <summary>
        /// Command that generates the settlement list.
        /// </summary>
        public ICommand GenerateCommand { get; }

        /// <summary>
        /// Creates the view model and sets default dates.
        /// </summary>
        public EconomyViewModel()
        {
            Title = "Monthly Settlement";
            GenerateCommand = new RelayCommand(_ => ExecuteGenerate(), _ => !IsBusy);
        }

        /// <summary>
        /// Generates settlement data. (Clears list for now.)
        /// </summary>
        private void ExecuteGenerate()
        {
            RunBusy(() =>
            {
                SettlementLines.Clear();
                // Future: calculate lines based on sales data.
            }, "Generating settlement…");
        }
    }

    /// <summary>
    /// A simple settlement line used in the settlement list.
    /// Shows item info, price, commission and payout.
    /// </summary>
    internal sealed class SettlementLineVm
    {
        /// <summary>
        /// Name of the customer who sold the item.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the item sold.
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Date the item was sold.
        /// </summary>
        public DateTime SaleDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Sale price of the item.
        /// </summary>
        public decimal ItemPrice { get; set; }

        /// <summary>
        /// Percent of the sale taken as commission.
        /// </summary>
        public decimal CommissionPercent { get; set; } = 10m;

        /// <summary>
        /// Commission amount taken from the sale.
        /// </summary>
        public decimal CommissionAmount => Math.Round(ItemPrice * (CommissionPercent / 100m), 2);

        /// <summary>
        /// Money left for the customer after commission.
        /// </summary>
        public decimal AmountToPay => Math.Round(ItemPrice - CommissionAmount, 2);
    }
}
