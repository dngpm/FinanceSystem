using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000005"));

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
                    RequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    AnnualRatePercent = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    CustomerNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
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

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000001"),
                column: "ProductType",
                value: "SavingDeposit");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000002"),
                column: "ProductType",
                value: "SavingDeposit");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000003"),
                column: "ProductType",
                value: "SavingDeposit");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000004"),
                column: "ProductType",
                value: "SavingDeposit");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000005"),
                column: "ProductType",
                value: "SavingDeposit");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000006"),
                column: "ProductType",
                value: "SavingDeposit");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000007"),
                column: "ProductType",
                value: "SavingDeposit");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000008"),
                column: "ProductType",
                value: "SavingDeposit");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000001"),
                columns: new[] { "AnnualRatePercent", "Note", "ProductType", "TermLabel", "TermMonths" },
                values: new object[] { 14.0m, "Không cần tài sản bảo đảm", "UnsecuredLoan", "Vay tín chấp", 36 });

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000002"),
                columns: new[] { "AnnualRatePercent", "Note", "ProductType", "TermLabel", "TermMonths" },
                values: new object[] { 16.0m, "Tính lãi trên số tiền đã sử dụng", "OverdraftLoan", "Vay thấu chi", 12 });

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000003"),
                columns: new[] { "Note", "ProductType", "TermLabel" },
                values: new object[] { "Trả góp định kỳ", "InstallmentLoan", "Vay trả góp" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_UserId",
                table: "ServiceRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "InterestRateTables");

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000001"),
                columns: new[] { "AnnualRatePercent", "Note", "TermLabel", "TermMonths" },
                values: new object[] { 8.5m, "Ngắn hạn", "≤ 12 tháng", 12 });

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000002"),
                columns: new[] { "AnnualRatePercent", "Note", "TermLabel", "TermMonths" },
                values: new object[] { 9.5m, "Ngắn hạn", "13–24 tháng", 24 });

            migrationBuilder.UpdateData(
                table: "InterestRateTables",
                keyColumn: "Id",
                keyValue: new Guid("b2000000-0000-0000-0000-000000000003"),
                columns: new[] { "Note", "TermLabel" },
                values: new object[] { "Trung hạn", "25–60 tháng" });

            migrationBuilder.InsertData(
                table: "InterestRateTables",
                columns: new[] { "Id", "AnnualRatePercent", "Note", "RateType", "TermLabel", "TermMonths", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("b2000000-0000-0000-0000-000000000004"), 11.5m, "Dài hạn", "Loan", "61–120 tháng", 120, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { new Guid("b2000000-0000-0000-0000-000000000005"), 12.5m, "Dài hạn", "Loan", "> 120 tháng", 240, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system" }
                });
        }
    }
}
