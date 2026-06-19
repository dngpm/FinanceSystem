using FinanceSystem.Models.Entities;

namespace FinanceSystem.Models.Services
{
    public static class CashFlowService
    {
        /// <summary>
        /// Nhóm các bút toán thành 3 dòng tiền theo kỳ
        /// Trả về Dictionary: Period → (OP, INV, FIN, NET)
        /// </summary>
        public static Dictionary<int, PeriodCashFlowSummary> GroupByPeriod(
            List<CashFlowEntry> entries)
        {
            var result = new Dictionary<int, PeriodCashFlowSummary>();

            foreach (var entry in entries)
            {
                if (!result.ContainsKey(entry.Period))
                    result[entry.Period] = new PeriodCashFlowSummary { Period = entry.Period };

                var summary = result[entry.Period];
                switch (entry.Type)
                {
                    case CashFlowType.Operating:
                        summary.OperatingCF += entry.Amount;
                        break;
                    case CashFlowType.Investing:
                        summary.InvestingCF += entry.Amount;
                        break;
                    case CashFlowType.Financing:
                        summary.FinancingCF += entry.Amount;
                        break;
                }
            }

            decimal runningBalance = 0;
            foreach (var key in result.Keys.OrderBy(k => k))
            {
                var s = result[key];
                s.NetCashFlow = s.OperatingCF + s.InvestingCF + s.FinancingCF;
                runningBalance += s.NetCashFlow;
                s.CumulativeBalance = runningBalance;
                s.IsDeficit = s.CumulativeBalance < 0;
            }

            return result;
        }

        public static List<int> GetDeficitPeriods(
            Dictionary<int, PeriodCashFlowSummary> summary)
        {
            return summary.Values
                .Where(s => s.IsDeficit)
                .Select(s => s.Period)
                .OrderBy(p => p)
                .ToList();
        }

        public static CashFlowTotals CalculateTotals(List<CashFlowEntry> entries)
        {
            return new CashFlowTotals
            {
                TotalOperating = entries.Where(e => e.Type == CashFlowType.Operating).Sum(e => e.Amount),
                TotalInvesting = entries.Where(e => e.Type == CashFlowType.Investing).Sum(e => e.Amount),
                TotalFinancing = entries.Where(e => e.Type == CashFlowType.Financing).Sum(e => e.Amount)
            };
        }

        /// <summary>
        /// Ghép dòng tiền tiết kiệm và vay thành dashboard dòng tiền tổng hợp.
        /// SavingsRow được xem là inflow từ lãi nhận được hoặc tiền tất toán.
        /// AmortizationRow được xem là outflow từ tiền trả nợ vay.
        /// </summary>
        public static CombinedCashFlowViewModel BuildCombinedCashFlow(
            List<SavingsRow> savingsSchedule,
            List<AmortizationRow> loanSchedule,
            decimal initialCashBalance)
        {
            savingsSchedule ??= new List<SavingsRow>();
            loanSchedule ??= new List<AmortizationRow>();

            int maxPeriod = Math.Max(
                savingsSchedule.Any() ? savingsSchedule.Max(x => x.Period) : 0,
                loanSchedule.Any() ? loanSchedule.Max(x => x.Period) : 0
            );

            var rows = new List<CombinedCashFlowRow>();
            decimal runningBalance = initialCashBalance;

            for (int period = 1; period <= maxPeriod; period++)
            {
                decimal openingBalance = runningBalance;

                decimal savingsInflow = savingsSchedule
                    .Where(x => x.Period == period)
                    .Sum(x => x.InterestPaid + (x.IsEarlyWithdraw ? x.ClosingBalance : 0));

                decimal loanOutflow = loanSchedule
                    .Where(x => x.Period == period)
                    .Sum(x => x.PrincipalPayment + x.InterestPayment + x.EarlyRepaymentPenalty);

                decimal netCashFlow = savingsInflow - loanOutflow;
                runningBalance += netCashFlow;

                rows.Add(new CombinedCashFlowRow
                {
                    Period = period,
                    PeriodLabel = $"Kỳ {period}",
                    OpeningCashBalance = openingBalance,
                    SavingsInflow = savingsInflow,
                    LoanOutflow = loanOutflow,
                    NetCashFlow = netCashFlow,
                    ClosingCashBalance = runningBalance,
                    IsDeficit = runningBalance < 0
                });
            }

            return new CombinedCashFlowViewModel
            {
                InitialCashBalance = initialCashBalance,
                Rows = rows,
                TotalSavingsInflow = rows.Sum(x => x.SavingsInflow),
                TotalLoanOutflow = rows.Sum(x => x.LoanOutflow),
                FinalCashBalance = runningBalance,
                HasDeficit = rows.Any(x => x.IsDeficit)
            };
        }
    }

    public class PeriodCashFlowSummary
    {
        public int Period { get; set; }
        public decimal OperatingCF { get; set; }
        public decimal InvestingCF { get; set; }
        public decimal FinancingCF { get; set; }
        public decimal NetCashFlow { get; set; }
        public decimal CumulativeBalance { get; set; }
        public bool IsDeficit { get; set; }
    }

    public class CashFlowTotals
    {
        public decimal TotalOperating { get; set; }
        public decimal TotalInvesting { get; set; }
        public decimal TotalFinancing { get; set; }
        public decimal NetTotal => TotalOperating + TotalInvesting + TotalFinancing;
    }

    public class CombinedCashFlowViewModel
    {
        public decimal InitialCashBalance { get; set; }
        public decimal TotalSavingsInflow { get; set; }
        public decimal TotalLoanOutflow { get; set; }
        public decimal NetCashFlow => TotalSavingsInflow - TotalLoanOutflow;
        public decimal FinalCashBalance { get; set; }
        public bool HasDeficit { get; set; }
        public List<CombinedCashFlowRow> Rows { get; set; } = new();
    }

    public class CombinedCashFlowRow
    {
        public int Period { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;

        public decimal OpeningCashBalance { get; set; }

        public decimal SavingsInflow { get; set; }
        public decimal LoanOutflow { get; set; }

        public decimal NetCashFlow { get; set; }

        public decimal ClosingCashBalance { get; set; }

        public bool IsDeficit { get; set; }
    }
}