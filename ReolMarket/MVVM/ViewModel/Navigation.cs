using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReolMarket.Core;

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// Genbrugelig, data-drevet navigation:
    /// - Tilføj sider med Add/AddRange (Id, Title, ViewModel)
    /// - Bind ListBox: ItemsSource="{Binding Nav.Items}" SelectedItem="{Binding Nav.Selected}"
    /// - Bind indhold: Content="{Binding Nav.CurrentView}" (eller forward i MainViewModel)
    /// - Brug NavigateCommand med CommandParameter=Id hvis du også har separate knapper/menuer.
    /// </summary>
    public sealed class Navigation : INotifyPropertyChanged
    {
        public sealed class NavItem
        {
            public string Id { get; set; } = "";    // f.eks. "Sale", "Renters", "Economy"
            public string Title { get; set; } = ""; // f.eks. "Salg", "Udlejere", "Økonomi"
        }

        private readonly Dictionary<string, BaseViewModel> _pageMap = new();
        public ObservableCollection<NavItem> Items { get; } = new();

        private NavItem? _selected;
        public NavItem? Selected
        {
            get => _selected;
            set
            {
                if (SetField(ref _selected, value) && value is not null)
                {
                    _currentId = value.Id;
                    _currentView = Resolve(_currentId);
                    OnPropertyChanged(nameof(CurrentId));
                    OnPropertyChanged(nameof(CurrentView));
                }
            }
        }

        private string _currentId = "";
        public string CurrentId
        {
            get => _currentId;
            private set => SetField(ref _currentId, value);
        }

        private BaseViewModel? _currentView;
        public BaseViewModel? CurrentView
        {
            get => _currentView;
            private set => SetField(ref _currentView, value);
        }

        public void Add(string id, string title, BaseViewModel vm)
        {
            Items.Add(new NavItem { Id = id, Title = title });
            _pageMap[id] = vm;
        }

        public void AddRange(params (string id, string title, BaseViewModel vm)[] pages)
        {
            foreach (var (id, title, vm) in pages)
                Add(id, title, vm);
        }

        public void Select(string id)
        {
            var item = FindById(id);
            if (item != null) Selected = item;
        }

        public void Initialize(string defaultId)
        {
            if (Items.Count == 0) return;
            Select(defaultId);
            if (Selected is null) Selected = Items[0];
        }

        public NavItem? FindById(string id)
        {
            foreach (var item in Items)
                if (item.Id == id) return item;
            return null;
        }

        public BaseViewModel? Resolve(string id)
            => _pageMap.TryGetValue(id, out var vm) ? vm : null;

        // ICommand til knapper/menupunkter (brug CommandParameter = "Id")
        public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand(param =>
        {
            if (param is string id) Select(id);
        });
        private ICommand? _navigateCommand;

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value!;
            OnPropertyChanged(name);
            return true;
        }
    }
}
