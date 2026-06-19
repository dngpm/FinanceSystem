
namespace FinanceSystem.Models.Entities
{
    public class LoanInfo
    {
        public int Id { get; set; }

        // ── Người thực hiện tính toán ──────────────────────────────────
        public Guid? UserId { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.Now;

        public decimal Principal { get; set; }
        public decimal AnnualInterestRate { get; set; }

        // Số kỳ vay (tháng / quý / năm tùy TermUnit)
        public int TermCount { get; set; }

        // Đơn vị kỳ hạn: Monthly / Quarterly / Yearly
        public TermUnit TermUnit { get; set; } = TermUnit.Monthly;

        public RepaymentMethod Method { get; set; }
        public bool IsEarlyRepayment { get; set; }
        public int? EarlyRepaymentPeriod { get; set; }
        public decimal EarlyRepaymentPenaltyRate { get; set; } = 0.02m;
        public decimal ServiceFee { get; set; } = 0m;
        public ICollection<AmortizationRow> Schedule { get; set; } = new List<AmortizationRow>();

        // ── Computed helpers ──────────────────────────────────────

        public int PeriodsPerYear => TermUnit switch
        {
            TermUnit.Monthly => 12,
            TermUnit.Quarterly => 4,
            TermUnit.Yearly => 1,
            _ => 12
        };

        public decimal PeriodRate => AnnualInterestRate / PeriodsPerYear;

        // Tổng số tháng — dùng để validate giới hạn 30 năm
        public int TermMonths => TermUnit switch
        {
            TermUnit.Monthly => TermCount,
            TermUnit.Quarterly => TermCount * 3,
            TermUnit.Yearly => TermCount * 12,
            _ => TermCount
        };
    }

    public enum TermUnit
    {
        Monthly,    // Hàng tháng
        Quarterly,  // Hàng quý
        Yearly    // Hàng năm
    }

    public enum RepaymentMethod
    {
        PrincipalEqual,
        AnnuityEqual,
        InterestOnlyThenPrincipal
    }
}