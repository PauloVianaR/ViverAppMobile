using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.ViewModels
{
    public partial class SelectableModel<TModel>(TModel model, int modelId, int selectedId) : ObservableObject
    {
        public TModel Model { get; set; } = model;
        public Color BackgroundColor => SelectedId == modelId ? Color.FromArgb("#F0F6FF") : Colors.White;
        public Color BorderColor => SelectedId == modelId ? Color.FromArgb("#66a3ff") : Colors.LightGray;

        [ObservableProperty] private int selectedId = selectedId;

        partial void OnSelectedIdChanged(int value)
        {
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }
}
