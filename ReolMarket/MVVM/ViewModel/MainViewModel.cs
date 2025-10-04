using ReolMarket.Core;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// The main ViewModel for the application.
    /// Uses a data-driven Navigation helper and exposes it to the View.
    /// Bind in XAML:
    ///   - List:    ItemsSource = {Binding Nav.Items}
    ///              SelectedItem = {Binding Nav.Selected}
    ///   - Content: Content     = {Binding Nav.CurrentView}
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        public Navigation Nav { get; } = new();

        private readonly RentersViewModel _rentersViewModel;
        private readonly EconomyViewModel _economyViewModel;
        private readonly SaleViewModel _saleViewModel;

        internal MainViewModel(RentersViewModel renters, EconomyViewModel economy, SaleViewModel sales)
        {
            Title = "ReolMarket";

            _rentersViewModel = renters;
            _economyViewModel = economy;
            _saleViewModel    = sales;

            // Registrér sider (én linje pr. side). Tilføj nye her.
            // 1st: Function - 2nd: Text on button 3 - 3rd: Binding to
            Nav.AddRange(
                ("Sale",    "Salg",     _saleViewModel),
                ("Renters", "Udlejere", _rentersViewModel),
                ("Economy", "Økonomi",  _economyViewModel)
            );

            // Vælg standard (prøver "Sale", ellers første item)
            Nav.Initialize(defaultId: "Sale");
        }
    }
}
