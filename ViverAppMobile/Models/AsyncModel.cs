using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Models
{
    public partial class AsyncModel<TModel>(TModel model, bool canSetFalseActiveModelChanged = false) : ObservableObject
    {
        private readonly bool canSetFalseActiveIfModelChanged = canSetFalseActiveModelChanged;

        [ObservableProperty] private TModel model = model;
        [ObservableProperty] private bool isBusy = false;
        [ObservableProperty] private bool isActive = false;

        public event EventHandler<AsyncModelEventArgs<bool>>? OnIsActiveChangedEvent;
        public event EventHandler<AsyncModelEventArgs<TModel>>? OnModelChangedEvent;

        public async Task ExecuteAsync(Func<TModel, Task> action)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await action(Model);
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            OnIsActiveChangedEvent?.Invoke(this, new AsyncModelEventArgs<bool>(oldValue, newValue));
        }

        partial void OnModelChanged(TModel? oldValue, TModel newValue)
        {
            if (oldValue is null)
                return;

            OnModelChangedEvent?.Invoke(this, new AsyncModelEventArgs<TModel>(oldValue, newValue));

            if (canSetFalseActiveIfModelChanged)
                this.IsActive = false;
        }
    }

    public class AsyncModelEventArgs<T>(T oldValue, T newValue) : EventArgs
    {
        public T? OldValue { get; set; } = oldValue;
        public T? NewValue { get; set; } = newValue;
    }
}
