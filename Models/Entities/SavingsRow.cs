using System.ComponentModel.DataAnnotations;

public class SavingsRow
{
    [Key]
    public Guid Id { get; set; }

    public Guid SavingsInfoId { get; set; }   // Khóa ngoại liên kết với SavingsInfo

    [Range(1, int.MaxValue, ErrorMessage = "Kỳ phải lớn hơn 0")]
    public int Period { get; set; }           // Kỳ thứ mấy

    [Required(ErrorMessage = "Tên kỳ không được để trống")]
    [StringLength(50, ErrorMessage = "Tên kỳ không được vượt quá 50 ký tự")]
    public string PeriodLabel { get; set; } = string.Empty;   // "Tháng 1", "Quý 1"...

    [Range(0, double.MaxValue, ErrorMessage = "Số dư đầu kỳ không được âm")]
    public decimal OpeningBalance { get; set; } // Số dư đầu kỳ

    [Range(0, double.MaxValue, ErrorMessage = "Lãi phát sinh không được âm")]
    public decimal InterestEarned { get; set; } // Lãi phát sinh kỳ này

    [Range(0, double.MaxValue, ErrorMessage = "Lãi thực trả không được âm")]
    public decimal InterestPaid { get; set; }   // Lãi thực trả ra (0 nếu gộp gốc)

    [Range(0, double.MaxValue, ErrorMessage = "Số dư cuối kỳ không được âm")]
    public decimal ClosingBalance { get; set; } // Số dư cuối kỳ (tăng nếu compound)

    public bool IsEarlyWithdraw { get; set; }   // Có phải kỳ rút trước hạn không
}