using System;
using System.Collections.Generic;

namespace FinanceSystem.Models.ViewModels
{
    /// <summary>
    /// ViewModel tổng hợp dòng tiền từ Tiết kiệm (Inflow) và Vay vốn (Outflow)
    /// Dùng cho Dashboard phân tích dòng tiền kết hợp
    /// </summary>
    public class CombinedCashFlowViewModel
    {
        // THÔNG TIN ĐẦU VÀO

        /// <summary>Số dư tiền mặt ban đầu của người dùng</summary>
        public decimal InitialCashBalance { get; set; }

        /// <summary>Ngưỡng cảnh báo số dư thấp</summary>
        public decimal WarningThreshold { get; set; }

        /// <summary>Tổng số tháng phân tích (lấy max của kỳ hạn vay và tiết kiệm)</summary>
        public int TotalMonths { get; set; }

        /// <summary>Có kịch bản lãi suất thả nổi không (để hiển thị nút Stress Test)</summary>
        public bool HasFloatingRate { get; set; }

        // KẾT QUẢ TỔNG HỢP

        /// <summary>Tổng gốc tiết kiệm nhận lại trong toàn kỳ</summary>
        public decimal TotalSavingsPrincipalReceived { get; set; }

        /// <summary>Tổng lãi tiết kiệm thu được trong toàn kỳ</summary>
        public decimal TotalSavingsInterestReceived { get; set; }

        /// <summary>Tổng tiền vào từ tiết kiệm</summary>
        public decimal TotalSavingsReceived =>
            TotalSavingsPrincipalReceived + TotalSavingsInterestReceived;

        /// <summary>Tổng tiền lãi phải trả cho khoản vay trong toàn kỳ</summary>
        public decimal TotalLoanInterestPaid { get; set; }

        /// <summary>Tổng gốc vay phải trả trong toàn kỳ</summary>
        public decimal TotalLoanPrincipalPaid { get; set; }

        /// <summary>Tổng phí/phạt trả nợ trước hạn</summary>
        public decimal TotalLoanPenaltyPaid { get; set; }

        /// <summary>Tổng tiền trả khoản vay</summary>
        public decimal TotalLoanPaid =>
            TotalLoanPrincipalPaid + TotalLoanInterestPaid + TotalLoanPenaltyPaid;

        /// <summary>Dòng tiền thuần tích lũy cuối kỳ (CashBalance tháng cuối cùng)</summary>
        public decimal FinalCashBalance { get; set; }

        /// <summary>
        /// Trạng thái tổng thể:
        /// Healthy = an toàn | Warning = dưới ngưỡng cảnh báo | Critical = đã âm
        /// </summary>
        public CashFlowStatus OverallStatus { get; set; }

        /// <summary>Tháng đầu tiên dự báo số dư tiền mặt bị âm (null = không bao giờ âm)</summary>
        public int? FirstNegativeMonth { get; set; }

        /// <summary>Danh sách dòng tiền từng tháng — dữ liệu cho bảng và đồ thị</summary>
        public List<MonthlyCashFlowRow> MonthlyRows { get; set; } = new();

        /// <summary>
        /// Kết quả kịch bản giả định lãi suất tăng.
        /// Null nếu người dùng chưa chạy Stress Test.
        /// </summary>
        public StressTestResult? StressTest { get; set; }
    }

    public class MonthlyCashFlowRow
    {
        /// <summary>Tháng thứ mấy (1, 2, 3...)</summary>
        public int Month { get; set; }

        /// <summary>Nhãn hiển thị: "Tháng 1 (01/2025)"</summary>
        public string MonthLabel { get; set; } = string.Empty;

        // INFLOW

        /// <summary>Tiền gốc tiết kiệm nhận lại khi đáo hạn hoặc rút trước hạn</summary>
        public decimal SavingsPrincipalInflow { get; set; }

        /// <summary>Lãi tiết kiệm nhận về tháng này</summary>
        public decimal SavingsInterestInflow { get; set; }

        /// <summary>Các khoản thu khác nếu người dùng nhập thêm</summary>
        public decimal OtherInflow { get; set; }

        /// <summary>Tổng dòng tiền vào tháng này</summary>
        public decimal TotalInflow =>
            SavingsPrincipalInflow + SavingsInterestInflow + OtherInflow;

        // OUTFLOW

        /// <summary>Tiền gốc vay phải trả tháng này</summary>
        public decimal LoanPrincipalOutflow { get; set; }

        /// <summary>Tiền lãi vay phải trả tháng này</summary>
        public decimal LoanInterestOutflow { get; set; }

        /// <summary>Phí/phạt trả nợ trước hạn nếu có</summary>
        public decimal LoanPenaltyOutflow { get; set; }

        /// <summary>Tổng nghĩa vụ nợ tháng này</summary>
        public decimal TotalLoanPayment =>
            LoanPrincipalOutflow + LoanInterestOutflow + LoanPenaltyOutflow;

        /// <summary>Các khoản chi khác nếu người dùng nhập thêm</summary>
        public decimal OtherOutflow { get; set; }

        /// <summary>Tổng dòng tiền ra tháng này</summary>
        public decimal TotalOutflow => TotalLoanPayment + OtherOutflow;

        // RESULT

        /// <summary>Dòng tiền thuần tháng này = TotalInflow - TotalOutflow</summary>
        public decimal NetCashFlow => TotalInflow - TotalOutflow;

        /// <summary>Số dư tiền mặt cuối tháng này</summary>
        public decimal ClosingCashBalance { get; set; }

        /// <summary>Lãi suất vay đang áp dụng tháng này (%/năm)</summary>
        public decimal AppliedLoanRate { get; set; }

        /// <summary>Trạng thái cảnh báo của tháng này</summary>
        public CashFlowStatus MonthStatus { get; set; }
    }

    public class StressTestResult
    {
        /// <summary>Lãi suất giả định tăng lên (%/năm)</summary>
        public decimal StressedAnnualRate { get; set; }

        /// <summary>Tháng bắt đầu áp dụng lãi suất mới</summary>
        public int RateChangeAtMonth { get; set; }

        /// <summary>Bảng dòng tiền theo kịch bản lãi tăng</summary>
        public List<MonthlyCashFlowRow> StressedMonthlyRows { get; set; } = new();

        /// <summary>Tổng tiền lãi vay tăng thêm so với kịch bản gốc</summary>
        public decimal AdditionalInterestCost { get; set; }

        /// <summary>Tháng đầu tiên số dư bị âm trong kịch bản stress</summary>
        public int? FirstNegativeMonthUnderStress { get; set; }

        /// <summary>Số dư cuối kỳ trong kịch bản stress</summary>
        public decimal FinalCashBalanceUnderStress { get; set; }

        /// <summary>Tóm tắt cảnh báo để hiển thị trên UI</summary>
        public string WarningMessage { get; set; } = string.Empty;
    }

    public enum CashFlowStatus
    {
        /// <summary>Số dư an toàn</summary>
        Healthy,

        /// <summary>Số dư dương nhưng dưới ngưỡng cảnh báo</summary>
        Warning,

        /// <summary>Số dư đã âm</summary>
        Critical
    }
}