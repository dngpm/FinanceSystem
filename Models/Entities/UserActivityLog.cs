using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Models.Entities
{
    public class UserActivityLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }

        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>Tên module/tính năng người dùng đã dùng (ví dụ: "Tính khoản vay", "Tiết kiệm"...)</summary>
        [StringLength(100)]
        public string Module { get; set; } = string.Empty;

        /// <summary>Mô tả hành động cụ thể</summary>
        [StringLength(300)]
        public string Action { get; set; } = string.Empty;

        /// <summary>URL đã truy cập</summary>
        [StringLength(500)]
        public string Path { get; set; } = string.Empty;

        /// <summary>HTTP Method (GET/POST)</summary>
        [StringLength(10)]
        public string HttpMethod { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation
        public AppUser? User { get; set; }
    }

    /// <summary>Ánh xạ URL/Controller+Action sang tên module thân thiện</summary>
    public static class ActivityModuleMapper
    {
        private static readonly Dictionary<string, (string Module, string Action)> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            // Khoản vay
            ["Loan/Index"]          = ("Tính khoản vay", "Mở trang tính khoản vay"),
            ["Loan/Calculate"]      = ("Tính khoản vay", "Tính toán khoản vay"),
            ["Loan/Save"]           = ("Tính khoản vay", "Lưu kết quả vay"),
            ["Loan/History"]        = ("Tính khoản vay", "Xem lịch sử khoản vay"),
            ["Loan/Delete"]         = ("Tính khoản vay", "Xóa khoản vay"),
            ["Loan/Overdraft"]      = ("Tính khoản vay", "Mở thấu chi"),
            ["Loan/OverdraftCalc"]  = ("Tính khoản vay", "Tính thấu chi"),

            // Tiết kiệm
            ["Savings/Index"]       = ("Tiết kiệm", "Mở trang tính tiết kiệm"),
            ["Savings/Calculate"]   = ("Tiết kiệm", "Tính toán tiết kiệm"),
            ["Savings/Save"]        = ("Tiết kiệm", "Lưu kết quả tiết kiệm"),
            ["Savings/History"]     = ("Tiết kiệm", "Xem lịch sử tiết kiệm"),
            ["Savings/Delete"]      = ("Tiết kiệm", "Xóa bản ghi tiết kiệm"),

            // Dòng tiền
            ["CashFlow/Index"]      = ("Phân tích dòng tiền", "Mở trang dòng tiền"),
            ["CashFlow/Add"]        = ("Phân tích dòng tiền", "Thêm bút toán"),
            ["CashFlow/Delete"]     = ("Phân tích dòng tiền", "Xóa bút toán"),
            ["CashFlow/Edit"]       = ("Phân tích dòng tiền", "Sửa bút toán"),

            // Tài khoản
            ["Account/Login"]       = ("Tài khoản", "Đăng nhập"),
            ["Account/Logout"]      = ("Tài khoản", "Đăng xuất"),
            ["Account/Register"]    = ("Tài khoản", "Đăng ký tài khoản"),
            ["Account/Profile"]     = ("Tài khoản", "Xem hồ sơ"),
            ["Account/ChangePassword"] = ("Tài khoản", "Đổi mật khẩu"),

            // Dịch vụ
            ["Services/Index"]      = ("Dịch vụ", "Mở trang dịch vụ"),
            ["Services/Submit"]     = ("Dịch vụ", "Gửi yêu cầu dịch vụ"),

            // Admin
            ["Admin/Users"]         = ("Quản trị", "Xem danh sách người dùng"),
            ["Admin/ServiceRequests"] = ("Quản trị", "Xem yêu cầu dịch vụ"),
            ["Admin/ReviewRequest"] = ("Quản trị", "Duyệt yêu cầu"),
            ["Admin/ToggleLock"]    = ("Quản trị", "Khóa/Mở khóa tài khoản"),
            ["Admin/PromoteToAdmin"]= ("Quản trị", "Cấp quyền Admin"),
            ["Admin/RevokeAdmin"]   = ("Quản trị", "Thu hồi quyền Admin"),
            ["Admin/UserActivity"]  = ("Quản trị", "Xem lịch sử hoạt động"),
        };

        public static (string Module, string Action) Resolve(string controller, string action)
        {
            var key = $"{controller}/{action}";
            return _map.TryGetValue(key, out var result)
                ? result
                : (controller, action);
        }
    }

    /// <summary>Thống kê tổng hợp cho một người dùng</summary>
    public class UserActivityStats
    {
        public string Username     { get; set; } = string.Empty;
        public int    TotalActions { get; set; }
        public DateTime FirstSeen  { get; set; }
        public DateTime LastSeen   { get; set; }
        public int    ActiveDays   { get; set; }
        public Dictionary<string, int> ModuleCounts { get; set; } = new();
    }
}
