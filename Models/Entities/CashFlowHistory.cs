namespace FinanceSystem.Models.Entities
{
    /// <summary>
    /// Lưu lịch sử mỗi lần người dùng tính toán dòng tiền
    /// </summary>
    public class CashFlowHistory
    {
        public int Id { get; set; }

        // ── Người thực hiện tính toán ─────────────────────────────────
        public Guid? UserId { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.Now;

        // ── Thông số đầu vào ──────────────────────────────────────────
        /// <summary>Vốn đầu tư ban đầu (có thể = 0 nếu chỉ phân tích dòng tiền)</summary>
        public decimal InitialInvestment { get; set; }

        /// <summary>Lãi suất chiết khấu (%/năm dạng thập phân, ví dụ 0.1 = 10%)</summary>
        public decimal DiscountRate { get; set; }

        /// <summary>Số bút toán dòng tiền đã nhập</summary>
        public int EntryCount { get; set; }

        /// <summary>Số kỳ phân tích (max period)</summary>
        public int PeriodCount { get; set; }

        // ── Kết quả tổng hợp ─────────────────────────────────────────
        public decimal TotalInflow { get; set; }
        public decimal TotalOutflow { get; set; }
        public decimal NetCashFlow { get; set; }

        /// <summary>Giá trị hiện tại thuần (NPV) — 0 nếu không có vốn đầu tư ban đầu</summary>
        public decimal NPV { get; set; }

        /// <summary>Tỷ suất hoàn vốn nội bộ (IRR) — 0 nếu không tính được</summary>
        public decimal IRR { get; set; }

        /// <summary>Kết luận phân tích dòng tiền</summary>
        public string Decision { get; set; } = string.Empty;

        /// <summary>Có kỳ dòng tiền âm không</summary>
        public bool HasDeficit { get; set; }
    }
}
