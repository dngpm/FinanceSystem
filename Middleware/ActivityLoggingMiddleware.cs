using FinanceSystem.Data;
using FinanceSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Middleware
{
    /// <summary>
    /// Middleware ghi lại lịch sử hoạt động của người dùng đã đăng nhập.
    /// Chỉ ghi các request GET (xem trang) và POST quan trọng (hành động).
    /// Bỏ qua static files, ajax polling, và các route không cần thiết.
    /// </summary>
    public class ActivityLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        // Các path prefix cần bỏ qua (static files, favicon...)
        private static readonly string[] _skipPrefixes =
        {
            "/css/", "/js/", "/lib/", "/images/", "/fonts/",
            "/favicon", "/_framework", "/_vs", "/.well-known"
        };

        // Các action chỉ ghi khi là POST (tránh log GET lặp lại)
        private static readonly HashSet<string> _postOnlyActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Calculate", "Save", "Delete", "Add", "Edit",
            "Submit", "ReviewRequest", "ToggleLock", "PromoteToAdmin",
            "RevokeAdmin", "ChangePassword", "Logout", "Register",
            "OverdraftCalc"
        };

        public ActivityLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, FinanceDbContext db)
        {
            // Bỏ qua nếu chưa đăng nhập
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? "";

            // Bỏ qua static files
            if (_skipPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Bỏ qua request không phải GET/POST
            var method = context.Request.Method;
            if (method != "GET" && method != "POST")
            {
                await _next(context);
                return;
            }

            // Lấy thông tin route
            await _next(context);

            // Chỉ log sau khi response thành công (2xx, 3xx)
            var statusCode = context.Response.StatusCode;
            if (statusCode < 200 || statusCode >= 400) return;

            try
            {
                var routeData = context.GetRouteData();
                var controller = routeData?.Values["controller"]?.ToString() ?? "";
                var action     = routeData?.Values["action"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(controller) || string.IsNullOrEmpty(action)) return;

                // Bỏ qua Login GET (chỉ ghi POST login)
                if (controller == "Account" && action == "Login" && method == "GET") return;

                // Với action "nặng" chỉ ghi khi POST
                if (_postOnlyActions.Contains(action) && method == "GET") return;

                var username = context.User.Identity.Name ?? "";

                // Tìm UserId
                var user = await db.AppUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == username);

                var (module, actionLabel) = ActivityModuleMapper.Resolve(controller, action);

                var log = new UserActivityLog
                {
                    Id         = Guid.NewGuid(),
                    UserId     = user?.Id,
                    Username   = username,
                    Module     = module,
                    Action     = actionLabel,
                    Path       = path,
                    HttpMethod = method,
                    Timestamp  = DateTime.Now
                };

                db.UserActivityLogs.Add(log);
                await db.SaveChangesAsync();
            }
            catch
            {
                // Không để lỗi log làm crash app
            }
        }
    }

    public static class ActivityLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseActivityLogging(this IApplicationBuilder app)
            => app.UseMiddleware<ActivityLoggingMiddleware>();
    }
}
