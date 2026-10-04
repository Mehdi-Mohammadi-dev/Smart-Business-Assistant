using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_Index_In_Check : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Checks_UserId",
                table: "Checks");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_UserId_CustomerId",
                table: "Checks",
                columns: new[] { "UserId", "CustomerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Checks_UserId_CustomerId",
                table: "Checks");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_UserId",
                table: "Checks",
                column: "UserId");
        }
    }
}
