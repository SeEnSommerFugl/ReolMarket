using System.Windows;
using ReolMarket.MVVM.ViewModel;

namespace ReolMarket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BoothsViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new BoothsViewModel();
            DataContext = viewModel;
        }
    }
}