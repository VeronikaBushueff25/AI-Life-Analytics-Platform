using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AILifeAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class AddCbtRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CbtRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Situation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PhysicalState = table.Column<string>(type: "text", nullable: false),
                    AutomaticThought = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThoughtBelief = table.Column<int>(type: "integer", nullable: false),
                    PrimaryEmotion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EmotionIntensity = table.Column<int>(type: "integer", nullable: false),
                    AdditionalEmotions = table.Column<string>(type: "text", nullable: false),
                    Behavior = table.Column<string>(type: "text", nullable: false),
                    DetectedDistortions = table.Column<string>(type: "text", nullable: false),
                    AiChallenge = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AiQuestions = table.Column<string>(type: "text", nullable: false),
                    EvidenceFor = table.Column<string>(type: "text", nullable: false),
                    EvidenceAgainst = table.Column<string>(type: "text", nullable: false),
                    ReframedThought = table.Column<string>(type: "text", nullable: false),
                    NewThoughtBelief = table.Column<int>(type: "integer", nullable: false),
                    NewEmotionIntensity = table.Column<int>(type: "integer", nullable: false),
                    Insight = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    AiSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CbtRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CbtRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CbtRecords_UserId_CreatedAt",
                table: "CbtRecords",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CbtRecords");
        }
    }
}
