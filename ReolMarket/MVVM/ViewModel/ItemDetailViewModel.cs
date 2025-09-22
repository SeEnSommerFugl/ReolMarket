using System.Collections.ObjectModel;
using System.Windows.Input;
using ReolMarket.Core;
using ReolMarket.Data.Repository;     // ItemDbRepository, BoothDbRepository
using ReolMarket.MVVM.Model;          // Item, Booth

namespace ReolMarket.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel for creating or editing one item.
    /// Uses synchronous repositories and current MVVM models.
    /// </summary>
    internal class ItemDetailViewModel : BaseViewModel
    {
        // Repositories (sync)
        private readonly ItemDbRepository _itemRepo;
        private readonly BoothDbRepository _boothRepo;

        // Backing fields
        private Guid? _itemId;
        private string? _itemName;
        private decimal _itemPrice;
        private Guid? _boothId;

        /// <summary>
        /// All available booths to pick from.
        /// Bind this to a ComboBox.
        /// </summary>
        public ObservableCollection<Booth> Booths { get; } = new();

        /// <summary>
        /// Gets or sets the item name.
        /// This field is required.
        /// </summary>
        public string? ItemName
        {
            get => _itemName;
            set { if (SetProperty(ref _itemName, value)) Validate(); }
        }

        /// <summary>
        /// Gets or sets the item price.
        /// Must be greater than or equal to zero.
        /// </summary>
        public decimal ItemPrice
        {
            get => _itemPrice;
            set { if (SetProperty(ref _itemPrice, value)) Validate(); }
        }

        /// <summary>
        /// Gets or sets the booth id that owns the item.
        /// This field is required.
        /// </summary>
        public Guid? BoothID
        {
            get => _boothId;
            set { if (SetProperty(ref _boothId, value)) Validate(); }
        }

        /// <summary>
        /// Command that saves the item (create or update).
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Command that cancels editing and closes the view.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Raised when the view should close.
        /// True means "saved", false means "cancelled".
        /// </summary>
        public event Action<bool>? RequestClose;

        /// <summary>
        /// Creates the view model and wires up commands.
        /// </summary>
        public ItemDetailViewModel()
        {
            Title = "Item details";
            _itemRepo = new ItemDbRepository();
            _boothRepo = new BoothDbRepository();

            SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
        }

        /// <summary>
        /// Loads an existing item for edit, or sets defaults for a new item.
        /// </summary>
        /// <param name="itemId">The id of the item to edit. Pass null to create a new item.</param>
        public void Load(Guid? itemId = null)
        {
            RunBusy(() =>
            {
                // Load booths for selection
                Booths.Clear();
                foreach (var booth in _boothRepo.GetAll().OrderBy(b => b.BoothNumber))
                    Booths.Add(booth);

                _itemId = itemId;

                if (itemId.HasValue)
                {
                    var item = _itemRepo.GetById(itemId.Value);
                    if (item != null)
                    {
                        ItemName = item.ItemName;
                        ItemPrice = item.ItemPrice;
                        BoothID = item.BoothID;
                    }
                }
                else
                {
                    // Defaults for new item
                    ItemName = string.Empty;
                    ItemPrice = 0m;
                    BoothID = Booths.FirstOrDefault()?.BoothID;
                }

                Validate();
            }, "Loading item…");
        }

        /// <summary>
        /// Validates all input fields and updates error state.
        /// </summary>
        protected override void Validate()
        {
            ClearErrors(nameof(ItemName));
            if (string.IsNullOrWhiteSpace(ItemName))
                AddError(nameof(ItemName), "Item name is required.");

            ClearErrors(nameof(ItemPrice));
            if (ItemPrice < 0)
                AddError(nameof(ItemPrice), "Price cannot be negative.");

            ClearErrors(nameof(BoothID));
            if (!BoothID.HasValue)
                AddError(nameof(BoothID), "Please select a booth.");

            (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Returns true if the item can be saved.
        /// </summary>
        private bool CanSave() => !IsBusy && !HasErrors;

        /// <summary>
        /// Saves the item using the repository.
        /// Creates a new item or updates the existing one.
        /// </summary>
        private void ExecuteSave()
        {
            if (!CanSave()) return;

            RunBusy(() =>
            {
                if (_itemId.HasValue)
                {
                    // Update existing item
                    var item = _itemRepo.GetById(_itemId.Value);
                    if (item != null)
                    {
                        item.ItemName = ItemName!.Trim();
                        item.ItemPrice = ItemPrice;
                        item.BoothID = BoothID!.Value;
                        _itemRepo.Update(item);
                    }
                }
                else
                {
                    // Insert new item
                    var newItem = new Item
                    {
                        ItemID = Guid.NewGuid(),
                        ItemName = ItemName!.Trim(),
                        ItemPrice = ItemPrice,
                        BoothID = BoothID!.Value
                    };
                    _itemRepo.Add(newItem);
                }

                RequestClose?.Invoke(true);
            }, "Saving item…");
        }
    }
}
