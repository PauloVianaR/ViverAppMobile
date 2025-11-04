using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;

namespace ViverAppMobile.Models.ChartsModels
{
    public class DoctorPerformanceChartModel : ChartModelBase<ScheduleDto>
    {
        public string DoctorName { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int AppointmentsCount { get; set; }
        public double Efficiency { get; set; }

        public override void PopulateChart(IEnumerable<ScheduleDto> data)
        {
            var groupedData = data
                .Where(d => !string.IsNullOrEmpty(d.DoctorName))
                .GroupBy(d => new {d.Iddoctor, d.DoctorName})
                .Select(g => new DoctorPerformanceChartModel
                {
                    DoctorName = g.Key.DoctorName!,
                    Rating = g.Average(x => x.Rating ?? 0),
                    AppointmentsCount = g.Count(),
                    Efficiency = (g.Average(x => x.Rating ?? 1) / 5.0) * 100
                })
                .OrderByDescending(x => x.DoctorName)
                .ToList();

            Series.Add(new LineSeries<double>
            {
                Name = "Avaliação Média",
                Values = groupedData.Select(x => x.Rating).ToArray(),
                GeometrySize = 10,
                Stroke = new SolidColorPaint(SKColors.Orange, 2),
                Fill = null,
                GeometryStroke = new SolidColorPaint(SKColors.Orange, 2),
                GeometryFill = new SolidColorPaint(SKColors.Orange),
                ScalesYAt = 1,
                YToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue:N2}"
            });

            Series.Add(new ColumnSeries<int>
            {
                Name = "Atendimentos",
                Values = groupedData.Select(x => x.AppointmentsCount).ToArray(),
                Fill = new SolidColorPaint(SKColors.DodgerBlue)
            });

            Series.Add(new ColumnSeries<double>
            {
                Name = "Eficiência %",
                Values = groupedData.Select(x => x.Efficiency).ToArray(),
                Fill = new SolidColorPaint(SKColors.MediumSeaGreen)
            });

            XAxes.Add(new Axis
            {
                Labels = groupedData.Select(x => x.DoctorName).ToArray(),
                LabelsRotation = 30,
                TextSize = 12,
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                Padding = new LiveChartsCore.Drawing.Padding(0),
                MinStep = 1
            });

            YAxes.Add(new Axis
            {
                Labeler = value => value.ToString("N0"),
                MinLimit = 0,
                MaxLimit = 100
            });

            YAxes.Add(new Axis
            {
                Position = LiveChartsCore.Measure.AxisPosition.End,
                Labeler = value => value.ToString("N0"),
                MinLimit = 1,
                MaxLimit = 5
            });
        }
    }
}
