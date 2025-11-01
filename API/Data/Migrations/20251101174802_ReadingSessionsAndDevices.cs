using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReadingSessionsAndDevices : Migration
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
                    ClientInfoUsed = table.Column<string>(type: "TEXT", nullable: true, defaultValue: "[]"),
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

            migrationBuilder.CreateTable(
                name: "ClientDevice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UiFingerprint = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceFingerprint = table.Column<string>(type: "TEXT", nullable: false),
                    FriendlyName = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentClientInfo = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{\"UserAgent\":\"\",\"IpAddress\":\"\",\"AuthType\":0,\"ClientType\":0,\"AppVersion\":null,\"Browser\":null,\"BrowserVersion\":null,\"Platform\":0,\"DeviceType\":null,\"ScreenWidth\":null,\"ScreenHeight\":null,\"Orientation\":null,\"CapturedAt\":\"0001-01-01T00:00:00\"}"),
                    FirstSeenUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AppUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientDevice_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientDeviceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientInfo = table.Column<string>(type: "TEXT", nullable: true, defaultValue: "{\"UserAgent\":\"\",\"IpAddress\":\"\",\"AuthType\":0,\"ClientType\":0,\"AppVersion\":null,\"Browser\":null,\"BrowserVersion\":null,\"Platform\":0,\"DeviceType\":null,\"ScreenWidth\":null,\"ScreenHeight\":null,\"Orientation\":null,\"CapturedAt\":\"0001-01-01T00:00:00\"}"),
                    CapturedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDeviceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientDeviceHistory_ClientDevice_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "ClientDevice",
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

            migrationBuilder.CreateIndex(
                name: "IX_ClientDevice_AppUserId",
                table: "ClientDevice",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDeviceHistory_DeviceId",
                table: "ClientDeviceHistory",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserReadingHistory");

            migrationBuilder.DropTable(
                name: "AppUserReadingSession");

            migrationBuilder.DropTable(
                name: "ClientDeviceHistory");

            migrationBuilder.DropTable(
                name: "ClientDevice");

            migrationBuilder.DropColumn(
                name: "TotalReads",
                table: "AppUserProgresses");
        }
    }
}
