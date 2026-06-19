using FinanceSystem.Data;
using FinanceSystem.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ════════════════════════════════════════════════════════════════
// 1. ĐĂNG KÝ DBCONTEXT
// ════════════════════════════════════════════════════════════════
builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FinanceSystemDB")
    )
);

// ════════════════════════════════════════════════════════════════
// 2. ĐĂNG KÝ AUTHENTICATION (Cookie — dùng cho AccountController)
// ════════════════════════════════════════════════════════════════
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath    = "/Account/Login";
        options.LogoutPath   = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
    });

// ════════════════════════════════════════════════════════════════
// 3. ĐĂNG KÝ MVC
// ════════════════════════════════════════════════════════════════
builder.Services.AddControllersWithViews(options =>
{
    // Cho phép nhập số kiểu VN: "500.000.000" hoặc "500,000,000" → 500000000
    options.ModelBinderProviders.Insert(0, new FinanceSystem.Infrastructure.DecimalModelBinderProvider());
});

var app = builder.Build();

// ════════════════════════════════════════════════════════════════
// 4. CẤU HÌNH PIPELINE HTTP
// ════════════════════════════════════════════════════════════════
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // ← phải trước UseAuthorization
app.UseAuthorization();
app.UseActivityLogging(); // ← ghi log hoạt động người dùng

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ════════════════════════════════════════════════════════════════
// 5. TỰ ĐỘNG TẠO DATABASE
// ════════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
	db.Database.Migrate();

    // ── Seed Role Admin nếu chưa có ──────────────────────────────
    if (!db.Roles.Any(r => r.RoleName == "Admin"))
    {
        db.Roles.Add(new FinanceSystem.Models.Entities.Role
        {
            Id          = Guid.NewGuid(),
            RoleName    = "Admin",
            Description = "Quản trị viên hệ thống"
        });
        db.SaveChanges();
    }

    // ── Seed tài khoản admin mặc định nếu chưa có ────────────────
    // Username: admin  |  Password: Admin@123
    if (!db.AppUsers.Any(u => u.Username == "admin"))
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = Convert.ToHexString(
            sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Admin@123")));

        var adminUser = new FinanceSystem.Models.Entities.AppUser
        {
            Id           = Guid.NewGuid(),
            Username     = "admin",
            Email        = "admin@financesystem.local",
            PasswordHash = hash,
            CreatedAt    = DateTime.Now
        };
        db.AppUsers.Add(adminUser);
        db.SaveChanges();

        var adminRole = db.Roles.First(r => r.RoleName == "Admin");
        db.UserRoles.Add(new FinanceSystem.Models.Entities.UserRole
        {
            Id     = Guid.NewGuid(),
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        });
        db.SaveChanges();
    }
}

app.Run();
