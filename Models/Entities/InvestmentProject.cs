namespace FinanceSystem.Models.Entities
{
    /// <summary>
    /// Dự án đầu tư — dùng để thẩm định NPV, IRR, Payback Period
    /// </summary>
    public class InvestmentProject
    {
        /// <summary>Khóa chính</summary>
        public int Id { get; set; }

        /// <summary>Tên dự án</summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>Vốn đầu tư ban đầu tại kỳ 0 (VND)</summary>
        public decimal InitialInvestment { get; set; }

        /// <summary>
        /// Chuỗi dòng tiền ước tính từ kỳ 1 đến kỳ N (VND)
        /// Được lưu dưới dạng JSON string trong database
        /// </summary>
        public List<decimal> CashFlows { get; set; } = new();

        /// <summary>Tỷ suất chiết khấu (hurdle rate) theo năm (VD: 0.10 = 10%)</summary>
        public decimal DiscountRate { get; set; }

        /// <summary>Số kỳ = số phần tử trong CashFlows</summary>
        public int Periods => CashFlows.Count;
    }
}
