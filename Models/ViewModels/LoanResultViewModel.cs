using FinanceSystem.Models.Entities;
using FinanceSystem.Models.Services;

namespace FinanceSystem.Models.ViewModels
{
    /// <summary>
    /// Kết quả tính toán khoản vay — Controller truyền sang View để hiển thị
    /// </summary>
    public class LoanResultViewModel
    {
        // ── Thông tin đầu vào (giữ lại để hiển thị lại trên form) ──
        public LoanInfo InputLoan { get; set; } = new();

        // ── Bảng lịch trả nợ ──
        public List<AmortizationRow> Schedule { get; set; } = new();

        // ── Tổng kết ──
        public decimal TotalPrincipal => InputLoan.Principal;
        public decimal TotalInterest => Schedule.Sum(r => r.InterestPayment);
        public decimal TotalPenalty => Schedule.Sum(r => r.EarlyRepaymentPenalty);
        public decimal TotalPayment => TotalPrincipal + TotalInterest + TotalPenalty + InputLoan.ServiceFee;

        // ── Lãi suất thực tế ──
        public decimal EIR { get; set; }

        // ── Hiển thị có định dạng (dùng trong Razor View) ──
        public string TotalPrincipalFormatted => FinancialHelper.FormatVND(TotalPrincipal);
        public string TotalInterestFormatted => FinancialHelper.FormatVND(TotalInterest);
        public string TotalPaymentFormatted => FinancialHelper.FormatVND(TotalPayment);
        public string EIRFormatted => FinancialHelper.FormatPercent(EIR);

        // ── Thông tin kỳ hạn ────────────────────────────────────────────

        /// <summary>Nhãn đơn vị: "tháng" / "quý" / "năm"</summary>
        public string UnitLabel => InterestCalculator.GetUnitLabel(InputLoan.TermUnit);

        /// <summary>Header cột kỳ trong bảng Schedule</summary>
        public string PeriodColumnHeader => InputLoan.TermUnit switch
        {
            TermUnit.Monthly => "Tháng",
            TermUnit.Quarterly => "Quý",
            TermUnit.Yearly => "Năm",
            _ => "Kỳ"
        };

        /// <summary>Tổng số kỳ trong lịch trả nợ</summary>
        public int TotalPeriods => Schedule.Count;

        /// <summary>Lãi suất/kỳ format theo TermUnit</summary>
        public string PeriodRateFormatted
        {
            get
            {
                decimal r = InterestCalculator.GetPeriodRate(
                    InputLoan.AnnualInterestRate, InputLoan.TermUnit);
                return FinancialHelper.FormatPercent(r, 4) + $"/{UnitLabel}";
            }
        }
		public string MethodDisplayName => InputLoan.Method switch
		{
			RepaymentMethod.PrincipalEqual
				=> "Trả gốc đều",

			RepaymentMethod.AnnuityEqual
				=> "Trả góp đều (Gốc + lãi)",

			RepaymentMethod.InterestOnlyThenPrincipal
				=> "Trả lãi định kỳ, trả gốc cuối kỳ",

			_ => InputLoan.Method.ToString()
		};


		// ── Thông báo tất toán trước hạn ──
		public bool HasEarlyRepayment => Schedule.Any(r => r.IsEarlyRepayment);
        public string EarlyRepaymentNote => HasEarlyRepayment
            ? $"Tất toán tại kỳ {Schedule.First(r => r.IsEarlyRepayment).PeriodLabel}, phí phạt: {FinancialHelper.FormatVND(TotalPenalty)}"
            : string.Empty;
    }
}
