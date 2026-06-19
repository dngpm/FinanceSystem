using FinanceSystem.Models.Entities;
using FinanceSystem.Models.Services;

namespace FinanceSystem.Models.ViewModels
{
    /// <summary>
    /// Tổng hợp dòng tiền cho Dashboard — hiển thị 3 dòng tiền + cảnh báo thâm hụt
    /// </summary>
    public class CashFlowSummaryViewModel
    {
        // ── Dữ liệu tổng hợp từng kỳ ──
        public List<PeriodCashFlowSummary> PeriodSummaries { get; set; } = new();

        // ── Tổng toàn bộ kỳ ──
        public CashFlowTotals Totals { get; set; } = new();

        // ── Cảnh báo ──
        public List<int> DeficitPeriods { get; set; } = new();

        // Nhận warnings từ GenerateWarnings()
        public List<string> Warnings { get; set; } = new();

        public decimal InitialInvestment { get; set; }

        public decimal DiscountRate { get; set; }

        public decimal PresentValue { get; set; }

        public decimal FutureValue { get; set; }

        public decimal NPV { get; set; }

        public decimal IRR { get; set; }

        public string Decision { get; set; } = string.Empty;

        public bool HasDeficit => DeficitPeriods.Any();

        public string DeficitWarning => HasDeficit
            ? $"⚠️ Cảnh báo: Dòng tiền âm tại kỳ {string.Join(", ", DeficitPeriods)}. Nguy cơ kiệt quệ tài chính!"
            : "✅ Dòng tiền ổn định — không phát hiện thâm hụt.";

        public string WarningBadgeClass => HasDeficit ? "alert alert-danger" : "alert alert-success";

        // ── Dữ liệu JSON cho Chart.js (biểu đồ cột chồng 3 dòng tiền) ──
        public string ChartLabels =>
            string.Join(",", PeriodSummaries.Select(s => $"\"Kỳ {s.Period}\""));
        public string OperatingData =>
            string.Join(",", PeriodSummaries.Select(s => s.OperatingCF.ToString("F0")));
        public string InvestingData =>
            string.Join(",", PeriodSummaries.Select(s => s.InvestingCF.ToString("F0")));
        public string FinancingData =>
            string.Join(",", PeriodSummaries.Select(s => s.FinancingCF.ToString("F0")));
        public string CumulativeData =>
            string.Join(",", PeriodSummaries.Select(s => s.CumulativeBalance.ToString("F0")));

        // ── Tổng định dạng ──
        public string TotalOperatingFormatted => FinancialHelper.FormatVND(Totals.TotalOperating);
        public string TotalInvestingFormatted => FinancialHelper.FormatVND(Totals.TotalInvesting);
        public string TotalFinancingFormatted => FinancialHelper.FormatVND(Totals.TotalFinancing);
        public string NetTotalFormatted => FinancialHelper.FormatVND(Totals.NetTotal);
        public string InitialInvestmentFormatted => FinancialHelper.FormatVND(InitialInvestment);
        public string DiscountRateFormatted => FinancialHelper.FormatPercent(DiscountRate);
        public string PresentValueFormatted => FinancialHelper.FormatVND(PresentValue);
        public string FutureValueFormatted => FinancialHelper.FormatVND(FutureValue);
        public string NPVFormatted => FinancialHelper.FormatVND(NPV);
        public string IRRFormatted => FinancialHelper.FormatPercent(IRR);
    }
}
