using Microsoft.EntityFrameworkCore.Migrations;

namespace MyLicenta.Migrations
{
    public partial class SymptomOccurrence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OccurenceProbability",
                table: "Symptoms",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccurenceProbability",
                table: "Symptoms");
        }
    }
}
