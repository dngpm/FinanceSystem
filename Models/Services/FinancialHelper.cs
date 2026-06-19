using FinanceSystem.Models.Entities;

namespace FinanceSystem.Models.Services
{
    /// <summary>
    /// Tiện ích dùng chung: format tiền tệ, phần trăm, làm tròn chuẩn
    /// Tránh sai số floating-point khi hiển thị số liệu tài chính
    /// </summary>
    public static class FinancialHelper
    {
        private static readonly System.Globalization.CultureInfo ViCulture
            = new("vi-VN");

        /// <summary>Format số tiền VND: 1,500,000 ₫</summary>
        public static string FormatVND(decimal amount)
            => string.Format(ViCulture, "{0:N0} ₫", amount);

        /// <summary>Format phần trăm: 0.0855 → "8.55%"</summary>
        public static string FormatPercent(decimal rate, int decimals = 2)
            => (rate * 100).ToString($"F{decimals}") + "%";

        /// <summary>Làm tròn tiền tệ về đơn vị đồng (0 chữ số thập phân)</summary>
        public static decimal RoundMoney(decimal value)
            => Math.Round(value, 0, MidpointRounding.AwayFromZero);

        /// <summary>Làm tròn lãi suất (giữ 6 chữ số thập phân)</summary>
        public static decimal RoundRate(decimal rate)
            => Math.Round(rate, 6);

        /// <summary>
        /// Kiểm tra hợp lệ số tiền vay (không âm, không quá lớn)
        /// </summary>
        public static bool IsValidPrincipal(decimal principal)
            => principal > 0 && principal <= 100_000_000_000m; // Tối đa 100 tỷ

        /// <summary>
        /// Kiểm tra lãi suất hợp lệ (0% đến 100%/năm)
        /// </summary>
        public static bool IsValidRate(decimal annualRate)
            => annualRate > 0 && annualRate <= 1.0m;

        /// <summary>
        /// Kiểm tra kỳ hạn hợp lệ (1 tháng đến 360 tháng / 30 năm)
        /// </summary>
        public static bool IsValidTerm(int months)
            => months >= 1 && months <= 360;
        /// <summary>
        /// Validate số kỳ theo đơn vị kỳ hạn.
        ///   Monthly:   1-360  |  Quarterly: 1-120  |  Yearly: 1-30
        /// </summary>
        public static bool IsValidTermCount(int termCount, TermUnit unit)
            => unit switch
            {
                TermUnit.Monthly => termCount >= 1 && termCount <= 360,
                TermUnit.Quarterly => termCount >= 1 && termCount <= 120,
                TermUnit.Yearly => termCount >= 1 && termCount <= 30,
                _ => false
            };

        /// <summary>Thông báo lỗi kỳ hạn phù hợp với TermUnit.</summary>
        public static string GetTermErrorMessage(TermUnit unit)
            => unit switch
            {
                TermUnit.Monthly => "Kỳ hạn tháng phải từ 1 đến 360 tháng.",
                TermUnit.Quarterly => "Kỳ hạn quý phải từ 1 đến 120 quý.",
                TermUnit.Yearly => "Kỳ hạn năm phải từ 1 đến 30 năm.",
                _ => "Kỳ hạn không hợp lệ."
            };

    }
}
