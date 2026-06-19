using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Models.Entities
{
    public class OverdraftLoanInfo
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên khoản vay không được để trống")]
        [StringLength(100)]
        public string LoanName { get; set; } = "Vay thấu chi";

        [Range(0.01, double.MaxValue, ErrorMessage = "Hạn mức vay phải lớn hơn 0")]
        public decimal CreditLimit { get; set; }

        /// <summary>
        /// Lãi suất năm. VD: 0.12 = 12%/năm
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Lãi suất không được âm")]
        public decimal AnnualInterestRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public List<OverdraftUsageEntry> UsageEntries { get; set; } = new();
    }
}