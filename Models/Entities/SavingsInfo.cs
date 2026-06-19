using System.ComponentModel.DataAnnotations;

public class SavingsInfo
{
    [Key]
    public Guid Id { get; set; }

    // ── Người thực hiện tính toán ──────────────────────────────────
    public Guid? UserId { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.Now;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền gốc phải lớn hơn 0")]
    public decimal Principal { get; set; }        // Số tiền gốc gửi

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Lãi suất năm không được âm")]
    public decimal AnnualInterestRate { get; set; } // Lãi suất năm (%)

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Kỳ hạn phải lớn hơn 0 tháng")]
    public int TermMonths { get; set; }           // Kỳ hạn (tháng)

    [Required]
    public DateTime DepositDate { get; set; }     // Ngày gửi

    // Hình thức nhận lãi
    [Required]
    public InterestPaymentType PaymentType { get; set; }
    // EndOfTerm | Monthly | Quarterly | Compound (gộp gốc)

    [Range(0, double.MaxValue, ErrorMessage = "Lãi suất không kỳ hạn không được âm")]
    public decimal DemandInterestRate { get; set; } = 0.1m; // mặc định 0.1%/năm

    // Rút trước hạn (nullable = không rút sớm)
    [Range(1, int.MaxValue, ErrorMessage = "Tháng rút trước hạn phải lớn hơn 0")]
    public int? EarlyWithdrawAtMonth { get; set; }
}

public enum InterestPaymentType
{
    EndOfTerm,   // Cuối kỳ
    Monthly,     // Hàng tháng
    Quarterly,   // Hàng quý
    Compound     // Gộp gốc (lãi kép)
}