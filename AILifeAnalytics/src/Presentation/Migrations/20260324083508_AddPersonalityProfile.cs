using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AILifeAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalityProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalityProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysAnalyzed = table.Column<int>(type: "integer", nullable: false),
                    ArchetypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ArchetypeDescription = table.Column<string>(type: "text", nullable: false),
                    ArchetypeEmoji = table.Column<string>(type: "text", nullable: false),
                    PeakPerformancePattern = table.Column<string>(type: "text", nullable: false),
                    EnergyPattern = table.Column<string>(type: "text", nullable: false),
                    StressPattern = table.Column<string>(type: "text", nullable: false),
                    Superpowers = table.Column<string>(type: "text", nullable: false),
                    Vulnerabilities = table.Column<string>(type: "text", nullable: false),
                    Recommendations = table.Column<string>(type: "text", nullable: false),
                    OptimalSleepHours = table.Column<double>(type: "double precision", nullable: false),
                    OptimalWorkHours = table.Column<double>(type: "double precision", nullable: false),
                    MostProductiveDayOfWeek = table.Column<string>(type: "text", nullable: false),
                    CorrelationSleepFocus = table.Column<double>(type: "double precision", nullable: false),
                    CorrelationStressMood = table.Column<double>(type: "double precision", nullable: false),
                    ForecastText = table.Column<string>(type: "text", nullable: false),
                    ForecastRisk = table.Column<string>(type: "text", nullable: false),
                    FullAnalysis = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalityProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalityProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalityProfiles_UserId_GeneratedAt",
                table: "PersonalityProfiles",
                columns: new[] { "UserId", "GeneratedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalityProfiles");
        }
    }
}
