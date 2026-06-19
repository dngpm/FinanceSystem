using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceSystem.Models.Entities
{
    public class OverdraftUsageEntry
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OverdraftLoanInfoId { get; set; }

        public OverdraftLoanInfo? OverdraftLoan { get; set; }

        /// <summary>Số tiền thực tế đã sử dụng từ hạn mức</summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền sử dụng phải lớn hơn 0")]
        public decimal UsedAmount { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        /// <summary>
        /// Ngày kết thúc sử dụng vốn. Nếu chưa trả thì để null, service sẽ tính đến ngày hiện tại.
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Lãi suất riêng cho lần sử dụng này. Nếu = 0 thì lấy lãi suất từ OverdraftLoanInfo.
        /// VD: 0.12 = 12%/năm
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Lãi suất không được âm")]
        public decimal AnnualInterestRate { get; set; }

        [StringLength(200)]
        public string Note { get; set; } = string.Empty;

        [NotMapped]
        public int UsedDays
        {
            get
            {
                var endDate = ToDate ?? DateTime.Today;
                return Math.Max(0, (endDate.Date - FromDate.Date).Days);
            }
        }
    }
}