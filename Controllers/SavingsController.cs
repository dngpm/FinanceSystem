using FinanceSystem.Data;
using FinanceSystem.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceSystem.Controllers
{
    /// <summary>
    /// Controller xử lý gửi tiết kiệm
    /// Route: /Savings
    /// </summary>
    public class SavingsController : Controller
    {
        private readonly FinanceDbContext _db;

        public SavingsController(FinanceDbContext db)
        {
            _db = db;
        }

        // ── GET /Savings ─────────────────────────────────────────────
        // Hiển thị form nhập thông tin gửi tiết kiệm
        [HttpGet]
        public IActionResult Index()
        {
            return View(new SavingsInfo
            {
                DepositDate = DateTime.Today,
                TermMonths  = 12
            });
        }

        // ── POST /Savings/Calculate ──────────────────────────────────
        // Tính lịch nhận lãi + kết quả tổng hợp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(SavingsInfo info)
        {
            if (!ModelState.IsValid)
                return View("Index", info);

            // Bước 1: Tính lịch nhận lãi từng kỳ
            var schedule = InterestCalculator.CalculateSavingsSchedule(info);

			// Bước 2: Tính tổng lãi và thực nhận
			// Bước 2: Tính tổng lãi và thực nhận
			decimal totalInterest = schedule.Sum(r => r.InterestEarned);
			decimal actualReceived = info.Principal + totalInterest;

			// Bước 3: Tính EIR (lãi suất thực tế/năm)
			decimal eir = InterestCalculator.CalculateEffectiveRate(
                info.AnnualInterestRate,
                periodsPerYear: info.PaymentType switch
                {
                    InterestPaymentType.Monthly   => 12,
                    InterestPaymentType.Quarterly =>  4,
                    _                             =>  1
                }
            );

            // Bước 4: Xử lý rút trước hạn (nếu có)
            bool   isEarly  = info.EarlyWithdrawAtMonth.HasValue;
            decimal earlyPenalty = 0m;
            decimal earlyAmount  = 0m;

            if (isEarly && info.EarlyWithdrawAtMonth.HasValue)
            {
                earlyPenalty = InterestCalculator.CalculateEarlyWithdrawPenalty(
                    info, info.EarlyWithdrawAtMonth.Value);
                earlyAmount = InterestCalculator.CalculateEarlyWithdraw(
                    info, info.EarlyWithdrawAtMonth.Value);
            }

            // Bước 5: Đóng gói ViewModel
            var vm = new SavingsResultViewModel
            {
                Principal             = info.Principal,
                AnnualInterestRate    = info.AnnualInterestRate,
                TermMonths            = info.TermMonths,
                PaymentType           = info.PaymentType,
                TotalInterest         = totalInterest,
                ActualAmountReceived  = actualReceived,
                EffectiveAnnualRate   = eir,
                IsEarlyWithdraw       = isEarly,
                EarlyWithdrawPenalty  = earlyPenalty,
                EarlyWithdrawAmount   = earlyAmount,
                Schedule              = schedule
            };

            // Bước 6: Lưu SavingsInfo vào DB
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim != null && idClaim != "guest" && Guid.TryParse(idClaim, out var userId))
                info.UserId = userId;
            info.CalculatedAt = DateTime.Now;
            _db.SavingsInfos.Add(info);
            _db.SaveChanges();

            return View("Result", vm);
        }

        // ── GET /Savings/History ─────────────────────────────────────
        // Danh sách lịch sử các lần gửi tiết kiệm
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var list = await _db.SavingsInfos
                .OrderByDescending(s => s.DepositDate)
                .Take(20)
                .ToListAsync();

            return View(list);
        }

        // ── GET /Savings/Detail/{id} ─────────────────────────────────
        // Xem lại kết quả 1 lần gửi tiết kiệm
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var info = await _db.SavingsInfos
                .FirstOrDefaultAsync(s => s.Id == id);

            if (info == null) return NotFound();

            // Tái tạo kết quả từ thông tin đã lưu
            var schedule      = InterestCalculator.CalculateSavingsSchedule(info);
            decimal totalInt  = schedule.Sum(r => r.InterestEarned);
            decimal eir       = InterestCalculator.CalculateEffectiveRate(
                info.AnnualInterestRate,
                periodsPerYear: info.PaymentType switch
                {
                    InterestPaymentType.Monthly   => 12,
                    InterestPaymentType.Quarterly =>  4,
                    _                             =>  1
                }
            );

            bool    isEarly      = info.EarlyWithdrawAtMonth.HasValue;
            decimal earlyPenalty = 0m;
            decimal earlyAmount  = 0m;

            if (isEarly && info.EarlyWithdrawAtMonth.HasValue)
            {
                earlyPenalty = InterestCalculator.CalculateEarlyWithdrawPenalty(
                    info, info.EarlyWithdrawAtMonth.Value);
                earlyAmount = InterestCalculator.CalculateEarlyWithdraw(
                    info, info.EarlyWithdrawAtMonth.Value);
            }

            var vm = new SavingsResultViewModel
            {
                Principal            = info.Principal,
                AnnualInterestRate   = info.AnnualInterestRate,
                TermMonths           = info.TermMonths,
                PaymentType          = info.PaymentType,
                TotalInterest        = totalInt,
                ActualAmountReceived = info.Principal + totalInt,
                EffectiveAnnualRate  = eir,
                IsEarlyWithdraw      = isEarly,
                EarlyWithdrawPenalty = earlyPenalty,
                EarlyWithdrawAmount  = earlyAmount,
                Schedule             = schedule
            };

            return View("Result", vm);
        }
    }
}
