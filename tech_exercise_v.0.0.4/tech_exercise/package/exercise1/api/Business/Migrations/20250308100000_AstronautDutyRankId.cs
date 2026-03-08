using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    public partial class AstronautDutyRankId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RankId",
                table: "AstronautDuty",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_AstronautDuty_RankId",
                table: "AstronautDuty",
                column: "RankId");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "AstronautDuty");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AstronautDuty_RankId",
                table: "AstronautDuty");

            migrationBuilder.AddColumn<string>(
                name: "Rank",
                table: "AstronautDuty",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "RankId",
                table: "AstronautDuty");
        }
    }
}
