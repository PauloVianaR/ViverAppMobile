using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverAppMobile.Helpers;

namespace ViverAppMobile.Models.ChartsModels
{
    public class AppointmentTypeDistributionChartModel : ChartModelBase<ScheduleDto>
    {
        public string AppointmentTypeDescription { get; set; } = string.Empty;
        public int Count { get; set; }

        public override void PopulateChart(IEnumerable<ScheduleDto> data)
        {
            var groupedData = data
                .Where(s => s.AppointmentType > 0)
                .GroupBy(s => new { s.AppointmentType })
                .Select(g => new AppointmentTypeDistributionChartModel
                {
                    AppointmentTypeDescription = EnumTranslator.TranslateAppointmentType((Models.AppointmentType)g.Key.AppointmentType)!,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            foreach (var item in groupedData)
            {
                Series.Add(new PieSeries<int>
                {
                    Name = item.AppointmentTypeDescription,
                    Values = [item.Count],
                    InnerRadius = 60,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{point.StackedValue.Share:P1}",
                    ToolTipLabelFormatter = point => $"{point.StackedValue.Share:P1} ({item.Count})"
                });
            }
        }
    }
}
