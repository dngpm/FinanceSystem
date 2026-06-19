using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixCashFlowComparer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000003"));

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "InterestRateTables");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                table: "InterestRateTables",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdminNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    AnnualRatePercent = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "InterestRateTables",
                columns: new[] { "Id", "AnnualRatePercent", "Note", "ProductType", "RateType", "TermLabel", "TermMonths", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), 2.5m, "", "SavingDeposit", "Savings", "1 tháng", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), 3.0m, "", "SavingDeposit", "Savings", "3 tháng", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), 4.5m, "", "SavingDeposit", "Savings", "6 tháng", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), 4.7m, "", "SavingDeposit", "Savings", "9 tháng", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), 5.2m, "", "SavingDeposit", "Savings", "12 tháng", 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), 5.5m, "", "SavingDeposit", "Savings", "18 tháng", 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), 5.7m, "", "SavingDeposit", "Savings", "24 tháng", 24, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), 6.0m, "Khóa hạn", "SavingDeposit", "Savings", "36 tháng", 36, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000001"), 14.0m, "Không cần tài sản bảo đảm", "UnsecuredLoan", "Loan", "Vay tín chấp", 36, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000002"), 16.0m, "Tính lãi trên số tiền đã sử dụng", "OverdraftLoan", "Loan", "Vay thấu chi", 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000003"), 10.5m, "Trả góp định kỳ", "InstallmentLoan", "Loan", "Vay trả góp", 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_UserId",
                table: "ServiceRequests",
                column: "UserId");
        }
    }
}
