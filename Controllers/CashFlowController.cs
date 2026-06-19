using FinanceSystem.Data;
using Microsoft.EntityFrameworkCore;
using FinanceSystem.Models.Entities;
using FinanceSystem.Models.Services;
using FinanceSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.Controllers
{
    public class CashFlowController : Controller
    {
        private readonly FinanceDbContext _db;

        public CashFlowController(FinanceDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(GetSampleEntries());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dashboard(List<CashFlowEntry> entries, decimal initialInvestment, decimal discountRate)
        {
            NormalizeRateInput(ref discountRate);

            entries = entries
                .Where(e => !string.IsNullOrWhiteSpace(e.Description) && e.Period > 0)
                .ToList();

            if (entries.Count == 0)
            {
                ModelState.AddModelError("", "Vui lòng nhập ít nhất một bút toán dòng tiền hợp lệ.");
                return View("Index", GetSampleEntries());
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Period < 1 || entries[i].Period > 120)
                    ModelState.AddModelError($"entries[{i}].Period",
                        $"Dòng {i + 1}: Kỳ phải từ 1 đến 120.");
            }

            if (initialInvestment < 0)
                ModelState.AddModelError(nameof(initialInvestment), "Vốn đầu tư ban đầu không được âm.");

            if (discountRate <= 0 || discountRate > 1)
                ModelState.AddModelError(nameof(discountRate), "Lãi suất chiết khấu phải trong khoảng 0% đến 100%.");

            if (!ModelState.IsValid)
                return View("Index", entries);

            var grouped = CashFlowService.GroupByPeriod(entries);
            var totals = CashFlowService.CalculateTotals(entries);
            var deficits = CashFlowService.GetDeficitPeriods(grouped);
            var netCashFlows = grouped.Values
                .OrderBy(s => s.Period)
                .Select(s => s.NetCashFlow)
                .ToList();

            var investmentProject = new InvestmentProject
            {
                ProjectName = "Phân tích dòng tiền",
                InitialInvestment = initialInvestment,
                DiscountRate = discountRate,
                CashFlows = netCashFlows
            };

            var presentValue = CalculatePresentValue(netCashFlows, discountRate);
            var futureValue = CalculateFutureValue(netCashFlows, discountRate);
            var npv = initialInvestment > 0
                ? InvestmentAnalyzer.CalculateNPV(investmentProject)
                : presentValue;
            var irr = initialInvestment > 0 && HasIrrShape(investmentProject)
                ? InvestmentAnalyzer.CalculateIRR(investmentProject)
                : 0m;

            var vm = new CashFlowSummaryViewModel
            {
                PeriodSummaries = grouped.Values.OrderBy(s => s.Period).ToList(),
                Totals = totals,
                DeficitPeriods = deficits,
                InitialInvestment = initialInvestment,
                DiscountRate = discountRate,
                PresentValue = presentValue,
                FutureValue = futureValue,
                NPV = npv,
                IRR = irr,
                Decision = BuildDecision(npv, irr, discountRate, totals.NetTotal, deficits.Any(), initialInvestment)
            };

            // ── Lưu lịch sử vào DB nếu người dùng đã đăng nhập ─────
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(idClaim) && idClaim != "guest" && Guid.TryParse(idClaim, out var userId))
            {
                decimal totalInflowAmt  = entries.Where(e => e.Amount > 0).Sum(e => e.Amount);
                decimal totalOutflowAmt = entries.Where(e => e.Amount < 0).Sum(e => Math.Abs(e.Amount));

                var history = new CashFlowHistory
                {
                    UserId            = userId,
                    CalculatedAt      = DateTime.Now,
                    InitialInvestment = initialInvestment,
                    DiscountRate      = discountRate,
                    EntryCount        = entries.Count,
                    PeriodCount       = entries.Max(e => e.Period),
                    TotalInflow       = totalInflowAmt,
                    TotalOutflow      = totalOutflowAmt,
                    NetCashFlow       = totals.NetTotal,
                    NPV               = npv,
                    IRR               = irr,
                    Decision          = vm.Decision,
                    HasDeficit        = deficits.Any()
                };
                _db.CashFlowHistories.Add(history);
                await _db.SaveChangesAsync();
            }

            return View("Dashboard", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
                return RedirectToAction("Login", "Account");

            var history = await _db.CashFlowHistories
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (history == null)
                return NotFound();

            return View(history);
        }

        [HttpGet]
        public IActionResult AddRow(int index)
        {
            ViewBag.Index = index;
            return PartialView("_CashFlowEntryRow", new CashFlowEntry { Period = 1 });
        }

        private static List<CashFlowEntry> GetSampleEntries() => new()
        {
            new() { Description = "Doanh thu bán hàng",       Amount =  500_000_000, Period = 1, Type = CashFlowType.Operating  },
            new() { Description = "Chi phí vận hành",          Amount = -200_000_000, Period = 1, Type = CashFlowType.Operating  },
            new() { Description = "Mua thiết bị máy móc",      Amount = -300_000_000, Period = 1, Type = CashFlowType.Investing  },
            new() { Description = "Vay ngân hàng",             Amount =  400_000_000, Period = 1, Type = CashFlowType.Financing  },
            new() { Description = "Doanh thu bán hàng (kỳ 2)", Amount =  600_000_000, Period = 2, Type = CashFlowType.Operating  },
            new() { Description = "Chi phí vận hành (kỳ 2)",   Amount = -250_000_000, Period = 2, Type = CashFlowType.Operating  },
            new() { Description = "Trả nợ gốc + lãi (kỳ 2)",  Amount = -150_000_000, Period = 2, Type = CashFlowType.Financing  },
        };

        private static void NormalizeRateInput(ref decimal discountRate)
        {
            if (discountRate > 1m)
                discountRate /= 100m;
        }

        private static decimal CalculatePresentValue(List<decimal> cashFlows, decimal discountRate)
        {
            double rate = (double)discountRate;
            double total = 0;

            for (int i = 0; i < cashFlows.Count; i++)
                total += (double)cashFlows[i] / Math.Pow(1 + rate, i + 1);

            return Math.Round((decimal)total, 0);
        }

        private static decimal CalculateFutureValue(List<decimal> cashFlows, decimal discountRate)
        {
            double rate = (double)discountRate;
            int periods = cashFlows.Count;
            double total = 0;

            for (int i = 0; i < periods; i++)
                total += (double)cashFlows[i] * Math.Pow(1 + rate, periods - i - 1);

            return Math.Round((decimal)total, 0);
        }

        private static bool HasIrrShape(InvestmentProject project)
        {
            var flows = new List<decimal> { -project.InitialInvestment };
            flows.AddRange(project.CashFlows);
            return flows.Any(f => f < 0) && flows.Any(f => f > 0);
        }

        private static string BuildDecision(decimal npv, decimal irr, decimal discountRate, decimal netTotal, bool hasDeficit, decimal initialInvestment)
        {
            if (hasDeficit)
                return "Dòng tiền có kỳ âm, cần bổ sung vốn lưu động hoặc giảm chi trước khi vay hay đầu tư.";

            if (initialInvestment > 0 && npv > 0 && irr > discountRate)
                return "Nên đầu tư: NPV dương và IRR cao hơn lãi suất chiết khấu.";

            if (netTotal > 0 && npv > 0)
                return "Nên ưu tiên tiết kiệm hoặc tái đầu tư phần tiền thặng dư.";

            if (netTotal < 0)
                return "Cần cân nhắc vay vốn hoặc cắt giảm chi phí vì dòng tiền thuần âm.";

            return "Phương án trung tính, nên so sánh thêm lãi vay và lãi tiết kiệm trước khi quyết định.";
        }
    }
}
