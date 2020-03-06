using Microsoft.EntityFrameworkCore.Migrations;

namespace MyLicenta.Migrations
{
    public partial class ModifiedIdMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Symptoms",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "SymptomDiseases",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Diseases",
                newName: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Symptoms",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SymptomDiseases",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Diseases",
                newName: "ID");
        }
    }
}
