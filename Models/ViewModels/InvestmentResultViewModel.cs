using FinanceSystem.Models.Entities;
using FinanceSystem.Models.Services;

namespace FinanceSystem.Models.ViewModels
{
    /// <summary>
    /// Kết quả thẩm định dự án đầu tư — hiển thị NPV, IRR, Payback + khuyến nghị
    /// </summary>
    public class InvestmentResultViewModel
    {
        public InvestmentProject InputProject { get; set; } = new();

        // ── Chỉ số thẩm định ──
        public decimal NPV { get; set; }
        public decimal IRR { get; set; }
        public decimal PaybackPeriod { get; set; }          // Hoàn vốn thường (kỳ)
        public decimal DiscountedPaybackPeriod { get; set; } // Hoàn vốn có chiết khấu (kỳ)

        // ── Khuyến nghị ──
        public string Recommendation { get; set; } = string.Empty;

        // ── Hiển thị có định dạng ──
        public string NPVFormatted => FinancialHelper.FormatVND(NPV);
        public string IRRFormatted => FinancialHelper.FormatPercent(IRR);
        public string DiscountRateFormatted => FinancialHelper.FormatPercent(InputProject.DiscountRate);

        public string PaybackFormatted => PaybackPeriod < 0
            ? "Không hoàn vốn trong vòng đời dự án"
            : $"{PaybackPeriod:F1} kỳ";

        public string DiscountedPaybackFormatted => DiscountedPaybackPeriod < 0
            ? "Không hoàn vốn (có chiết khấu)"
            : $"{DiscountedPaybackPeriod:F1} kỳ";

        // ── Màu sắc cảnh báo (dùng với Bootstrap class) ──
        public string NPVBadgeClass => NPV >= 0 ? "badge bg-success" : "badge bg-danger";
        public string IRRBadgeClass => IRR >= InputProject.DiscountRate ? "badge bg-success" : "badge bg-warning text-dark";

        // ── Dữ liệu cho Chart.js (chuỗi CF để vẽ biểu đồ) ──
        public string CashFlowLabels =>
            string.Join(",", Enumerable.Range(1, InputProject.Periods).Select(t => $"\"Kỳ {t}\""));
        public string CashFlowData =>
            string.Join(",", InputProject.CashFlows.Select(cf => cf.ToString("F0")));
    }
}
