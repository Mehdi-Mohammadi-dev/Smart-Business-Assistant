using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

#nullable disable

namespace Vexora.Infrastracture.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261028212400_AddReferralWorkflow")]
public partial class AddReferralWorkflow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "InviteCode", table: "Users", type: "nvarchar(32)", maxLength: 32, nullable: true);
        migrationBuilder.Sql("UPDATE [Users] SET [InviteCode] = CONCAT('NOTA_', [Id]) WHERE [InviteCode] IS NULL;");
        migrationBuilder.AlterColumn<string>(name: "InviteCode", table: "Users", type: "nvarchar(32)", maxLength: 32, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(32)", oldMaxLength: 32, oldNullable: true);
        migrationBuilder.CreateIndex(name: "IX_Users_InviteCode", table: "Users", column: "InviteCode", unique: true);

        migrationBuilder.CreateTable(name: "ReferralSettings", columns: table => new
        {
            Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            IsEnabled = table.Column<bool>(type: "bit", nullable: false),
            RewardDays = table.Column<int>(type: "int", nullable: false),
            MaxRewardDaysPerUser = table.Column<int>(type: "int", nullable: false),
            CreateAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            UpdateAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            IsDelete = table.Column<bool>(type: "bit", nullable: false),
            DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_ReferralSettings", x => x.Id));

        migrationBuilder.CreateTable(name: "Referrals", columns: table => new
        {
            Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            ReferrerUserId = table.Column<long>(type: "bigint", nullable: false),
            ReferredUserId = table.Column<long>(type: "bigint", nullable: false),
            RewardGranted = table.Column<bool>(type: "bit", nullable: false),
            RewardedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            RewardDays = table.Column<int>(type: "int", nullable: false),
            CreateAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            UpdateAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            IsDelete = table.Column<bool>(type: "bit", nullable: false),
            DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Referrals", x => x.Id);
            table.ForeignKey(name: "FK_Referrals_Users_ReferredUserId", column: x => x.ReferredUserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            table.ForeignKey(name: "FK_Referrals_Users_ReferrerUserId", column: x => x.ReferrerUserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_Referrals_ReferredUserId", table: "Referrals", column: "ReferredUserId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Referrals_ReferrerUserId", table: "Referrals", column: "ReferrerUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Referrals");
        migrationBuilder.DropTable(name: "ReferralSettings");
        migrationBuilder.DropIndex(name: "IX_Users_InviteCode", table: "Users");
        migrationBuilder.DropColumn(name: "InviteCode", table: "Users");
    }
}
