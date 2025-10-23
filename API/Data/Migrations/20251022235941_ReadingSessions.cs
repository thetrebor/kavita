using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReadingSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalReads",
                table: "AppUserProgresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppUserReadingHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true, defaultValue: "{\"TotalMinutesRead\":0,\"TotalPagesRead\":0,\"TotalWordsRead\":0,\"LongestSessionMinutes\":0}"),
                    AppUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserReadingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserReadingHistory_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUserReadingSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    ActivityData = table.Column<string>(type: "TEXT", nullable: true, defaultValue: "[]"),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AppUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserReadingSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserReadingSession_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserReadingHistory_AppUserId",
                table: "AppUserReadingHistory",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserReadingHistory_DateUtc",
                table: "AppUserReadingHistory",
                column: "DateUtc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserReadingSession_AppUserId",
                table: "AppUserReadingSession",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserReadingSession_IsActive",
                table: "AppUserReadingSession",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserReadingHistory");

            migrationBuilder.DropTable(
                name: "AppUserReadingSession");

            migrationBuilder.DropColumn(
                name: "TotalReads",
                table: "AppUserProgresses");
        }
    }
}
