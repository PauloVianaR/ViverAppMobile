using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;

namespace ViverAppMobile.Models.ChartsModels
{
    public class RevenueByUserTypeChartModel : ChartModelBase<PaymentHistoricDto>
    {
        public string Month { get; set; } = string.Empty;
        public double Regular { get; set; }
        public double Premium { get; set; }

        public override void PopulateChart(IEnumerable<PaymentHistoricDto> data)
        {
            var chartData = data
                .Where(p => p.Paidday.HasValue)
                .GroupBy(p => new { p.Paidday!.Value.Year, p.Paidday!.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new RevenueByUserTypeChartModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1)
                            .ToString("MMM/yy", CultureInfo.CurrentCulture),
                    Regular = g.Where(p => p.IsUserPremium == 0).Sum(p => (double)p.Paidprice!),
                    Premium = g.Where(p => p.IsUserPremium == 1).Sum(p => (double)p.Paidprice!)
                });

            Series.Clear();
            Series.Add(new StackedAreaSeries<double>
            {
                Name = "Premium",
                Values = chartData.Select(x => x.Premium).ToArray(),
            });
            Series.Add(new StackedAreaSeries<double>
            {
                Name = "Regular",
                Values = chartData.Select(x => x.Regular).ToArray()
            });

            XAxes.Clear();
            XAxes.Add(new Axis
            {
                Labels = chartData.Select(x => x.Month).ToArray()
            });

            YAxes.Add(new Axis
            {
                Labeler = value => "R$ " + value / 1000 + "k"
            });
        }
    }
}
