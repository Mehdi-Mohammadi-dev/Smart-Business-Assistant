using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class fix_check_and_Payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "PaymentDate",
                table: "Payments",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<byte>(
                name: "Direction",
                table: "Payments",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "BankRegistrationStatus",
                table: "Checks",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Direction",
                table: "Checks",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SettlementStatus",
                table: "Checks",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direction",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BankRegistrationStatus",
                table: "Checks");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "Checks");

            migrationBuilder.DropColumn(
                name: "SettlementStatus",
                table: "Checks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
        }
    }
}
