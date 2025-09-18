using System.Windows.Input;
using ReolMarket.Core;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// The main ViewModel for the application.
    /// It controls navigation between Booths, Items, and Renters views.
    /// </summary>
    internal class MainViewModel : BaseViewModel
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

        private readonly BoothViewModel _boothsViewModel;
        private readonly ItemsViewModel _itemsViewModel;
        private readonly RentersViewModel _rentersViewModel;

        /// <summary>
        /// Creates the MainViewModel and sets up navigation.
        /// </summary>
        public MainViewModel()
        {
            Title = "ReolMarket";

            _boothsViewModel = new BoothViewModel();
            _itemsViewModel = new ItemsViewModel();
            _rentersViewModel = new RentersViewModel();

            NavigateBoothsCommand = new RelayCommand(_ => CurrentView = _boothsViewModel);
            NavigateItemsCommand = new RelayCommand(_ => CurrentView = _itemsViewModel);
            NavigateRentersCommand = new RelayCommand(_ => CurrentView = _rentersViewModel);

            CurrentView = _boothsViewModel; // Show Booths by default


        }
    }
}
