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
    public class RevenueVsAppointmentChartModel : ChartModelBase<PaymentHistoricDto>
    {
        public string Month { get; set; } = string.Empty;
        public double Revenue { get; set; }
        public double Appointments { get; set; }

        public override void PopulateChart(IEnumerable<PaymentHistoricDto> data)
        {
            var chartData = data
                .Where(p => p.Paidday.HasValue)
                .GroupBy(p => new { p.Paidday!.Value.Year, p.Paidday!.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new RevenueVsAppointmentChartModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1)
                            .ToString("MMM/yy", CultureInfo.CurrentCulture),
                    Revenue = g.Sum(p => (double)p.Paidprice!),
                    Appointments = g.Count()
                });

            Series.Add(new ColumnSeries<double>
            {
                Name = "Receita (R$)",
                Values = chartData.Select(x => x.Revenue).ToArray(),
            });

            Series.Add(new LineSeries<double>
            {
                Name = "Serviços",
                Values = chartData.Select(x => x.Appointments).ToArray(),
                GeometrySize = 10,
                Stroke = new SolidColorPaint(SKColors.Red, 2),
                Fill = null,
                ScalesYAt = 1
            });

            XAxes.Add(new Axis
            {
                Labels = chartData.Select(x => x.Month).ToArray()
            });

            YAxes.Add(new Axis
            {
                Labeler = value => "R$ " + value / 1000 + "k"
            });

            YAxes.Add(new Axis
            {
                Position = LiveChartsCore.Measure.AxisPosition.End
            });
        }
    }
}
