using FinanceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FinanceSystem.Controllers
{
    /// <summary>
    /// Controller trang chủ — Dashboard tổng quan hệ thống
    /// Route: /  hoặc  /Home
    /// </summary>
    public class HomeController : Controller
    {
        // ─────────────────────────────────────────────
        // GET /  →  Trang chủ / Landing page
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult Index()
        {
            // Truyền thống kê nhanh để hiển thị trên dashboard
            ViewBag.QuickStats = new
            {
                MaxLoanAmount = "100 tỷ đồng",
                MaxLoanTerm = "30 năm (360 tháng)",
                MaxCashFlowTerms = "120 kỳ",
                SupportedMethods = "Gốc đều · Annuity · Trả lãi định kỳ"
            };

            return View();
        }

        // ─────────────────────────────────────────────
        // GET /Home/About  →  Giới thiệu hệ thống
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        // ─────────────────────────────────────────────
        // GET /Home/Guide  →  Hướng dẫn sử dụng
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult Guide()
        {
            return View();
        }

        // ─────────────────────────────────────────────
        // GET /Home/Privacy
        // ─────────────────────────────────────────────

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        // ─────────────────────────────────────────────
        // Error handler
        // ─────────────────────────────────────────────

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}