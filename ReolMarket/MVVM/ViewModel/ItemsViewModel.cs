using System.Collections.ObjectModel;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.Data;
using ReolMarket.MVVM.Model;          // Item, Booth

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel that lists and manages items.
    /// Uses synchronous repositories and current MVVM models.
    /// </summary>
    internal class ItemsViewModel : BaseViewModel
    {
        // Repositories (sync)
        private readonly IBaseRepository<Item, Guid> _itemRepo;
        private readonly IBaseRepository<Booth, Guid> _boothRepo;

        // Backing fields
        private Item? _selectedItem;
        private string? _searchText;
        private Guid? _boothFilterId;

        // Cache
        private Item[] _allItems = Array.Empty<Item>();

        /// <summary>
        /// Items shown in the UI after filtering.
        /// </summary>
        public ObservableCollection<Item> Items { get; } = new();

        /// <summary>
        /// All booths, used for a filter dropdown and lookups.
        /// </summary>
        public ObservableCollection<Booth> Booths { get; } = new();

        /// <summary>
        /// The item currently selected in the list.
        /// </summary>
        public Item? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                    RefreshCommands();
            }
        }

        /// <summary>
        /// Free-text search. Matches item name and booth number.
        /// </summary>
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilters();
            }
        }

        /// <summary>
        /// Optional booth filter. When set, only items from this booth are shown.
        /// </summary>
        public Guid? BoothFilterId
        {
            get => _boothFilterId;
            set
            {
                if (SetProperty(ref _boothFilterId, value))
                    ApplyFilters();
            }
        }

        /// <summary>
        /// Reloads data from repositories.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Adds a new item with simple defaults.
        /// </summary>
        public ICommand AddItemCommand { get; }

        /// <summary>
        /// Saves a small change to the selected item.
        /// </summary>
        public ICommand EditItemCommand { get; }

        /// <summary>
        /// Deletes the selected item.
        /// </summary>
        public ICommand DeleteItemCommand { get; }

        /// <summary>
        /// Creates the view model and loads initial data.
        /// </summary>
        public ItemsViewModel(IBaseRepository<Item, Guid> itemRepo, IBaseRepository<Booth, Guid> boothRepo)
        {
            Title = "Items";

            _itemRepo = itemRepo;
            _boothRepo = boothRepo;

            RefreshCommand = new RelayCommand(_ => Load());
            AddItemCommand = new RelayCommand(_ => AddItem(), _ => !IsBusy);
            EditItemCommand = new RelayCommand(_ => EditItem(), _ => !IsBusy && SelectedItem != null);
            DeleteItemCommand = new RelayCommand(_ => DeleteItem(), _ => !IsBusy && SelectedItem != null);

            Load();
        }

        /// <summary>
        /// Loads booths and items, then applies current filters.
        /// </summary>
        private void Load()
        {
            RunBusy(() =>
            {
                // Booths to support filter and booth number lookups
                Booths.Clear();
                foreach (var b in _boothRepo.GetAll().OrderBy(b => b.BoothNumber))
                    Booths.Add(b);

                _allItems = _itemRepo.GetAll().ToArray();
                ApplyFilters();
            }, "Loading items…");
        }

        /// <summary>
        /// Applies the text and booth filters to the cached list
        /// and updates the UI collection.
        /// </summary>
        private void ApplyFilters()
        {
            var query = _allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim();

                // Lookup booth number via repository for search convenience
                query = query.Where(i =>
                    (!string.IsNullOrWhiteSpace(i.ItemName) &&
                     i.ItemName.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (_boothRepo.GetById(i.BoothID)?.BoothNumber
                        .ToString()
                        .Contains(s, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (BoothFilterId.HasValue)
                query = query.Where(i => i.BoothID == BoothFilterId.Value);

            var result = query
                .OrderBy(i => _boothRepo.GetById(i.BoothID)?.BoothNumber ?? int.MaxValue)
                .ThenBy(i => i.ItemName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Items.Clear();
            foreach (var it in result)
                Items.Add(it);
        }

        /// <summary>
        /// Adds a simple item for the first booth and saves it.
        /// </summary>
        private void AddItem()
        {
            RunBusy(() =>
            {
                var booth = Booths.FirstOrDefault();
                if (booth == null) return;

                var item = new Item
                {
                    ItemID = Guid.NewGuid(),
                    ItemName = "New item",
                    ItemPrice = 10m,
                    BoothID = booth.BoothID
                };
                _itemRepo.Add(item);
                Load();
            }, "Adding item…");
        }

        /// <summary>
        /// Makes a small visible change to the selected item and saves it.
        /// </summary>
        private void EditItem()
        {
            if (SelectedItem == null) return;

            RunBusy(() =>
            {
                SelectedItem.ItemName = (SelectedItem.ItemName ?? string.Empty).Trim() + " *";
                _itemRepo.Update(SelectedItem);
                Load();
            }, "Saving item…");
        }

        /// <summary>
        /// Deletes the selected item and reloads the list.
        /// </summary>
        private void DeleteItem()
        {
            if (SelectedItem == null) return;

            RunBusy(() =>
            {
                _itemRepo.Delete(SelectedItem.ItemID);
                Load();
            }, "Deleting item…");
        }

        /// <summary>
        /// Updates command states after selection changes.
        /// </summary>
        private void RefreshCommands()
        {
            (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (EditItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeleteItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
