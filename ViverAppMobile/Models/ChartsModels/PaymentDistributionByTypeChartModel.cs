using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;

namespace ViverAppMobile.Models.ChartsModels
{
    public class PaymentDistributionByTypeChartModel : ChartModelBase<PaymentHistoricDto>
    {
        public override void PopulateChart(IEnumerable<PaymentHistoricDto> data)
        {
            var groupedData = data
                .Where(p => p.Paidday.HasValue)
                .GroupBy(p => new { p.Paidday!.Value.Year, p.Paidday!.Value.Month })
                .Select(g => g
                    .GroupBy(x => x.Idpaymenttype)
                    .ToDictionary(x => (PayMethod)x.Key, x => x.Sum(v => (double)v.Paidprice!)))
                .ToList();

            double credit = groupedData.Sum(g => g.TryGetValue(PayMethod.CREDIT_CARD, out var v1) ? v1 : 0);
            double debit = groupedData.Sum(g => g.TryGetValue(PayMethod.DEBIT_CARD, out var v2) ? v2 : 0);
            double cardTotal = credit + debit;

            double pixTotal = groupedData.Sum(g => g.TryGetValue(PayMethod.PIX, out var v3) ? v3 : 0);
            double cashTotal = groupedData.Sum(g => g.TryGetValue(PayMethod.Cash, out var v4) ? v4 : 0);
            double bankSlipTotal = groupedData.Sum(g => g.TryGetValue(PayMethod.BankSlip, out var v5) ? v5 : 0);

            var seriesList = new[]
            {
                (label: "Cartão", value: cardTotal, color: SKColors.DodgerBlue),
                (label: "PIX", value: pixTotal, color: SKColors.MediumSeaGreen),
                (label: "Dinheiro", value: cashTotal, color: SKColors.Red),
                (label: "Boleto", value: bankSlipTotal, color: SKColors.Orange),
            };

            foreach (var (label, value, color) in seriesList)
            {
                Series.Add(new PieSeries<double>
                {
                    Name = label,
                    Values = [value],
                    Fill = new SolidColorPaint(color),
                    InnerRadius = 30,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{point.StackedValue.Share:P1}",
                    ToolTipLabelFormatter = point => $"{point.StackedValue.Share:P1}"
                });
            }
        }
    }
}
