using System.ComponentModel.DataAnnotations;

public class SavingsResultViewModel
{
    // Thông tin đầu vào
    [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền gốc phải lớn hơn 0")]
    public decimal Principal { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Lãi suất năm không được âm")]
    public decimal AnnualInterestRate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Kỳ hạn phải lớn hơn 0 tháng")]
    public int TermMonths { get; set; }

    public InterestPaymentType PaymentType { get; set; }

	public string PaymentTypeDisplayName => PaymentType switch
	{
		InterestPaymentType.EndOfTerm
			=> "Nhận lãi cuối kỳ",

		InterestPaymentType.Monthly
			=> "Nhận lãi hàng tháng",

		InterestPaymentType.Quarterly
			=> "Nhận lãi hàng quý",

		InterestPaymentType.Compound
			=> "Tái tục gốc và lãi (lãi kép)",

		_ => PaymentType.ToString()
	};

	public int TotalPeriods => Schedule.Count;

	// Kết quả tổng
	[Range(0, double.MaxValue, ErrorMessage = "Tổng lãi không được âm")]
    public decimal TotalInterest { get; set; }        // Tổng lãi nhận được

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền thực nhận không được âm")]
    public decimal ActualAmountReceived { get; set; } // Thực nhận (gốc + lãi)

    [Range(0, double.MaxValue, ErrorMessage = "Lãi suất thực tế không được âm")]
    public decimal EffectiveAnnualRate { get; set; }  // Lãi suất thực tế/năm (EIR)

    // Xử lý rút trước hạn
    public bool IsEarlyWithdraw { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Tiền phạt rút trước hạn không được âm")]
    public decimal EarlyWithdrawPenalty { get; set; } // Số tiền bị mất do phạt

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền rút trước hạn không được âm")]
    public decimal EarlyWithdrawAmount { get; set; }  // Thực nhận nếu rút sớm

    // Lịch nhận lãi chi tiết
    public List<SavingsRow> Schedule { get; set; } = new List<SavingsRow>();
}