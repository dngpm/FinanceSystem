using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashFlowEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvestmentProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InitialInvestment = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    CashFlows = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(8,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Principal = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    AnnualInterestRate = table.Column<decimal>(type: "decimal(8,6)", nullable: false),
                    TermCount = table.Column<int>(type: "int", nullable: false),
                    TermUnit = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    IsEarlyRepayment = table.Column<bool>(type: "bit", nullable: false),
                    EarlyRepaymentPeriod = table.Column<int>(type: "int", nullable: true),
                    EarlyRepaymentPenaltyRate = table.Column<decimal>(type: "decimal(8,6)", nullable: false, defaultValue: 0.02m),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,0)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AmortizationRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanInfoId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    PrincipalPayment = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    InterestPayment = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    IsEarlyRepayment = table.Column<bool>(type: "bit", nullable: false),
                    EarlyRepaymentPenalty = table.Column<decimal>(type: "decimal(18,0)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmortizationRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmortizationRows_LoanInfos_LoanInfoId",
                        column: x => x.LoanInfoId,
                        principalTable: "LoanInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmortizationRows_LoanInfoId",
                table: "AmortizationRows",
                column: "LoanInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmortizationRows");

            migrationBuilder.DropTable(
                name: "CashFlowEntries");

            migrationBuilder.DropTable(
                name: "InvestmentProjects");

            migrationBuilder.DropTable(
                name: "LoanInfos");
        }
    }
}
