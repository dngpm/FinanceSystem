using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Models.Entities
{
    /// <summary>
    /// Bảng lãi suất chuẩn của hệ thống — chỉ Admin mới được sửa.
    /// </summary>
    public class InterestRateTable
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Loại: "Savings" (tiết kiệm) hoặc "Loan" (vay)</summary>
        [Required]
        [StringLength(20)]
        public string RateType { get; set; } = string.Empty;

        [Required]
        [StringLength(40)]
        public string ProductType { get; set; } = FinancialProductType.SavingDeposit;

        /// <summary>Nhãn hiển thị kỳ hạn, VD: "1 tháng", "6 tháng", "12 tháng"</summary>
        [Required]
        [StringLength(50)]
        public string TermLabel { get; set; } = string.Empty;

        /// <summary>Số tháng của kỳ hạn (dùng để map với form tính)</summary>
        public int TermMonths { get; set; }

        /// <summary>Lãi suất %/năm, lưu dạng thập phân VD: 5.2 = 5.2%/năm</summary>
        [Required]
        [Range(0, 100)]
        public decimal AnnualRatePercent { get; set; }

        /// <summary>Ghi chú thêm (ưu đãi, điều kiện...)</summary>
        [StringLength(200)]
        public string Note { get; set; } = string.Empty;

        /// <summary>Thời điểm cập nhật lần cuối</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>Người cập nhật lần cuối</summary>
        [StringLength(50)]
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public static class InterestRateType
    {
        public const string Savings = "Savings";
        public const string Loan = "Loan";
    }
}
