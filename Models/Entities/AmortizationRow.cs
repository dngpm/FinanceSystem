namespace FinanceSystem.Models.Entities
{
    /// <summary>
    /// Một dòng trong bảng lịch trả nợ (Amortization Schedule)
    /// </summary>
    public class AmortizationRow
    {
        /// <summary>Khóa chính</summary>
        public int Id { get; set; }

        /// <summary>Khóa ngoại — liên kết về LoanInfo</summary>
        public int LoanInfoId { get; set; }

        /// <summary>Kỳ thứ mấy (1, 2, 3, ... N)</summary>
        public int Period { get; set; }

        /// <summary>
        /// Nhãn kỳ thân thiện để View hiển thị.
        /// InterestCalculator tự gán: "Tháng 1", "Quý 2", "Năm 3".
        /// [NotMapped] — không lưu DB.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string PeriodLabel { get; set; } = string.Empty;

        /// <summary>Số dư nợ đầu kỳ (VND)</summary>
        public decimal OpeningBalance { get; set; }

        /// <summary>Gốc phải trả kỳ này (VND)</summary>
        public decimal PrincipalPayment { get; set; }

        /// <summary>Lãi phải trả kỳ này (VND)</summary>
        public decimal InterestPayment { get; set; }

        /// <summary>Tổng tiền phải trả kỳ này = Gốc + Lãi (VND)</summary>
        public decimal TotalPayment => PrincipalPayment + InterestPayment;

        /// <summary>Số dư nợ cuối kỳ (VND)</summary>
        public decimal ClosingBalance { get; set; }

        /// <summary>Đánh dấu kỳ tất toán trước hạn</summary>
        public bool IsEarlyRepayment { get; set; } = false;

        /// <summary>Phí phạt tất toán sớm kỳ này (VND)</summary>
        public decimal EarlyRepaymentPenalty { get; set; } = 0m;

        /// <summary>Navigation property ngược về LoanInfo</summary>
        public LoanInfo? Loan { get; set; }
    }
}
