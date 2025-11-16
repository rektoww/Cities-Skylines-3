using Core.GameEngine;
using System.Text;

namespace Infrastructure.Services
{
    public class FinanceReportService
    {
        private readonly GameEngine _engine;

        public FinanceReportService(GameEngine engine)
        {
            _engine = engine;
        }

        public FinanceReport GetFinanceReport()
        {
            return new FinanceReport
            {
                CityBudget = _engine.FinancialSystem?.CityBudget ?? 0m,
                BuildingCount = _engine.Map?.Buildings?.Count ?? 0,
                RoadSegmentsCount = _engine.Map?.RoadSegments?.Count ?? 0,
                CitizenCount = _engine.PopulationService?.CitizenCount ?? 0,
                TotalIncome = _engine.FinancialSystem?.GetFinancialReport()?.TotalIncome ?? 0m,
                TotalExpenses = _engine.FinancialSystem?.GetFinancialReport()?.TotalExpenses ?? 0m
            };
        }

        public string GetFormattedFinanceInfo()
        {
            var report = GetFinanceReport();
            var sb = new StringBuilder();
            sb.AppendLine($"Бюджет города: {report.CityBudget:C}");
            sb.AppendLine($"Зданий: {report.BuildingCount}");
            sb.AppendLine($"Дорог: {report.RoadSegmentsCount}");
            sb.AppendLine($"Жителей: {report.CitizenCount}");
            sb.AppendLine($"Доходы: {report.TotalIncome:C}");
            sb.AppendLine($"Расходы: {report.TotalExpenses:C}");
            sb.AppendLine($"Баланс: {report.PeriodBalance:C}");

            return sb.ToString();
        }
    }

    public class FinanceReport
    {
        public decimal CityBudget { get; set; }
        public int BuildingCount { get; set; }
        public int RoadSegmentsCount { get; set; }
        public int CitizenCount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal PeriodBalance => TotalIncome - TotalExpenses;
    }
}
