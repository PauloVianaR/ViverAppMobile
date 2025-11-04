using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Models.ChartsModels;
using ViverAppMobile.Services;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Admin
{
    public partial class AdminAnalyticsViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly CancellationToken token = Master.GlobalToken;
        private readonly PaymentService paymentService;
        private readonly ScheduleService scheduleService;
        private bool isLoading = false;

        [ObservableProperty] private int selectedTab = 0;
        [ObservableProperty] private string selectedTimeFilter = string.Empty;
        [ObservableProperty] private decimal revenue = 0m;
        [ObservableProperty] private double revenuePercent = 0;
        [ObservableProperty] private int appointmentsCount = 0;
        [ObservableProperty] private double appointmentsPercent = 0;
        [ObservableProperty] private decimal averagePrice = 0m;
        [ObservableProperty] private double averagePricePercent = 0;
        [ObservableProperty] private string satisfactionString = "0.0";
        [ObservableProperty] private double satisfaction = 0;
        [ObservableProperty] private double satisfactionPercent = 0;
        [ObservableProperty] private RevenueByUserTypeChartModel revenueByUserTypeChart = new();
        [ObservableProperty] private RevenueVsAppointmentChartModel revenueVsAppointmentChart = new();
        [ObservableProperty] private PaymentsByTypeChartModel paymentsByTypeChart = new();
        [ObservableProperty] private PaymentDistributionByTypeChartModel paymentsDistributionChart = new();
        [ObservableProperty] private WherePaidTrendEvolutionChartModel wherePaidTrendEvolutionChart = new();
        [ObservableProperty] private WherePaidDistributionChartModel wherePaidDistributionChart = new();
        [ObservableProperty] private AppointmentDistributionChartModel appointmentDistributionChart = new();
        [ObservableProperty] private AppointmentTypeDistributionChartModel appointmentTypeDistributionChart = new();
        [ObservableProperty] private DoctorPerformanceChartModel doctorPerformanceChart = new();

        public ObservableCollection<string> TimeFilter { get; set; } = ["Último mês", "3 Meses", "6 Meses", "1 Ano"];
        public AdminAnalyticsViewModel()
        {
            paymentService = new();
            scheduleService = new();
            SelectedTimeFilter = TimeFilter[2];
        }

        public async Task InitializeAsync()
        {
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            if (isLoading)
                return null;
            isLoading = true;

            try
            {
                int months = this.GetMonthsCountByFilter();

                var paymentResp = await paymentService.GetPaymentsByMonths(months: months);
                if (!paymentResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    paymentResp.ThrowIfIsNotSucess();
                }

                var payments = paymentResp?.Response ?? [];
                RevenueByUserTypeChart.PopulateChart(payments);
                RevenueVsAppointmentChart.PopulateChart(payments);
                PaymentsByTypeChart.PopulateChart(payments);
                PaymentsDistributionChart.PopulateChart(payments);
                WherePaidTrendEvolutionChart.PopulateChart(payments);
                WherePaidDistributionChart.PopulateChart(payments);

                await Task.Delay(500);

                var scheduleHistoricResp = await scheduleService.GetScheduleByMonthsAsync(id: 0, isDoctor: default, isHistoric: true, months: months);
                if (!scheduleHistoricResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    scheduleHistoricResp.ThrowIfIsNotSucess();
                }

                var scheduleHistoric = scheduleHistoricResp?.Response ?? [];
                AppointmentDistributionChart.PopulateChart(scheduleHistoric);
                AppointmentTypeDistributionChart.PopulateChart(scheduleHistoric);
                DoctorPerformanceChart.PopulateChart(scheduleHistoric);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Revenue = payments.Sum(p => p.Paidprice ?? 0);
                    AveragePrice = Math.Round(payments.Average(p => p.Paidprice ?? 0), 2);
                    AppointmentsCount = scheduleHistoric.Count();
                    Satisfaction = Math.Round(scheduleHistoric.Average(s => s.Rating ?? 0), 2);
                    SatisfactionString = Satisfaction.ToString("N2");
                });

                var paymentsLastResp = await paymentService.GetPaymentsByMonths(months: months, isMonthsBefore: true);
                if (!paymentsLastResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    paymentsLastResp.ThrowIfIsNotSucess();
                }

                var paymentsLast = paymentsLastResp?.Response ?? [];
                if (paymentsLast.Any())
                {
                    decimal newRevenue = payments.Sum(p => p.Paidprice ?? 0);
                    decimal oldRevenue = paymentsLast.Sum(p => p.Paidprice ?? 0);
                    await MainThread.InvokeOnMainThreadAsync(() => RevenuePercent = CalculateGrowthPercentage(newRevenue, oldRevenue));

                    decimal newAveragePrice = payments.Average(p => p.Paidprice ?? 0);
                    decimal oldAveragePrice = paymentsLast.Average(p => p.Paidprice ?? 0);
                    await MainThread.InvokeOnMainThreadAsync(() => AveragePricePercent = CalculateGrowthPercentage(newAveragePrice, oldAveragePrice));
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        RevenuePercent = 100;
                        AveragePricePercent = 100;
                    });
                }

                var scheduleLastHistoricResp = await scheduleService.GetScheduleByMonthsAsync(id: 0, isDoctor: default, isHistoric: true, months: months, isMonthsBefore: true);
                if (!scheduleLastHistoricResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    scheduleLastHistoricResp.ThrowIfIsNotSucess();
                }

                var scheduleLastHistoric = scheduleLastHistoricResp.Response ?? [];
                if(scheduleLastHistoric.Any())
                {
                    int newCount = scheduleHistoric.Count();
                    int oldCount = scheduleLastHistoric.Count();
                    await MainThread.InvokeOnMainThreadAsync(() => AppointmentsPercent = CalculateGrowthPercentage(newCount, oldCount));

                    decimal newSatisfaction = (decimal)scheduleHistoric.Average(s => s.Rating ?? 1);
                    decimal oldSatisfaction = (decimal)scheduleLastHistoric.Average(s => s.Rating ?? 1);
                    await MainThread.InvokeOnMainThreadAsync(() => SatisfactionPercent = CalculateGrowthPercentage(newSatisfaction, oldSatisfaction));
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        AppointmentsPercent = 100;
                        SatisfactionPercent = 100;
                    });
                }

                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }

        [RelayCommand] private void ToogleSelectedTab(int tab) => SelectedTab = tab;

        partial void OnSelectedTimeFilterChanged(string value)
        {
            if (isLoading || string.IsNullOrWhiteSpace(value))
                return;

            RevenueByUserTypeChart.ClearChart();
            RevenueVsAppointmentChart.ClearChart();
            PaymentsByTypeChart.ClearChart();
            PaymentsDistributionChart.ClearChart();
            WherePaidTrendEvolutionChart.ClearChart();
            WherePaidDistributionChart.ClearChart();
            AppointmentDistributionChart.ClearChart();
            AppointmentTypeDistributionChart.ClearChart();
            DoctorPerformanceChart.ClearChart();

            _ = Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        private int GetMonthsCountByFilter()
        {
            int index = TimeFilter.IndexOf(SelectedTimeFilter);

            return index switch
            {
                0 => 1,
                1 => 3,
                2 => 6,
                4 => 12,
                _ => 6
            };
        }

        private static double CalculateGrowthPercentage(decimal newValue, decimal oldValue)
        {
            if (oldValue <= 0)
                oldValue = 1;

            return Math.Round((double)((newValue - oldValue) / oldValue) * 100, 2);
        }
    }
}
