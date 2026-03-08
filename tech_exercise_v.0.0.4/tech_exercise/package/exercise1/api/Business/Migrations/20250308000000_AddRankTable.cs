using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class AddRankTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rank",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rank", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rank_Level",
                table: "Rank",
                column: "Level",
                unique: true);

            migrationBuilder.InsertData(
                table: "Rank",
                columns: new[] { "Id", "Name", "Level" },
                values: new object[,]
                {
                    { 1, "Specialist", 1 },
                    { 2, "Sergeant", 2 },
                    { 3, "Captain", 3 },
                    { 4, "Colonel", 4 },
                    { 5, "General", 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Rank");
        }
    }
}
