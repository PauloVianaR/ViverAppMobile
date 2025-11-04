using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;

namespace ViverAppMobile.Models.ChartsModels
{
    public abstract class ChartModelBase<TDto>
    {
        public ObservableCollection<ISeries> Series { get; set; } = [];
        public ObservableCollection<Axis> XAxes { get; set; } = [];
        public ObservableCollection<Axis> YAxes { get; set; } = [];

        public abstract void PopulateChart(IEnumerable<TDto> data);
        
        public void ClearChart()
        {
            Series.Clear();
            XAxes.Clear();
            YAxes.Clear();
        }
    }
}
