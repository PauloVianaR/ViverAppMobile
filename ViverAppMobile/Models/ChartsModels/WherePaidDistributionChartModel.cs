using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;

namespace ViverAppMobile.Models.ChartsModels
{
    public class WherePaidDistributionChartModel : ChartModelBase<PaymentHistoricDto>
    {
        public double Online { get; set; }
        public double Presential { get; set; }

        public override void PopulateChart(IEnumerable<PaymentHistoricDto> data)
        {
            Online = data.Count(a => a.Paidday.HasValue && a.Paidonline == 1);
            Presential = data.Count(a => a.Paidday.HasValue && a.Paidonline == 0);

            Series.Add(new PieSeries<double>
            {
                Name = "Online (App)",
                Values = [Online],
                Fill = new SolidColorPaint(SKColors.MediumSeaGreen, 2),
                InnerRadius = 30,
                DataLabelsPosition = PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{point.StackedValue.Share:P1}",
                ToolTipLabelFormatter = point => $"{point.StackedValue.Share:P1}"
            });
            Series.Add(new PieSeries<double>
            {
                Name = "Presencial",
                Values = [Presential],
                Fill = new SolidColorPaint(SKColors.DodgerBlue, 2),
                InnerRadius = 30,
                DataLabelsPosition = PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{point.StackedValue.Share:P1}",
                ToolTipLabelFormatter = point => $"{point.StackedValue.Share:P1}"
            });
        }
    }
}
