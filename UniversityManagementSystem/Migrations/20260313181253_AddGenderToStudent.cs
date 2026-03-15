using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversityManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Students__ClassI__3C69FB99",
                table: "Students");

            migrationBuilder.AlterColumn<int>(
                name: "ClassID",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK__Students__ClassI__3C69FB99",
                table: "Students",
                column: "ClassID",
                principalTable: "Classes",
                principalColumn: "ClassID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Students__ClassI__3C69FB99",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Students");

            migrationBuilder.AlterColumn<int>(
                name: "ClassID",
                table: "Students",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK__Students__ClassI__3C69FB99",
                table: "Students",
                column: "ClassID",
                principalTable: "Classes",
                principalColumn: "ClassID");
        }
    }
}
