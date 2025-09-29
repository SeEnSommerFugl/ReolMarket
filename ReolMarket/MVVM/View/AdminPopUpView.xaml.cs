using System.Windows;
using System.Windows.Controls;
using ReolMarket.MVVM.ViewModel;

namespace ReolMarket.MVVM.View
{
    /// <summary>
    /// Interaction logic for AdminPopUpView.xaml
    /// </summary>
    public partial class AdminPopUpView : Window
    {
        private AdminPopUpViewModel ViewModel => (AdminPopUpViewModel)DataContext;

        public AdminPopUpView()
        {
            InitializeComponent();
        }
        private void BoothCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is Guid boothId)
            {
                // Ask HashSet if this booth is selected
                checkBox.IsChecked = ViewModel?.IsBoothSelected(boothId) ?? false;
            }
        }

        /// <summary>
        /// When user checks checkbox, add to HashSet
        /// </summary>
        private void BoothCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is Guid boothId)
            {
                // Add to HashSet = super fast operation
                ViewModel?.SetBoothSelection(boothId, true);
            }
        }

        /// <summary>
        /// When user unchecks checkbox, remove from HashSet
        /// </summary>
        private void BoothCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is Guid boothId)
            {
                // Remove from HashSet = super fast operation
                ViewModel?.SetBoothSelection(boothId, false);
            }
        }

        private void BoothCheckBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                // Clear state when container is being recycled
                checkBox.IsChecked = false;
            }
        }
    }

}
