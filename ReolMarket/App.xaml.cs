using System.Windows;
using ReolMarket.Data;
using ReolMarket.Data.Repository;
using ReolMarket.MVVM.Model;
using ReolMarket.MVVM.ViewModel;

namespace ReolMarket
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ---- Singletons (shared across app) ----
            IBaseRepository<Booth, Guid> boothRepo = new BoothDbRepository();
            IBaseRepository<Customer, Guid> customerRepo = new CustomerDbRepository();
            IBaseRepository<Sale, Guid> saleRepo = new SaleDbRepository();
            IBaseRepository<Item, Guid> itemRepo = new ItemDbRepository();

            // ---- ViewModels (pass interfaces in) ----

            var itemsVM = new ItemsViewModel(itemRepo, boothRepo);
            var rentersVM = new RentersViewModel(boothRepo, customerRepo);
            var economyVM = new EconomyViewModel(boothRepo, customerRepo, saleRepo, itemRepo);

            var mainVM = new MainViewModel(itemsVM, rentersVM, economyVM);

            // ---- Shell window ----
            var mainWindow = new MainWindow(mainVM);
            mainWindow.Show();
        }

    }
}
