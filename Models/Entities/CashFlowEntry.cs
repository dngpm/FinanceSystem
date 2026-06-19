using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Models.Entities
{
    /// <summary>
    /// Một bút toán dòng tiền (Cash Flow Entry)
    /// Giá trị dương = thu vào, âm = chi ra (VND)
    /// </summary>
    public class CashFlowEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả bút toán.")]
        [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá trị dòng tiền.")]
        [Range(-10_000_000_000, 10_000_000_000,
            ErrorMessage = "Giá trị phải nằm trong khoảng ±10 tỷ VND.")]
        public decimal Amount { get; set; }

        [Range(1, 120, ErrorMessage = "Kỳ phải từ 1 đến 120.")]
        public int Period { get; set; } = 1;

        [Required(ErrorMessage = "Vui lòng chọn loại dòng tiền.")]
        public CashFlowType Type { get; set; }

        // ── Computed helpers ──────────────────────────────

        /// <summary>Thu vào (Amount > 0)</summary>
        public bool IsInflow => Amount > 0;

        /// <summary>Chi ra (Amount < 0)</summary>
        public bool IsOutflow => Amount < 0;

        /// <summary>Giá trị tuyệt đối — dùng cho hiển thị</summary>
        public decimal AbsAmount => Math.Abs(Amount);
    }

    public enum CashFlowType
    {
        /// <summary>Dòng tiền từ hoạt động kinh doanh (Operating Cash Flow)</summary>
        Operating,

        /// <summary>Dòng tiền từ hoạt động đầu tư (Investing Cash Flow)</summary>
        Investing,

        /// <summary>Dòng tiền từ hoạt động tài chính (Financing Cash Flow)</summary>
        Financing
    }
}