using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReolMarket.Core
{
    /// <summary>
    /// Base class for ViewModels.
    /// Provides: PropertyChanged, simple validation support,
    /// busy state handling, and a common async load pattern.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged, INotifyDataErrorInfo, IDisposable
    {
        private string _title = string.Empty;
        private bool _isBusy;
        private string? _busyText;
        private readonly Dictionary<string, List<string>> _errors = new();
        private CancellationTokenSource? _loadCts;

        /// <summary>
        /// Raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Title shown for this ViewModel (for windows or pages).
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// True while the ViewModel is doing work.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            protected set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// Optional text to show while busy (e.g. "Loading…").
        /// </summary>
        public string? BusyText
        {
            get => _busyText;
            protected set => SetProperty(ref _busyText, value);
        }

        /// <summary>
        /// Sets a backing field and raises <see cref="PropertyChanged"/> if the value changed.
        /// </summary>
        /// <typeparam name="T">Type of the field.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">New value.</param>
        /// <param name="propertyName">Name of the property (filled in by the compiler).</param>
        /// <returns>True if the value changed; otherwise false.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the given property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ------------ Async Load pattern ------------

        /// <summary>
        /// Cancels any ongoing load operation.
        /// </summary>
        protected void CancelLoad()
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;
        }

        /// <summary>
        /// Starts (or restarts) loading data for this ViewModel.
        /// Cancels a previous load, sets busy state, and calls <see cref="OnLoadAsync"/>.
        /// </summary>
        /// <param name="cancellationToken">External cancellation token.</param>
        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            CancelLoad();
            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var ct = _loadCts.Token;

            try
            {
                IsBusy = true;
                BusyText = BusyText ?? "Loading…";
                await OnLoadAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                IsBusy = false;
                BusyText = null;
            }
        }

        /// <summary>
        /// Override in derived classes to load data.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task that completes when load work is done.</returns>
        protected virtual Task OnLoadAsync(CancellationToken ct) => Task.CompletedTask;

        /// <summary>
        /// Runs an async action while setting busy state and optional text.
        /// Respects cancellation.
        /// </summary>
        /// <param name="action">Async action to run.</param>
        /// <param name="busyText">Text to show while running.</param>
        /// <param name="ct">External cancellation token.</param>
        /// <returns>A task that completes when the action finishes.</returns>
        protected async Task RunBusyAsync(Func<CancellationToken, Task> action, string? busyText = null, CancellationToken ct = default)
        {
            var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            try
            {
                IsBusy = true;
                if (!string.IsNullOrWhiteSpace(busyText))
                    BusyText = busyText;

                await action(linked.Token).ConfigureAwait(false);
            }
            finally
            {
                IsBusy = false;
                BusyText = null;
                linked.Dispose();
            }
        }

        /// <summary>
        /// Runs a sync action while setting busy state and optional text.
        /// Use this for synchronous repository calls.
        /// </summary>
        /// <param name="action">Action to run.</param>
        /// <param name="busyText">Text to show while running.</param>
        protected void RunBusy(Action action, string? busyText = null)
        {
            try
            {
                IsBusy = true;
                if (!string.IsNullOrWhiteSpace(busyText))
                    BusyText = busyText;

                action();
            }
            finally
            {
                IsBusy = false;
                BusyText = null;
            }
        }

        // ------------ Validation (INotifyDataErrorInfo) ------------

        /// <summary>
        /// True if this ViewModel currently has validation errors.
        /// </summary>
        public bool HasErrors => _errors.Count > 0;

        /// <summary>
        /// Raised when validation errors change for a property.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// Gets validation errors for a property.
        /// </summary>
        /// <param name="propertyName">Property name. Null or empty returns no errors.</param>
        /// <returns>A sequence of error strings.</returns>
        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return Array.Empty<string>();

            return _errors.TryGetValue(propertyName, out var list) ? (IEnumerable)list : Array.Empty<string>();
        }

        /// <summary>
        /// Adds a validation error for the given property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="error">Error message.</param>
        protected void AddError(string propertyName, string error)
        {
            if (!_errors.TryGetValue(propertyName, out var list))
            {
                list = new List<string>();
                _errors[propertyName] = list;
            }

            if (!list.Contains(error))
            {
                list.Add(error);
                RaiseErrorsChanged(propertyName);
            }
        }

        /// <summary>
        /// Clears all validation errors for the given property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
            {
                RaiseErrorsChanged(propertyName);
            }
        }

        /// <summary>
        /// Raises <see cref="ErrorsChanged"/> for a property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected void RaiseErrorsChanged(string propertyName) =>
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        /// <summary>
        /// Override to run validation for your ViewModel.
        /// Call this from property setters as values change.
        /// </summary>
        protected virtual void Validate() { }

        // ------------ Disposal ------------

        /// <summary>
        /// Disposes resources used by this ViewModel.
        /// Cancels any ongoing load and calls <see cref="OnDispose"/>.
        /// </summary>
        public void Dispose()
        {
            CancelLoad();
            OnDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Override for extra cleanup in derived classes.
        /// </summary>
        protected virtual void OnDispose() { }
    }
}
