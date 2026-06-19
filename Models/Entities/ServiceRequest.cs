using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Models.Entities
{
    public class ServiceRequest
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }
        public AppUser? User { get; set; }

        [Required]
        [StringLength(30)]
        public string RequestType { get; set; } = ServiceRequestType.Loan;

        [Required]
        [StringLength(40)]
        public string ProductType { get; set; } = FinancialProductType.UnsecuredLoan;

        [Required]
        [Range(1000, double.MaxValue, ErrorMessage = "So tien phai lon hon 0.")]
        public decimal Amount { get; set; }

        [Range(0, 600)]
        public int TermMonths { get; set; }

        [Range(0, 100)]
        public decimal AnnualRatePercent { get; set; }

        [StringLength(500)]
        public string CustomerNote { get; set; } = string.Empty;

        [StringLength(500)]
        public string AdminNote { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = ServiceRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }

        [StringLength(50)]
        public string ReviewedBy { get; set; } = string.Empty;
    }

    public static class ServiceRequestType
    {
        public const string Loan = "Loan";
        public const string Savings = "Savings";
    }

    public static class ServiceRequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";
    }

    public static class FinancialProductType
    {
        public const string UnsecuredLoan = "UnsecuredLoan";
        public const string OverdraftLoan = "OverdraftLoan";
        public const string InstallmentLoan = "InstallmentLoan";
        public const string SavingDeposit = "SavingDeposit";
    }
}
