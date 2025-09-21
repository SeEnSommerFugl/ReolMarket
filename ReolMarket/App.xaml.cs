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
            // Add others as needed:
            // IBaseRepository<Item, Guid> itemRepo = new ItemDbRepository();

            // ---- ViewModels (pass interfaces in) ----
            var boothsVM = new BoothViewModel(boothRepo, customerRepo);
            var itemsVM = new ItemsViewModel(/* e.g., itemRepo */);
            var rentersVM = new RentersViewModel(/*customerRepo /* etc. */);

            var mainVM = new MainViewModel(boothsVM, itemsVM, rentersVM);

            // ---- Shell window ----
            var mainWindow = new MainWindow(mainVM);
            mainWindow.Show();
        }

    }
}
