using FinanceSystem.Data;
using FinanceSystem.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinanceSystem.Controllers
{
    /// <summary>
    /// Controller xử lý đăng ký / đăng nhập / đăng xuất
    /// Route: /Account
    /// </summary>
    public class AccountController : Controller
    {
        private readonly FinanceDbContext _db;

        public AccountController(FinanceDbContext db)
        {
            _db = db;
        }

        // ── GET /Account/Login ───────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Cho phép Guest vào trang đăng nhập để đăng nhập thật sự
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Guest"))
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ── POST /Account/Login ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            // Tìm user theo username
            var user = await _db.AppUsers
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            // Kiểm tra mật khẩu
            if (user == null || user.PasswordHash != HashPassword(password))
            {
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu.");
                return View();
            }

            if (user.IsLocked)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View();
            }

            // Tạo Claims để đăng nhập
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name,            user.Username),
                new(ClaimTypes.Email,           user.Email)
            };

            // Thêm Role vào Claims
            foreach (var ur in user.UserRoles.Where(ur => ur.Role != null))
                claims.Add(new Claim(ClaimTypes.Role, ur.Role!.RoleName));

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true }
            );

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ── GET /Account/Register ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Guest()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "guest"),
                new(ClaimTypes.Name, "Khách"),
                new(ClaimTypes.Role, "Guest")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false }
            );

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ── POST /Account/Register ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string username, string email,
            string password, string confirmPassword)
        {
            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email)    ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            // Kiểm tra trùng username / email
            bool existUsername = await _db.AppUsers
                .AnyAsync(u => u.Username == username);
            bool existEmail = await _db.AppUsers
                .AnyAsync(u => u.Email == email);

            if (existUsername)
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                return View();
            }

            if (existEmail)
            {
                ModelState.AddModelError("", "Email đã được sử dụng.");
                return View();
            }

            // Lấy hoặc tạo Role "User"
            var userRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.RoleName == SystemRole.User);

            if (userRole == null)
            {
                userRole = new Role
                {
                    Id          = Guid.NewGuid(),
                    RoleName    = SystemRole.User,
                    Description = "Người dùng thông thường"
                };
                _db.Roles.Add(userRole);
                await _db.SaveChangesAsync();
            }

            // Tạo user mới
            var newUser = new AppUser
            {
                Id           = Guid.NewGuid(),
                Username     = username.Trim(),
                Email        = email.Trim().ToLower(),
                PasswordHash = HashPassword(password),
                CreatedAt    = DateTime.Now
            };

            _db.AppUsers.Add(newUser);

            // Gán Role
            _db.UserRoles.Add(new UserRole
            {
                Id     = Guid.NewGuid(),
                UserId = newUser.Id,
                RoleId = userRole.Id
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ── POST /Account/Logout ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        // ── GET /Account/Profile ─────────────────────────────────────
        // Xem thông tin tài khoản hiện tại
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null) return RedirectToAction("Login");

            // Khách vãng lai không có profile
            if (idClaim == "guest" || !Guid.TryParse(idClaim, out var userId))
                return RedirectToAction("Index", "Home");

            var user = await _db.AppUsers
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return View(user);
        }

        // ── GET /Account/MyHistory ───────────────────────────────────
        // Lịch sử sử dụng công cụ tính toán của người dùng hiện tại
        [HttpGet]
        public async Task<IActionResult> MyHistory(int page = 1, string? tab = null)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null || idClaim == "guest" || !Guid.TryParse(idClaim, out var userId))
                return RedirectToAction("Login");

            // Lịch sử khoản vay đã tính
            var loans = await _db.LoanInfos
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CalculatedAt)
                .Take(50)
                .ToListAsync();

            // Lịch sử tiết kiệm đã tính
            var savings = await _db.SavingsInfos
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CalculatedAt)
                .Take(50)
                .ToListAsync();

            // Lịch sử dòng tiền đã tính
            var cashFlows = await _db.CashFlowHistories
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CalculatedAt)
                .Take(50)
                .ToListAsync();

            // Thống kê
            ViewBag.Tab           = tab ?? "loan";
            ViewBag.LoanCount     = loans.Count;
            ViewBag.SavingsCount  = savings.Count;
            ViewBag.CashFlowCount = cashFlows.Count;
            ViewBag.TotalCount    = loans.Count + savings.Count + cashFlows.Count;
            ViewBag.LastSeen      = loans.Select(l => l.CalculatedAt)
                                         .Concat(savings.Select(s => s.CalculatedAt))
                                         .Concat(cashFlows.Select(c => c.CalculatedAt))
                                         .OrderByDescending(d => d)
                                         .FirstOrDefault();
            ViewBag.Loans     = loans;
            ViewBag.Savings   = savings;
            ViewBag.CashFlows = cashFlows;

            return View();
        }

        // ── Secret dùng để tạo/xác minh token ───────────────────────
        // (trong production nên lấy từ IConfiguration)
        private const string TokenSecret = "FinanceSystem_ResetPwd_Secret_2024";

        private static string MakeResetToken(Guid userId, long ticks)
        {
            var raw = $"{userId}|{ticks}|{TokenSecret}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash);
        }

        // ── GET /Account/ForgotPassword ──────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // ── POST /Account/ForgotPassword ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập email hoặc tên đăng nhập.");
                return View();
            }

            var input = email.Trim();
            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == input.ToLower() || u.Username == input);

            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy tài khoản với email hoặc tên đăng nhập này.");
                return View();
            }

            // Token = HMAC(userId|ticks) — không cần lưu server-side
            var ticks = DateTime.UtcNow.Ticks;
            var token = MakeResetToken(user.Id, ticks);

            return RedirectToAction("ResetPassword", new
            {
                uid = user.Id.ToString(),
                t   = ticks,
                tok = token
            });
        }

        // ── GET /Account/ResetPassword ────────────────────────────────
        [HttpGet]
        public IActionResult ResetPassword(string uid, long t, string tok)
        {
            if (!Guid.TryParse(uid, out var userId) || string.IsNullOrEmpty(tok))
            {
                TempData["Error"] = "Liên kết không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            // Kiểm tra token hợp lệ
            var expected = MakeResetToken(userId, t);
            if (!string.Equals(expected, tok, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Liên kết đặt lại mật khẩu không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            // Hết hạn sau 15 phút
            if (DateTime.UtcNow.Ticks - t > TimeSpan.FromMinutes(15).Ticks)
            {
                TempData["Error"] = "Liên kết đã hết hạn (15 phút). Vui lòng thử lại.";
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Uid = uid;
            ViewBag.T   = t;
            ViewBag.Tok = tok;
            return View();
        }

        // ── POST /Account/ResetPassword ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            string uid, long t, string tok,
            string newPassword, string confirmPassword)
        {
            if (!Guid.TryParse(uid, out var userId) || string.IsNullOrEmpty(tok))
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            var expected = MakeResetToken(userId, t);
            if (!string.Equals(expected, tok, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Liên kết đặt lại mật khẩu không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            if (DateTime.UtcNow.Ticks - t > TimeSpan.FromMinutes(15).Ticks)
            {
                TempData["Error"] = "Liên kết đã hết hạn (15 phút). Vui lòng thử lại.";
                return RedirectToAction("ForgotPassword");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
                ViewBag.Uid = uid; ViewBag.T = t; ViewBag.Tok = tok;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                ViewBag.Uid = uid; ViewBag.T = t; ViewBag.Tok = tok;
                return View();
            }

            var user = await _db.AppUsers.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("ForgotPassword");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ── Helper: Hash mật khẩu bằng SHA-256 ──────────────────────
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash  = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
