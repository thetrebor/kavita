using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSessionActivityDataToOwnTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityData",
                table: "AppUserReadingSession");

            migrationBuilder.CreateTable(
                name: "AppUserReadingSessionActivityData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppUserReadingSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChapterId = table.Column<int>(type: "INTEGER", nullable: false),
                    VolumeId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeriesId = table.Column<int>(type: "INTEGER", nullable: false),
                    LibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartPage = table.Column<int>(type: "INTEGER", nullable: false),
                    EndPage = table.Column<int>(type: "INTEGER", nullable: false),
                    StartBookScrollId = table.Column<string>(type: "TEXT", nullable: true),
                    EndBookScrollId = table.Column<string>(type: "TEXT", nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PagesRead = table.Column<int>(type: "INTEGER", nullable: false),
                    WordsRead = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPages = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalWords = table.Column<long>(type: "INTEGER", nullable: false),
                    DeviceIds = table.Column<string>(type: "TEXT", nullable: false),
                    ClientInfo = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserReadingSessionActivityData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserReadingSessionActivityData_AppUserReadingSession_AppUserReadingSessionId",
                        column: x => x.AppUserReadingSessionId,
                        principalTable: "AppUserReadingSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserReadingSessionActivityData_AppUserReadingSessionId",
                table: "AppUserReadingSessionActivityData",
                column: "AppUserReadingSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserReadingSessionActivityData");

            migrationBuilder.AddColumn<string>(
                name: "ActivityData",
                table: "AppUserReadingSession",
                type: "TEXT",
                nullable: true,
                defaultValue: "[]");
        }
    }
}
