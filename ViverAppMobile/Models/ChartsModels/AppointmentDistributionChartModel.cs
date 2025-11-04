using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using ViverApp.Shared.DTos;

namespace ViverAppMobile.Models.ChartsModels
{
    public class AppointmentDistributionChartModel : ChartModelBase<ScheduleDto>
    {
        public string AppointmentTitle { get; set; } = string.Empty;
        public int Count { get; set; }

        public override void PopulateChart(IEnumerable<ScheduleDto> data)
        {
            var groupedData = data
                .Where(s => !string.IsNullOrEmpty(s.AppointmentTitle))
                .GroupBy(s => new { s.IdAppointment, s.AppointmentTitle })
                .Select(g => new AppointmentDistributionChartModel
                {
                    AppointmentTitle = g.Key.AppointmentTitle!,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            foreach (var item in groupedData)
            {
                Series.Add(new PieSeries<int>
                {
                    Name = item.AppointmentTitle,
                    Values = [item.Count],
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{point.StackedValue.Share:P1}",
                    ToolTipLabelFormatter = point => $"{point.StackedValue.Share:P1} ({item.Count})"
                });
            }
        }
    }
}
