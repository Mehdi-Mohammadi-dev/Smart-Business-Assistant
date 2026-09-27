using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToCustomerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId_FirstName_LastName",
                table: "Customers",
                columns: new[] { "UserId", "FirstName", "LastName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_UserId_FirstName_LastName",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId");
        }
    }
}
