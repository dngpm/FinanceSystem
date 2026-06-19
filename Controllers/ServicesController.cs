using FinanceSystem.Data;
using FinanceSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceSystem.Controllers
{
    public class ServicesController : Controller
    {
        private readonly FinanceDbContext _db;

        public ServicesController(FinanceDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Rates = await _db.InterestRateTables
                .Where(r => r.ProductType != FinancialProductType.OverdraftLoan)
                .OrderBy(r => r.RateType)
                .ThenBy(r => r.ProductType)
                .ThenBy(r => r.TermMonths)
                .ToListAsync();

            return View();
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> Apply(string type = ServiceRequestType.Loan, string product = FinancialProductType.UnsecuredLoan)
        {
            if (product == FinancialProductType.OverdraftLoan)
                return RedirectToAction("Index");

            var rate = await FindBestRate(type, product, 12);
            return View(new ServiceRequest
            {
                RequestType = type,
                ProductType = product,
                TermMonths = rate?.TermMonths ?? 12,
                AnnualRatePercent = rate?.AnnualRatePercent ?? 0
            });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ServiceRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Services") });

            if (request.ProductType == FinancialProductType.OverdraftLoan)
            {
                ModelState.AddModelError(nameof(ServiceRequest.ProductType), "Sản phẩm thấu chi không còn nằm trong phạm vi hệ thống.");
                return View(request);
            }

            var rate = await FindBestRate(request.RequestType, request.ProductType, request.TermMonths);
            request.Id = Guid.NewGuid();
            request.UserId = userId.Value;
            request.Status = ServiceRequestStatus.Pending;
            request.CreatedAt = DateTime.Now;
            request.ReviewedAt = null;
            request.ReviewedBy = string.Empty;
            request.AdminNote = string.Empty;
            request.AnnualRatePercent = rate?.AnnualRatePercent ?? request.AnnualRatePercent;

            ModelState.Remove(nameof(ServiceRequest.Id));
            ModelState.Remove(nameof(ServiceRequest.User));
            ModelState.Remove(nameof(ServiceRequest.UserId));
            ModelState.Remove(nameof(ServiceRequest.Status));
            ModelState.Remove(nameof(ServiceRequest.CreatedAt));
            ModelState.Remove(nameof(ServiceRequest.ReviewedAt));
            ModelState.Remove(nameof(ServiceRequest.ReviewedBy));
            ModelState.Remove(nameof(ServiceRequest.AdminNote));

            if (!ModelState.IsValid)
                return View(request);

            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Yêu cầu của bạn đã được gửi. Admin sẽ xem xét và phản hồi.";
            return RedirectToAction("MyRequests");
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var requests = await _db.ServiceRequests
                .Where(r => r.UserId == userId.Value)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        private Guid? GetCurrentUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }

        private Task<InterestRateTable?> FindBestRate(string type, string product, int termMonths)
        {
            return _db.InterestRateTables
                .Where(r => r.RateType == type && r.ProductType == product)
                .OrderBy(r => Math.Abs(r.TermMonths - termMonths))
                .FirstOrDefaultAsync();
        }
    }
}
