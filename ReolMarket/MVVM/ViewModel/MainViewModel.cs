using System.Windows.Input;
using ReolMarket.Core;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// The main ViewModel for the application.
    /// It controls navigation between Booths, Items, and Renters views.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel? _currentView;

        /// <summary>
        /// The view that is currently shown in the UI.
        /// </summary>
        public BaseViewModel? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        /// <summary>
        /// Command to switch to the Booths view.
        /// </summary>
        public ICommand NavigateBoothsCommand { get; }

        /// <summary>
        /// Command to switch to the Items view.
        /// </summary>
        public ICommand NavigateItemsCommand { get; }

        /// <summary>
        /// Command to switch to the Renters view.
        /// </summary>
        public ICommand NavigateRentersCommand { get; }

        public ICommand NavigateEconomyCommand { get; }

        private readonly ItemsViewModel _itemsViewModel;
        private readonly RentersViewModel _rentersViewModel;
        private readonly EconomyViewModel _economyViewModel;

        public ICommand NavigateSaleCommand { get; }

        private readonly SaleViewModel _saleViewModel;


        /// <summary>
        /// Creates the MainViewModel and sets up navigation.
        /// </summary>
        internal MainViewModel(ItemsViewModel items, RentersViewModel renters, EconomyViewModel economy, SaleViewModel sales)
        {
            Title = "ReolMarket";

            _itemsViewModel = items;
            _rentersViewModel = renters;
            _economyViewModel = economy;
            _saleViewModel = sales;


            NavigateRentersCommand = new RelayCommand(_ => CurrentView = _rentersViewModel);
            NavigateItemsCommand = new RelayCommand(_ => CurrentView = _itemsViewModel);
            NavigateEconomyCommand = new RelayCommand(_ => CurrentView = _economyViewModel);
            NavigateSaleCommand = new RelayCommand(_ => CurrentView = _saleViewModel);

            CurrentView = _currentView; // Show Booths by default


        }
    }
}
