using FinanceSystem.Models.Entities;

namespace FinanceSystem.Models.Services
{
    /// <summary>
    /// Engine thẩm định dự án đầu tư
    /// Tính NPV, IRR, Payback Period (thường và có chiết khấu)
    /// </summary>
    public static class InvestmentAnalyzer
    {
        // ─────────────────────────────────────────────
        // 1. NPV — Net Present Value (Giá trị hiện tại thuần)
        // NPV = -CF0 + Σ [ CFt / (1+r)^t ]
        // Nếu NPV > 0 → Nên đầu tư | NPV < 0 → Từ chối
        // ─────────────────────────────────────────────

        public static decimal CalculateNPV(InvestmentProject project)
        {
            double r = (double)project.DiscountRate;
            double npv = -(double)project.InitialInvestment;

            for (int t = 0; t < project.CashFlows.Count; t++)
            {
                npv += (double)project.CashFlows[t] / Math.Pow(1 + r, t + 1);
            }

            return Math.Round((decimal)npv, 0);
        }

        // ─────────────────────────────────────────────
        // 2. IRR — Internal Rate of Return
        // Là r* sao cho NPV(r*) = 0
        // Dùng Newton-Raphson (tái sử dụng từ InterestCalculator)
        // Nếu IRR > DiscountRate → Nên đầu tư
        // ─────────────────────────────────────────────

        public static decimal CalculateIRR(InvestmentProject project)
        {
            // Xây dựng chuỗi cash flow đầy đủ: CF0 âm, CF1..N dương
            var flows = new List<double> { -(double)project.InitialInvestment };
            flows.AddRange(project.CashFlows.Select(x => (double)x));

            double irr = InterestCalculator.SolveIRR(flows, guess: 0.1);
            return Math.Round((decimal)irr, 6); // dạng 0.xxxxxx (IRR/kỳ tương ứng 1 năm)
        }

        // ─────────────────────────────────────────────
        // 3. Payback Period — Thời gian hoàn vốn thông thường
        // Kỳ mà tổng CF tích lũy >= vốn đầu tư ban đầu
        // Trả về số kỳ (thập phân), -1 nếu không hoàn vốn trong vòng đời
        // ─────────────────────────────────────────────

        public static decimal CalculatePaybackPeriod(InvestmentProject project)
        {
            decimal cumulative = 0;
            for (int t = 0; t < project.CashFlows.Count; t++)
            {
                decimal prev = cumulative;
                cumulative += project.CashFlows[t];

                if (cumulative >= project.InitialInvestment)
                {
                    // Nội suy phần lẻ của kỳ
                    decimal remaining = project.InitialInvestment - prev;
                    decimal fraction = remaining / project.CashFlows[t];
                    return Math.Round(t + fraction, 2);
                }
            }
            return -1; // Không hoàn vốn trong vòng đời dự án
        }

        // ─────────────────────────────────────────────
        // 4. Discounted Payback Period — Hoàn vốn có chiết khấu
        // Tương tự Payback nhưng dùng PV của từng CF
        // ─────────────────────────────────────────────

        public static decimal CalculateDiscountedPaybackPeriod(InvestmentProject project)
        {
            double r = (double)project.DiscountRate;
            decimal cumPV = 0;

            for (int t = 0; t < project.CashFlows.Count; t++)
            {
                decimal prevPV = cumPV;
                decimal pv = (decimal)((double)project.CashFlows[t] / Math.Pow(1 + r, t + 1));
                cumPV += pv;

                if (cumPV >= project.InitialInvestment)
                {
                    decimal remaining = project.InitialInvestment - prevPV;
                    decimal fraction = remaining / pv;
                    return Math.Round(t + fraction, 2);
                }
            }
            return -1;
        }

        // ─────────────────────────────────────────────
        // 5. Khuyến nghị đầu tư tổng hợp
        // ─────────────────────────────────────────────

        public static string GetRecommendation(decimal npv, decimal irr, decimal discountRate)
        {
            bool npvPositive = npv > 0;
            bool irrAboveHurdle = irr > discountRate;

            if (npvPositive && irrAboveHurdle)
                return "✅ Nên đầu tư — NPV dương và IRR vượt ngưỡng chiết khấu.";
            if (npvPositive && !irrAboveHurdle)
                return "⚠️ Cân nhắc — NPV dương nhưng IRR chưa đủ hấp dẫn.";
            if (!npvPositive && irrAboveHurdle)
                return "⚠️ Cân nhắc — IRR tốt nhưng NPV âm do quy mô dự án.";

            return "❌ Từ chối — NPV âm và IRR thấp hơn tỷ suất chiết khấu yêu cầu.";
        }
    }
}
