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
using ViverAppMobile.Helpers;

namespace ViverAppMobile.Models.ChartsModels
{
    public class PaymentsByTypeChartModel : ChartModelBase<PaymentHistoricDto>
    {
        public override void PopulateChart(IEnumerable<PaymentHistoricDto> data)
        {
            var groupedData = data
                .Where(p => p.Paidday.HasValue)
                .GroupBy(p => new { p.Paidday!.Value.Year, p.Paidday!.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1)
                            .ToString("MMM", CultureInfo.CurrentCulture),
                    Payments = g.GroupBy(x => x.Idpaymenttype)
                                .ToDictionary(x => (PayMethod)x.Key, x => x.Sum(v => (double)v.Paidprice!))
                })
                .ToList();

            var labels = groupedData.Select(x => x.Month).ToArray();

            XAxes.Add(new Axis
            {
                Labels = labels,
                LabelsRotation = 0
            });

            YAxes.Add(new Axis
            {
                Labeler = value => "R$ " + value/1000 + " k",
            });

            var paymentTypes = new[]
            {
                PayMethod.Card,
                PayMethod.PIX,
                PayMethod.Cash,
                PayMethod.BankSlip
            };

            var colors = new Dictionary<PayMethod, SKColor>
            {
                [PayMethod.Card] = SKColors.DodgerBlue,
                [PayMethod.Cash] = SKColors.Red,
                [PayMethod.PIX] = SKColors.MediumSeaGreen,
                [PayMethod.BankSlip] = SKColors.Orange
            };

            foreach (var type in paymentTypes)
            {
                double[] values;

                if (type == PayMethod.Card)
                {
                    values = groupedData.Select(x =>
                    {
                        var credit = x.Payments.TryGetValue(PayMethod.CREDIT_CARD, out double v1) ? v1 : 0;
                        var debit = x.Payments.TryGetValue(PayMethod.DEBIT_CARD, out double v2) ? v2 : 0;
                        return credit + debit;
                    }).ToArray();
                }
                else
                {
                    values = groupedData.Select(x =>
                        x.Payments.TryGetValue(type, out double value) ? value : 0).ToArray();
                }

                Series.Add(new ColumnSeries<double>
                {
                    Name = EnumTranslator.TranslatePaymentType(type),
                    Values = values,
                    Fill = new SolidColorPaint(colors[type])
                });
            }

        }
    }
}
