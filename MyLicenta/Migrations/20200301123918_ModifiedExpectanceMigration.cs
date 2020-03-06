using Microsoft.EntityFrameworkCore.Migrations;

namespace MyLicenta.Migrations
{
    public partial class ModifiedExpectanceMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyPart",
                table: "Symptoms",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "OccurenceProbability",
                table: "SymptomDiseases",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "GeneralProbability",
                table: "Diseases",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyPart",
                table: "Symptoms");

            migrationBuilder.AlterColumn<int>(
                name: "OccurenceProbability",
                table: "SymptomDiseases",
                type: "int",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "GeneralProbability",
                table: "Diseases",
                type: "int",
                nullable: false,
                oldClrType: typeof(double));
        }
    }
}
