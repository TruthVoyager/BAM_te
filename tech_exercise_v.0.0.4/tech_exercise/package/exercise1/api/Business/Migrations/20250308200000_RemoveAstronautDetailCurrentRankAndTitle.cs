using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class RemoveAstronautDetailCurrentRankAndTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentDutyTitle",
                table: "AstronautDetail");

            migrationBuilder.DropColumn(
                name: "CurrentRank",
                table: "AstronautDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentDutyTitle",
                table: "AstronautDetail",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrentRank",
                table: "AstronautDetail",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
