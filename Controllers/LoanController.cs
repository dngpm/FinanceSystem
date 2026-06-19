using FinanceSystem.Data;
using FinanceSystem.Models.Entities;
using FinanceSystem.Models.Services;
using FinanceSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.Controllers
{
    /// <summary>
    /// Controller xử lý tính lãi suất và lịch trả nợ
    /// Route: /Loan
    /// </summary>
    public class LoanController : Controller
    {
        private readonly FinanceDbContext _db;
        public LoanController(FinanceDbContext db) { _db = db; }
        // ─────────────────────────────────────────────
        // GET /Loan  →  Hiển thị form nhập khoản vay
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult Index()
        {
            // Truyền object rỗng để Razor có model type-safe
            return View(new LoanInfo
            {
                TermCount = 24,
                TermUnit = TermUnit.Monthly
            }
);
        }

        // ─────────────────────────────────────────────
        // POST /Loan/Calculate  →  Tính toán và trả kết quả
        // ─────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(LoanInfo loan)
        {
            NormalizeRateInput(loan);

            // ── Bước 1: Validate nghiệp vụ tài chính ──
            if (!ValidateLoanInput(loan))
                return View("Index", loan); // Trả về form kèm lỗi

            // ── Bước 2: Chọn phương thức và dựng lịch trả nợ ──
            var schedule = loan.Method switch
            {
                RepaymentMethod.PrincipalEqual => InterestCalculator.BuildPrincipalEqualSchedule(loan),
                RepaymentMethod.AnnuityEqual => InterestCalculator.BuildAnnuitySchedule(loan),
                RepaymentMethod.InterestOnlyThenPrincipal => InterestCalculator.BuildInterestOnlySchedule(loan),
                _ => throw new ArgumentOutOfRangeException()
            };

            // ── Bước 3: Tính EIR (Lãi suất thực tế) ──
            decimal eir = InterestCalculator.CalculateEIR(loan, schedule);

            // ── Bước 4: Lưu kết quả vào DB ──
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim != null && idClaim != "guest" && Guid.TryParse(idClaim, out var userId))
            {
                loan.UserId = userId;
            }
            loan.CalculatedAt = DateTime.Now;
            _db.LoanInfos.Add(loan);
            await _db.SaveChangesAsync();

            // ── Bước 5: Đóng gói ViewModel và trả về View kết quả ──
            var vm = new LoanResultViewModel
            {
                InputLoan = loan,
                Schedule = schedule,
                EIR = eir
            };

            return View("Result", vm);
        }

        // ─────────────────────────────────────────────
        // GET /Loan/Detail/{id}  →  Xem lại kết quả đã lưu
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var loan = await _db.LoanInfos.FindAsync(id);
            if (loan == null) return NotFound();

            var schedule = loan.Method switch
            {
                RepaymentMethod.PrincipalEqual => InterestCalculator.BuildPrincipalEqualSchedule(loan),
                RepaymentMethod.AnnuityEqual => InterestCalculator.BuildAnnuitySchedule(loan),
                RepaymentMethod.InterestOnlyThenPrincipal => InterestCalculator.BuildInterestOnlySchedule(loan),
                _ => throw new ArgumentOutOfRangeException()
            };
            decimal eir = InterestCalculator.CalculateEIR(loan, schedule);

            var vm = new LoanResultViewModel { InputLoan = loan, Schedule = schedule, EIR = eir };
            return View("Result", vm);
        }

        // ─────────────────────────────────────────────
        // GET /Loan/Compare  →  So sánh 3 phương thức trả nợ
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult Compare()
        {
            return View(new LoanInfo());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Compare(LoanInfo loan)
        {
            NormalizeRateInput(loan);

            if (!ValidateLoanInput(loan))
                return View(loan); ;

            // Tính lịch trả nợ cho cả 3 phương thức để so sánh
            var loanCopy = CloneLoan(loan);

            loanCopy.Method = RepaymentMethod.PrincipalEqual;
            var schedPrincipalEqual = InterestCalculator.BuildPrincipalEqualSchedule(loanCopy);

            loanCopy.Method = RepaymentMethod.AnnuityEqual;
            var schedAnnuity = InterestCalculator.BuildAnnuitySchedule(loanCopy);

            loanCopy.Method = RepaymentMethod.InterestOnlyThenPrincipal;
            var schedInterestOnly = InterestCalculator.BuildInterestOnlySchedule(loanCopy);

            // Truyền cả 3 kết quả qua ViewBag (view đơn giản, không cần ViewModel riêng)
            ViewBag.InputLoan = loan;
            ViewBag.PrincipalEqual = new LoanResultViewModel
            {
                InputLoan = CloneLoan(loan, RepaymentMethod.PrincipalEqual),
                Schedule = schedPrincipalEqual,
                EIR = InterestCalculator.CalculateEIR(loan, schedPrincipalEqual)
            };
            ViewBag.Annuity = new LoanResultViewModel
            {
                InputLoan = CloneLoan(loan, RepaymentMethod.AnnuityEqual),
                Schedule = schedAnnuity,
                EIR = InterestCalculator.CalculateEIR(loan, schedAnnuity)
            };
            ViewBag.InterestOnly = new LoanResultViewModel
            {
                InputLoan = CloneLoan(loan, RepaymentMethod.InterestOnlyThenPrincipal),
                Schedule = schedInterestOnly,
                EIR = InterestCalculator.CalculateEIR(loan, schedInterestOnly)
            };

            return View("CompareResult");
        }

        // ─────────────────────────────────────────────
        // GET /Loan/SimpleCompound  →  Lãi đơn vs Lãi kép
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult SimpleCompound()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SimpleCompound(decimal principal, decimal annualRate,
    int termCount, TermUnit termUnit)
        {
            if (principal <= 0 || annualRate <= 0 || termCount <= 0)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ và hợp lệ thông tin.");
                return View();
            }

            string unitLabel = InterestCalculator.GetUnitLabel(termUnit);

            ViewBag.Principal = principal;
            ViewBag.AnnualRate = annualRate;
            ViewBag.TermCount = termCount;
            ViewBag.TermUnit = termUnit;
            ViewBag.UnitLabel = unitLabel;
            ViewBag.SimpleInterest = InterestCalculator.SimpleInterest(principal, annualRate, termCount, termUnit);
            ViewBag.CompoundFV = InterestCalculator.CompoundFutureValue(principal, annualRate, termCount, termUnit);

            // Dữ liệu chart so sánh từng kỳ
            var fvByPeriod = Enumerable.Range(1, termCount)
                .Select(t => new {
                    Label = InterestCalculator.GetPeriodLabel(t, termUnit),
                    Simple = principal + InterestCalculator.SimpleInterest(principal, annualRate, t, termUnit),
                    Compound = InterestCalculator.CompoundFutureValue(principal, annualRate, t, termUnit)
                }).ToList();

            ViewBag.ChartLabels = string.Join(",", fvByPeriod.Select(x => $"\"{x.Label}\""));
            ViewBag.SimpleData = string.Join(",", fvByPeriod.Select(x => x.Simple.ToString("F0")));
            ViewBag.CompoundData = string.Join(",", fvByPeriod.Select(x => x.Compound.ToString("F0")));


            return View("SimpleCompoundResult");
        }

        // ─────────────────────────────────────────────
        // Helpers nội bộ
        // ─────────────────────────────────────────────

        private bool ValidateLoanInput(LoanInfo loan)
        {
            if (!FinancialHelper.IsValidPrincipal(loan.Principal))
                ModelState.AddModelError(nameof(loan.Principal),
                    "Số tiền vay phải từ 1 đến 100 tỷ đồng.");

            if (!FinancialHelper.IsValidRate(loan.AnnualInterestRate))
                ModelState.AddModelError(nameof(loan.AnnualInterestRate),
                    "Lãi suất phải trong khoảng 0% – 100%/năm.");

            if (!FinancialHelper.IsValidTermCount(loan.TermCount, loan.TermUnit))
                ModelState.AddModelError(nameof(loan.TermCount),
                    FinancialHelper.GetTermErrorMessage(loan.TermUnit));

            if (loan.IsEarlyRepayment)
            {
                if (!loan.EarlyRepaymentPeriod.HasValue || loan.EarlyRepaymentPeriod <= 0)
                    ModelState.AddModelError(nameof(loan.EarlyRepaymentPeriod),
                        $"Vui lòng nhập {InterestCalculator.GetUnitLabel(loan.TermUnit)} tất toán.");
                else if (loan.EarlyRepaymentPeriod >= loan.TermMonths)
                    ModelState.AddModelError(nameof(loan.EarlyRepaymentPeriod),
                        $"Kỳ tất toán phải nhỏ hơn tổng số {InterestCalculator.GetUnitLabel(loan.TermUnit)} vay ({loan.TermCount}).");
            }

            return ModelState.IsValid;
        }

        private static void NormalizeRateInput(LoanInfo loan)
        {
            // Người dùng thường nhập 8 để chỉ 8%/năm; engine tài chính dùng 0.08.
            if (loan.AnnualInterestRate > 1m)
                loan.AnnualInterestRate /= 100m;
        }

        private static LoanInfo CloneLoan(LoanInfo src, RepaymentMethod? method = null) => new()
        {
            Principal = src.Principal,
            AnnualInterestRate = src.AnnualInterestRate,
            TermCount = src.TermCount,
            TermUnit = src.TermUnit,
            Method = method ?? src.Method,
            IsEarlyRepayment = src.IsEarlyRepayment,
            EarlyRepaymentPeriod = src.EarlyRepaymentPeriod,
            EarlyRepaymentPenaltyRate = src.EarlyRepaymentPenaltyRate,
            ServiceFee = src.ServiceFee
        };
    }
}