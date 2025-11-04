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
using ViverApp.Shared.Models;

namespace ViverAppMobile.Models.ChartsModels
{
    public class WherePaidTrendEvolutionChartModel : ChartModelBase<PaymentHistoricDto>
    {
        public string Month { get; set; } = string.Empty;
        public double Online { get; set; }
        public double Presential { get; set; }

        public override void PopulateChart(IEnumerable<PaymentHistoricDto> data)
        {
            var chartData = data
                .Where(a => a.Paidday.HasValue)
                .GroupBy(a => new { a.Paidday!.Value.Year, a.Paidday!.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new WherePaidTrendEvolutionChartModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1)
                            .ToString("MMM", CultureInfo.CurrentCulture),
                    Online = g.Count(x => x.Paidonline == 1),
                    Presential = g.Count(x => x.Paidonline == 0)
                })
                .ToList();

            Series.Add(new LineSeries<double>
            {
                Name = "Online (App)",
                Values = chartData.Select(x => x.Online).ToArray(),
                GeometrySize = 10,
                Stroke = new SolidColorPaint(SKColors.MediumSeaGreen, 2),
                Fill = null
            });

            Series.Add(new LineSeries<double>
            {
                Name = "Presencial",
                Values = chartData.Select(x => x.Presential).ToArray(),
                GeometrySize = 10,
                Stroke = new SolidColorPaint(SKColors.DodgerBlue, 2),
                Fill = null
            });

            XAxes.Add(new Axis
            {
                Labels = chartData.Select(x => x.Month).ToArray()
            });

            YAxes.Add(new Axis
            {
                Labeler = value => value.ToString("N0")
            });
        }
    }
}
