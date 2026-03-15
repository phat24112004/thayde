using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversityManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeGradeToUniversityStandard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "Grades");

            migrationBuilder.AddColumn<decimal>(
                name: "FinalScore",
                table: "Grades",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LetterGrade",
                table: "Grades",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MidtermScore1",
                table: "Grades",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MidtermScore2",
                table: "Grades",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalScore",
                table: "Grades",
                type: "decimal(5,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalScore",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "LetterGrade",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "MidtermScore1",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "MidtermScore2",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "Grades");

            migrationBuilder.AddColumn<decimal>(
                name: "Score",
                table: "Grades",
                type: "decimal(4,2)",
                nullable: true);
        }
    }
}
