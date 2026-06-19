using FinanceSystem.Models.Entities;

namespace FinanceSystem.Models.Services
{
    /// <summary>
    /// Engine tính toán lãi suất và lịch trả nợ.
    /// Hỗ trợ 3 đơn vị kỳ hạn: Tháng / Quý / Năm.
    ///
    /// Nguyên lý chuyển đổi lãi suất:
    ///   Lãi suất đầu vào luôn là %/NĂM (AnnualInterestRate).
    ///   Tuỳ TermUnit, Service tự suy ra lãi suất/kỳ:
    ///     Monthly   → r_kỳ = r_năm / 12
    ///     Quarterly → r_kỳ = (1 + r_năm)^(1/4) - 1   (lãi kép tương đương)
    ///     Yearly    → r_kỳ = r_năm
    /// </summary>
    public static class InterestCalculator
    {
        // ═════════════════════════════════════════════════════════════════════
        // PHẦN 1 — TIỆN ÍCH CHUYỂN ĐỔI LÃI SUẤT & KỲ HẠN
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tính lãi suất/kỳ tương đương từ lãi suất năm theo đơn vị kỳ hạn.
        /// Dùng công thức lãi kép tương đương để đảm bảo nhất quán.
        ///   Monthly:   r_kỳ = r_năm / 12
        ///   Quarterly: r_kỳ = (1 + r_năm)^(3/12) - 1
        ///   Yearly:    r_kỳ = r_năm
        /// </summary>
        public static decimal GetPeriodRate(decimal annualRate, TermUnit unit) => unit switch
        {
            TermUnit.Monthly => annualRate / 12m,
            TermUnit.Quarterly => (decimal)(Math.Pow((double)(1 + annualRate), 3.0 / 12.0) - 1),
            TermUnit.Yearly => annualRate,
            _ => annualRate / 12m
        };

        /// <summary>
        /// Nhãn hiển thị tên kỳ (dùng trong View khi render header bảng).
        /// Ví dụ: Period = 3, TermUnit = Quarterly → "Quý 3"
        /// </summary>
        public static string GetPeriodLabel(int period, TermUnit unit) => unit switch
        {
            TermUnit.Monthly => $"Tháng {period}",
            TermUnit.Quarterly => $"Quý {period}",
            TermUnit.Yearly => $"Năm {period}",
            _ => $"Kỳ {period}"
        };

        /// <summary>
        /// Tên đơn vị kỳ hạn dạng văn bản ngắn gọn.
        /// </summary>
        public static string GetUnitLabel(TermUnit unit) => unit switch
        {
            TermUnit.Monthly => "tháng",
            TermUnit.Quarterly => "quý",
            TermUnit.Yearly => "năm",
            _ => "kỳ"
        };

        // ═════════════════════════════════════════════════════════════════════
        // PHẦN 2 — LÃI ĐƠN / LÃI KÉP (SimpleInterest / CompoundFV)
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tính tổng lãi đơn sau T kỳ.
        /// Công thức: I = P × r_kỳ × T
        /// </summary>
        public static decimal SimpleInterest(decimal principal, decimal annualRate,
            int termCount, TermUnit unit = TermUnit.Monthly)
        {
            decimal rPeriod = GetPeriodRate(annualRate, unit);
            return Math.Round(principal * rPeriod * termCount, 0);
        }

        /// <summary>
        /// Tính giá trị tương lai với lãi kép ghép theo kỳ hạn.
        /// Công thức: FV = P × (1 + r_kỳ)^T
        /// </summary>
        public static decimal CompoundFutureValue(decimal principal, decimal annualRate,
            int termCount, TermUnit unit = TermUnit.Monthly)
        {
            decimal rPeriod = GetPeriodRate(annualRate, unit);
            decimal fv = principal * (decimal)Math.Pow((double)(1 + rPeriod), termCount);
            return Math.Round(fv, 0);
        }

