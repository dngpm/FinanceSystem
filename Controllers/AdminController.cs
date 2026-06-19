using FinanceSystem.Data;
using FinanceSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Controllers
{
    /// <summary>
    /// Khu vực quản trị — chỉ tài khoản có role Admin mới được truy cập.
    /// Routes: /Admin/*
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly FinanceDbContext _db;

        public AdminController(FinanceDbContext db)
        {
            _db = db;
        }

        // ═══════════════════════════════════════════════════════════
        // TRANG CHỦ ADMIN — Thống kê tổng quan
        // GET /Admin/Index
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Tổng tiết kiệm đang gửi
            var totalSavings = await _db.SavingsInfos.SumAsync(s => s.Principal);

            // Tổng dư nợ cho vay (tổng gốc của tất cả khoản vay đã tạo)
            var totalLoan = await _db.LoanInfos.SumAsync(l => (decimal)l.Principal);

            // Tổng hạn mức thấu chi đang active

            // Tổng dư nợ thấu chi thực tế (tổng số tiền đã dùng)

            // Tổng dự án đầu tư

            // Số người dùng
            var totalUsers    = await _db.AppUsers.CountAsync();
            var lockedUsers   = await _db.AppUsers.CountAsync(u => u.IsLocked);
            var activeUsers   = totalUsers - lockedUsers;

            // Người dùng mới trong 7 ngày gần nhất
            var since7Days    = DateTime.Now.AddDays(-7);
            var newUsers7Days = await _db.AppUsers.CountAsync(u => u.CreatedAt >= since7Days);

            // Tổng bút toán thu chi
            var totalCashFlowEntries = await _db.CashFlowEntries.CountAsync();

            // Lãi suất cuối cùng được cập nhật
            var lastRateUpdate = await _db.InterestRateTables
                .OrderByDescending(r => r.UpdatedAt)
                .Select(r => (DateTime?)r.UpdatedAt)
                .FirstOrDefaultAsync();

            ViewBag.TotalSavings         = totalSavings;
            ViewBag.TotalLoan            = totalLoan;
            ViewBag.TotalUsers           = totalUsers;
            ViewBag.ActiveUsers          = activeUsers;
            ViewBag.LockedUsers          = lockedUsers;
            ViewBag.NewUsers7Days        = newUsers7Days;
            ViewBag.TotalCashFlowEntries = totalCashFlowEntries;
            ViewBag.LastRateUpdate       = lastRateUpdate;

            return View();
        }

        // ═══════════════════════════════════════════════════════════
        // QUẢN LÝ NGƯỜI DÙNG
        // ═══════════════════════════════════════════════════════════

        // GET /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users(string? search, int page = 1)
        {
            const int pageSize = 15;

            var query = _db.AppUsers
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search    = search;
            ViewBag.Page      = page;
            ViewBag.TotalPage = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total     = total;

            return View(users);
        }

        // POST /Admin/ToggleLock — Khóa / Mở khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(Guid id)
        {
            var user = await _db.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            // Không cho phép khóa chính tài khoản admin đang đăng nhập
            var currentUsername = User.Identity?.Name;
            if (user.Username == currentUsername)
            {
                TempData["Error"] = "Không thể khóa tài khoản đang đăng nhập.";
                return RedirectToAction("Users");
            }

            user.IsLocked = !user.IsLocked;
            await _db.SaveChangesAsync();

            TempData["Success"] = user.IsLocked
                ? $"Đã khóa tài khoản «{user.Username}»."
                : $"Đã mở khóa tài khoản «{user.Username}».";

            return RedirectToAction("Users");
        }

        // POST /Admin/PromoteToAdmin — Gán quyền Admin cho user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(Guid id)
        {
            var user = await _db.AppUsers
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (user.IsAdmin())
            {
                TempData["Error"] = $"«{user.Username}» đã là Admin rồi.";
                return RedirectToAction("Users");
            }

            var adminRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == SystemRole.Admin);
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Id          = Guid.NewGuid(),
                    RoleName    = SystemRole.Admin,
                    Description = "Quản trị viên hệ thống"
                };
                _db.Roles.Add(adminRole);
                await _db.SaveChangesAsync();
            }

            _db.UserRoles.Add(new UserRole
            {
                Id     = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = adminRole.Id
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã cấp quyền Admin cho «{user.Username}».";
            return RedirectToAction("Users");
        }

        // POST /Admin/RevokeAdmin — Thu hồi quyền Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeAdmin(Guid id)
        {
            var currentUsername = User.Identity?.Name;
            var user = await _db.AppUsers
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (user.Username == currentUsername)
            {
                TempData["Error"] = "Không thể tự thu hồi quyền Admin của chính mình.";
                return RedirectToAction("Users");
            }

            var adminUserRole = user.UserRoles
                .FirstOrDefault(ur => ur.Role?.RoleName == SystemRole.Admin);

            if (adminUserRole == null)
            {
                TempData["Error"] = $"«{user.Username}» không có quyền Admin.";
                return RedirectToAction("Users");
            }

            _db.UserRoles.Remove(adminUserRole);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã thu hồi quyền Admin của «{user.Username}».";
            return RedirectToAction("Users");
        }

        // ═══════════════════════════════════════════════════════════
        // LỊCH SỬ HOẠT ĐỘNG NGƯỜI DÙNG
        // ═══════════════════════════════════════════════════════════

        // GET /Admin/UserActivity?userId=...&module=...&page=1
        [HttpGet]
        public async Task<IActionResult> UserActivity(Guid? userId, string? module, int page = 1)
        {
            const int pageSize = 30;

            var query = _db.UserActivityLogs
                .AsNoTracking()
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId);

            if (!string.IsNullOrWhiteSpace(module))
                query = query.Where(l => l.Module == module);

            var total = await query.CountAsync();
            var logs  = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Danh sách user để filter
            var users = await _db.AppUsers
                .AsNoTracking()
                .OrderBy(u => u.Username)
                .Select(u => new { u.Id, u.Username })
                .ToListAsync();

            // Danh sách module để filter
            var modules = await _db.UserActivityLogs
                .AsNoTracking()
                .Select(l => l.Module)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            // Thống kê nhanh cho user được chọn
            UserActivityStats? stats = null;
            if (userId.HasValue)
            {
                var selectedUser = users.FirstOrDefault(u => u.Id == userId);
                var userLogs = await _db.UserActivityLogs
                    .AsNoTracking()
                    .Where(l => l.UserId == userId)
                    .ToListAsync();

                if (userLogs.Any())
                {
                    stats = new UserActivityStats
                    {
                        Username       = selectedUser?.Username ?? "",
                        TotalActions   = userLogs.Count,
                        FirstSeen      = userLogs.Min(l => l.Timestamp),
                        LastSeen       = userLogs.Max(l => l.Timestamp),
                        ModuleCounts   = userLogs
                            .GroupBy(l => l.Module)
                            .OrderByDescending(g => g.Count())
                            .ToDictionary(g => g.Key, g => g.Count()),
                        ActiveDays     = userLogs.Select(l => l.Timestamp.Date).Distinct().Count()
                    };
                }
            }

            ViewBag.UserId    = userId;
            ViewBag.Module    = module;
            ViewBag.Page      = page;
            ViewBag.TotalPage = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total     = total;
            ViewBag.Users     = users;
            ViewBag.Modules   = modules;
            ViewBag.Stats     = stats;

            return View(logs);
        }

        // POST /Admin/ClearActivity — Xóa log cũ (> 90 ngày)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearActivity()
        {
            var cutoff = DateTime.Now.AddDays(-90);
            var old = await _db.UserActivityLogs
                .Where(l => l.Timestamp < cutoff)
                .ToListAsync();

            _db.UserActivityLogs.RemoveRange(old);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa {old.Count} bản ghi log cũ hơn 90 ngày.";
            return RedirectToAction("UserActivity");
        }

        // ═══════════════════════════════════════════════════════════
        // QUẢN LÝ BIỂU LÃI SUẤT
        // ═══════════════════════════════════════════════════════════

        // GET /Admin/InterestRates
        [HttpGet]
        public async Task<IActionResult> InterestRates()
        {
            var rates = await _db.InterestRateTables
                .OrderBy(r => r.RateType)
                .ThenBy(r => r.TermMonths)
                .ToListAsync();

            return View(rates);
        }

        // GET /Admin/EditRate/{id}
        [HttpGet]
        public async Task<IActionResult> EditRate(Guid id)
        {
            var rate = await _db.InterestRateTables.FindAsync(id);
            if (rate == null) return NotFound();
            return View(rate);
        }

        // POST /Admin/EditRate/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRate(Guid id, decimal annualRatePercent, string note)
        {
            var rate = await _db.InterestRateTables.FindAsync(id);
            if (rate == null) return NotFound();

            if (annualRatePercent < 0 || annualRatePercent > 100)
            {
                ModelState.AddModelError("", "Lãi suất phải nằm trong khoảng 0 – 100%.");
                return View(rate);
            }

            rate.AnnualRatePercent = annualRatePercent;
            rate.Note              = note?.Trim() ?? string.Empty;
            rate.UpdatedAt         = DateTime.Now;
            rate.UpdatedBy         = User.Identity?.Name ?? "admin";

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật lãi suất «{rate.TermLabel}» ({rate.RateType}).";
            return RedirectToAction("InterestRates");
        }

        // GET /Admin/AddRate
        [HttpGet]
        public IActionResult AddRate()
        {
            return View(new InterestRateTable());
        }

        // POST /Admin/AddRate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRate(InterestRateTable model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.AnnualRatePercent < 0 || model.AnnualRatePercent > 100)
            {
                ModelState.AddModelError("", "Lãi suất phải nằm trong khoảng 0 – 100%.");
                return View(model);
            }

            model.Id        = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(model.ProductType))
            {
                model.ProductType = model.RateType == InterestRateType.Savings
                    ? FinancialProductType.SavingDeposit
                    : FinancialProductType.InstallmentLoan;
            }
            model.UpdatedAt = DateTime.Now;
            model.UpdatedBy = User.Identity?.Name ?? "admin";

            _db.InterestRateTables.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm kỳ hạn «{model.TermLabel}» mới.";
            return RedirectToAction("InterestRates");
        }

        // POST /Admin/DeleteRate/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRate(Guid id)
        {
            var rate = await _db.InterestRateTables.FindAsync(id);
            if (rate == null) return NotFound();

            _db.InterestRateTables.Remove(rate);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa kỳ hạn «{rate.TermLabel}».";
            return RedirectToAction("InterestRates");
        }
    }
}
