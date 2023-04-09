using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Semibox.StatisticService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIdField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Statistics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Statistics",
                type: "TEXT",
                nullable: true);
        }
    }
}
