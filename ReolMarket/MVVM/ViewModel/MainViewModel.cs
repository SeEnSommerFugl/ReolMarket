using System.ComponentModel;
using System.Windows.Input;
using ReolMarket.Core;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// The main ViewModel for the application.
    /// It controls navigation between Booths, Items, and Renters views.
    /// </summary>
    public class MainViewModel : BaseViewModel, INotifyPropertyChanged, INotifyDataErrorInfo, IDisposable
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

        private readonly RentersViewModelOLD _boothsViewModel;
        private readonly ItemsViewModel _itemsViewModel;
        private readonly RentersViewModel _rentersViewModel;

        /// <summary>
        /// Creates the MainViewModel and sets up navigation.
        /// </summary>
        internal MainViewModel(RentersViewModelOLD booths, ItemsViewModel items, RentersViewModel renters)
        {
            Title = "ReolMarket";

            _boothsViewModel = booths;
            _itemsViewModel = items;
            _rentersViewModel = renters;

            NavigateRentersCommand = new RelayCommand(_ => CurrentView = _rentersViewModel);
            NavigateBoothsCommand = new RelayCommand(_ => CurrentView = _boothsViewModel);
            NavigateItemsCommand = new RelayCommand(_ => CurrentView = _itemsViewModel);

            CurrentView = _currentView; // Show Booths by default


        }
    }
}
