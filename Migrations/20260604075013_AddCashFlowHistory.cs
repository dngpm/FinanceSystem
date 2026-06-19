using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCashFlowHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashFlowHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    InitialInvestment = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(8,6)", nullable: false),
                    EntryCount = table.Column<int>(type: "int", nullable: false),
                    PeriodCount = table.Column<int>(type: "int", nullable: false),
                    TotalInflow = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TotalOutflow = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NetCashFlow = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NPV = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    IRR = table.Column<decimal>(type: "decimal(8,6)", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HasDeficit = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowHistories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashFlowHistories");
        }
    }
}
