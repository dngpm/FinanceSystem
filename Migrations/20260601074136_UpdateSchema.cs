using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterestRateTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RateType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TermLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    AnnualRatePercent = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterestRateTables", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "InterestRateTables",
                columns: new[] { "Id", "AnnualRatePercent", "Note", "RateType", "TermLabel", "TermMonths", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), 2.5m, "", "Savings", "1 tháng", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), 3.0m, "", "Savings", "3 tháng", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), 4.5m, "", "Savings", "6 tháng", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), 4.7m, "", "Savings", "9 tháng", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), 5.2m, "", "Savings", "12 tháng", 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), 5.5m, "", "Savings", "18 tháng", 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), 5.7m, "", "Savings", "24 tháng", 24, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), 6.0m, "Khóa hạn", "Savings", "36 tháng", 36, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000001"), 8.5m, "Ngắn hạn", "Loan", "≤ 12 tháng", 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000002"), 9.5m, "Ngắn hạn", "Loan", "13–24 tháng", 24, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000003"), 10.5m, "Trung hạn", "Loan", "25–60 tháng", 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000004"), 11.5m, "Dài hạn", "Loan", "61–120 tháng", 120, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000005"), 12.5m, "Dài hạn", "Loan", "> 120 tháng", 240, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterestRateTables");
        }
    }
}