        // ═════════════════════════════════════════════════════════════════════
        // PHẦN 3 — LỊCH TRẢ NỢ (3 PHƯƠNG THỨC × 3 ĐƠN VỊ KỲ HẠN)
        // ═════════════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────────────────────────────
        // 3A. Gốc đều (Principal Equal)
        //     Mỗi kỳ trả gốc bằng nhau = P/N
        //     Lãi kỳ t = Dư nợ đầu kỳ × r_kỳ
        //     Tổng trả giảm dần qua từng kỳ
        // ─────────────────────────────────────────────────────────────────────

        public static List<AmortizationRow> BuildPrincipalEqualSchedule(LoanInfo loan)
        {
            var schedule = new List<AmortizationRow>();
            decimal r = GetPeriodRate(loan.AnnualInterestRate, loan.TermUnit);
            int n = loan.TermCount;                          // số kỳ theo TermUnit
            decimal pPer = Math.Round(loan.Principal / n, 0);      // gốc mỗi kỳ
            decimal bal = loan.Principal;

            // Tính kỳ tất toán quy về kỳ theo TermUnit (không phải tháng)
            int? earlyPeriod = loan.IsEarlyRepayment ? loan.EarlyRepaymentPeriod : null;

            for (int t = 1; t <= n; t++)
            {
                decimal interest = Math.Round(bal * r, 0);
                decimal principal = (t == n) ? bal : pPer;   // kỳ cuối: trả hết dư nợ

                var row = new AmortizationRow
                {
                    Period = t,
                    PeriodLabel = GetPeriodLabel(t, loan.TermUnit),
                    OpeningBalance = bal,
                    PrincipalPayment = principal,
                    InterestPayment = interest,
                    ClosingBalance = bal - principal
                };

                // Xử lý tất toán trước hạn
                if (earlyPeriod.HasValue && t == earlyPeriod.Value)
                {
                    row.IsEarlyRepayment = true;
                    row.EarlyRepaymentPenalty = Math.Round(
                        row.ClosingBalance * loan.EarlyRepaymentPenaltyRate, 0);
                    row.PrincipalPayment += row.ClosingBalance; // trả toàn bộ dư nợ còn lại
                    row.ClosingBalance = 0;
                    schedule.Add(row);
                    break;
                }

                bal = row.ClosingBalance;
                schedule.Add(row);
            }

            return schedule;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3B. Annuity (Gốc + Lãi đều)
        //     PMT = P × r_kỳ / (1 - (1 + r_kỳ)^-N)
        //     Tổng trả mỗi kỳ bằng nhau, tỷ lệ gốc/lãi thay đổi qua từng kỳ
        // ─────────────────────────────────────────────────────────────────────

        public static List<AmortizationRow> BuildAnnuitySchedule(LoanInfo loan)
        {
            var schedule = new List<AmortizationRow>();
            decimal r = GetPeriodRate(loan.AnnualInterestRate, loan.TermUnit);
            int n = loan.TermCount;
            decimal pmt = CalculatePMT(loan.Principal, r, n);
            decimal bal = loan.Principal;

            int? earlyPeriod = loan.IsEarlyRepayment ? loan.EarlyRepaymentPeriod : null;

            for (int t = 1; t <= n; t++)
            {
                decimal interest = Math.Round(bal * r, 0);
                decimal principal = (t == n) ? bal : Math.Round(pmt - interest, 0);

                var row = new AmortizationRow
                {
                    Period = t,
                    PeriodLabel = GetPeriodLabel(t, loan.TermUnit),
                    OpeningBalance = bal,
                    PrincipalPayment = principal,
                    InterestPayment = interest,
                    ClosingBalance = bal - principal
                };

                if (earlyPeriod.HasValue && t == earlyPeriod.Value)
                {
                    row.IsEarlyRepayment = true;
                    row.EarlyRepaymentPenalty = Math.Round(
                        row.ClosingBalance * loan.EarlyRepaymentPenaltyRate, 0);
                    row.PrincipalPayment += row.ClosingBalance;
                    row.ClosingBalance = 0;
                    schedule.Add(row);
                    break;
                }

                bal = row.ClosingBalance;
                schedule.Add(row);
            }

            return schedule;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3C. Trả lãi định kỳ, gốc cuối kỳ (Interest Only)
        //     Mỗi kỳ chỉ trả lãi = P × r_kỳ
        //     Kỳ cuối: trả toàn bộ gốc + lãi kỳ cuối
        // ─────────────────────────────────────────────────────────────────────

        public static List<AmortizationRow> BuildInterestOnlySchedule(LoanInfo loan)
        {
            var schedule = new List<AmortizationRow>();
            decimal r = GetPeriodRate(loan.AnnualInterestRate, loan.TermUnit);
            decimal interest = Math.Round(loan.Principal * r, 0); // lãi cố định mỗi kỳ

            for (int t = 1; t <= loan.TermCount; t++)
            {
                bool isLast = (t == loan.TermCount);
                schedule.Add(new AmortizationRow
                {
                    Period = t,
                    PeriodLabel = GetPeriodLabel(t, loan.TermUnit),
                    OpeningBalance = loan.Principal,
                    PrincipalPayment = isLast ? loan.Principal : 0,
                    InterestPayment = interest,
                    ClosingBalance = isLast ? 0 : loan.Principal
                });
            }

            return schedule;
        }

        // ═════════════════════════════════════════════════════════════════════
        // PHẦN 4 — EIR (Effective Interest Rate)
        //
        // EIR tính trên chuỗi dòng tiền thực:
        //   Kỳ 0: -( Principal - ServiceFee )   ← nhận vay ròng
        //   Kỳ t: TotalPayment + EarlyRepaymentPenalty
        //
        // IRR giải ra là lãi suất/kỳ (theo TermUnit).
        // Sau đó annualize về năm bằng công thức ghép lãi:
        //   EIR_năm = (1 + r_kỳ)^PeriodsPerYear - 1
        // ═════════════════════════════════════════════════════════════════════

        public static decimal CalculateEIR(LoanInfo loan, List<AmortizationRow> schedule)
        {
            decimal netProceeds = loan.Principal - loan.ServiceFee;
            var cashFlows = new List<decimal> { -netProceeds };
            foreach (var row in schedule)
                cashFlows.Add(row.TotalPayment + row.EarlyRepaymentPenalty);

            double eirPerPeriod = SolveIRR(cashFlows.Select(x => (double)x).ToList());

            // Số kỳ/năm để annualize
            int periodsPerYear = loan.TermUnit switch
            {
                TermUnit.Monthly => 12,
                TermUnit.Quarterly => 4,
                TermUnit.Yearly => 1,
                _ => 12
            };

            double eirAnnual = Math.Pow(1 + eirPerPeriod, periodsPerYear) - 1;
            return Math.Round((decimal)eirAnnual, 6);
        }

        // ═════════════════════════════════════════════════════════════════════
        // PHẦN 5 — HELPER METHODS
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// PMT — khoản trả đều mỗi kỳ (Annuity).
        /// Công thức: PMT = P × r / (1 - (1+r)^-N)
        /// </summary>
        public static decimal CalculatePMT(decimal principal, decimal periodRate, int periods)
        {
            if (periodRate == 0) return Math.Round(principal / periods, 0);
            double r = (double)periodRate;
            double pmt = (double)principal * r / (1 - Math.Pow(1 + r, -periods));
            return Math.Round((decimal)pmt, 0);
        }

        /// <summary>
        /// Giải IRR bằng Newton-Raphson.
        /// Trả về IRR/kỳ dạng double (chưa annualize).
        /// </summary>
        internal static double SolveIRR(List<double> cashFlows,
            double guess = 0.01, int maxIter = 1000)
        {
            double r = guess;
            for (int i = 0; i < maxIter; i++)
            {
                double npv = 0, dnpv = 0;
                for (int t = 0; t < cashFlows.Count; t++)
                {
                    double factor = Math.Pow(1 + r, t);
                    npv += cashFlows[t] / factor;
                    if (t > 0) dnpv -= t * cashFlows[t] / (factor * (1 + r));
                }
                if (Math.Abs(dnpv) < 1e-12) break;
                double rNew = r - npv / dnpv;
                if (Math.Abs(rNew - r) < 1e-8) return rNew;
                r = rNew;
            }
            return r;
        }
        // ═════════════════════════════════════════════════════════════════════
        // PHẦN 6 — TÍNH TIỀN GỬI TIẾT KIỆM
        // ═════════════════════════════════════════════════════════════════════

        private static decimal ToRate(decimal percentRate)
        {
            return percentRate / 100m;
        }

        private static int GetSavingsPeriodsPerYear(InterestPaymentType paymentType)
        {
            return paymentType switch
            {
                InterestPaymentType.Monthly => 12,
                InterestPaymentType.Quarterly => 4,
                InterestPaymentType.Compound => 12,
                InterestPaymentType.EndOfTerm => 1,
                _ => 12
            };
        }

        /// <summary>
        /// Tính lịch tiết kiệm theo hình thức nhận lãi.
        /// AnnualInterestRate nhập dạng %, ví dụ 8 nghĩa là 8%/năm.
        /// </summary>
        public static List<SavingsRow> CalculateSavingsSchedule(SavingsInfo info)
        {
            var schedule = new List<SavingsRow>();

            if (info.Principal <= 0 || info.TermMonths <= 0)
                return schedule;

            decimal principal = info.Principal;
            decimal annualRate = ToRate(info.AnnualInterestRate);
            decimal balance = principal;

            int maxMonth = info.EarlyWithdrawAtMonth.HasValue
                ? Math.Min(info.EarlyWithdrawAtMonth.Value, info.TermMonths)
                : info.TermMonths;

            if (info.PaymentType == InterestPaymentType.EndOfTerm)
            {
                decimal interest = Math.Round(principal * annualRate * maxMonth / 12m, 0);

                schedule.Add(new SavingsRow
                {
                    Period = 1,
                    PeriodLabel = info.EarlyWithdrawAtMonth.HasValue ? $"Rút tháng {maxMonth}" : "Cuối kỳ",
                    OpeningBalance = principal,
                    InterestEarned = interest,
                    InterestPaid = interest,
                    ClosingBalance = principal,
                    IsEarlyWithdraw = info.EarlyWithdrawAtMonth.HasValue
                });

                return schedule;
            }

            int stepMonths = info.PaymentType == InterestPaymentType.Quarterly ? 3 : 1;
            int period = 0;

            for (int month = stepMonths; month <= maxMonth; month += stepMonths)
            {
                period++;

                decimal periodRate = annualRate * stepMonths / 12m;
                decimal interest = Math.Round(balance * periodRate, 0);
                bool isCompound = info.PaymentType == InterestPaymentType.Compound;

                schedule.Add(new SavingsRow
                {
                    Period = period,
                    PeriodLabel = GetSavingsPeriodLabel(period, info.PaymentType),
                    OpeningBalance = balance,
                    InterestEarned = interest,
                    InterestPaid = isCompound ? 0 : interest,
                    ClosingBalance = isCompound ? balance + interest : balance,
                    IsEarlyWithdraw = info.EarlyWithdrawAtMonth.HasValue && month == maxMonth
                });

                if (isCompound)
                    balance += interest;
            }

            return schedule;
        }

        private static string GetSavingsPeriodLabel(int period, InterestPaymentType paymentType)
        {
            return paymentType switch
            {
                InterestPaymentType.Monthly => $"Tháng {period}",
                InterestPaymentType.Quarterly => $"Quý {period}",
                InterestPaymentType.Compound => $"Tháng {period}",
                InterestPaymentType.EndOfTerm => "Cuối kỳ",
                _ => $"Kỳ {period}"
            };
        }

        /// <summary>
        /// Tính số tiền thực nhận khi rút trước hạn.
        /// DemandInterestRate nhập dạng %, ví dụ 0.1 nghĩa là 0.1%/năm.
        /// </summary>
        public static decimal CalculateEarlyWithdraw(SavingsInfo info, int withdrawAtMonth)
        {
            if (info.Principal <= 0 || withdrawAtMonth <= 0)
                return 0;

            int actualMonth = Math.Min(withdrawAtMonth, info.TermMonths);
            decimal demandRate = ToRate(info.DemandInterestRate);

            decimal actualInterest = Math.Round(
                info.Principal * demandRate * actualMonth / 12m, 0);

            return info.Principal + actualInterest;
        }

        /// <summary>
        /// Tính tiền lãi bị mất khi rút trước hạn.
        /// </summary>
        public static decimal CalculateEarlyWithdrawPenalty(SavingsInfo info, int withdrawAtMonth)
        {
            decimal normalInterest = SimpleInterest(
                info.Principal,
                ToRate(info.AnnualInterestRate),
                info.TermMonths,
                TermUnit.Monthly);

            decimal earlyAmount = CalculateEarlyWithdraw(info, withdrawAtMonth);
            decimal earlyInterest = earlyAmount - info.Principal;

            return Math.Max(0, normalInterest - earlyInterest);
        }

        /// <summary>
        /// Tính lãi suất thực tế năm.
        /// nominalRate nhập dạng %, ví dụ 8 nghĩa là 8%/năm.
        /// Kết quả trả về cũng là %, ví dụ 8.3 nghĩa là 8.3%/năm.
        /// </summary>
        public static decimal CalculateEffectiveRate(decimal nominalRate, int periodsPerYear)
        {
            if (periodsPerYear <= 0)
                return 0;

            decimal rate = ToRate(nominalRate);
            decimal periodRate = rate / periodsPerYear;

            decimal eir = (decimal)Math.Pow(
                (double)(1 + periodRate),
                periodsPerYear) - 1;

            return Math.Round(eir * 100m, 6);
        }
        // ═════════════════════════════════════════════════════════════════════
        // PHẦN 7 — VAY THẤU CHI / VAY HẠN MỨC
        // Lãi tính trên số tiền thực tế sử dụng và số ngày thực tế.
        // Công thức: Interest = UsedAmount × AnnualRate × UsedDays / 365
        // ═════════════════════════════════════════════════════════════════════

        public static decimal CalculateOverdraftInterest(OverdraftUsageEntry entry, decimal defaultAnnualRate = 0m)
        {
            if (entry == null || entry.UsedAmount <= 0)
                return 0;

            decimal annualRate = entry.AnnualInterestRate > 0
                ? entry.AnnualInterestRate
                : defaultAnnualRate;

            if (annualRate <= 0 || entry.UsedDays <= 0)
                return 0;

            decimal interest = entry.UsedAmount * annualRate * entry.UsedDays / 365m;
            return Math.Round(interest, 0);
        }

        public static decimal CalculateTotalOverdraftInterest(OverdraftLoanInfo loan)
        {
            if (loan == null || loan.UsageEntries == null)
                return 0;

            return loan.UsageEntries.Sum(entry =>
                CalculateOverdraftInterest(entry, loan.AnnualInterestRate));
        }

        public static bool IsOverCreditLimit(OverdraftLoanInfo loan)
        {
            if (loan == null || loan.UsageEntries == null)
                return false;

            decimal totalUsingAmount = loan.UsageEntries
                .Where(e => e.ToDate == null || e.ToDate.Value.Date >= DateTime.Today)
                .Sum(e => e.UsedAmount);

            return totalUsingAmount > loan.CreditLimit;
        }
    }
}