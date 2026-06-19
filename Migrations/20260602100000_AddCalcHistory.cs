using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCalcHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // LoanInfos
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "LoanInfos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CalculatedAt",
                table: "LoanInfos",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            // SavingsInfos
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "SavingsInfos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CalculatedAt",
                table: "SavingsInfos",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "UserId", table: "LoanInfos");
            migrationBuilder.DropColumn(name: "CalculatedAt", table: "LoanInfos");
            migrationBuilder.DropColumn(name: "UserId", table: "SavingsInfos");
            migrationBuilder.DropColumn(name: "CalculatedAt", table: "SavingsInfos");
        }
    }
}
