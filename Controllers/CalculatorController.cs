using FinanceSystem.Data;
using FinanceSystem.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly FinanceDbContext _db;

        public CalculatorController(FinanceDbContext db)
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
    }
}
