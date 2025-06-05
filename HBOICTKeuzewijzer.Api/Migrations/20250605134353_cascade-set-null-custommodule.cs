using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBOICTKeuzewijzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class cascadesetnullcustommodule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_CustomModules_CustomModuleId",
                table: "Semesters");

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_CustomModules_CustomModuleId",
                table: "Semesters",
                column: "CustomModuleId",
                principalTable: "CustomModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_CustomModules_CustomModuleId",
                table: "Semesters");

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_CustomModules_CustomModuleId",
                table: "Semesters",
                column: "CustomModuleId",
                principalTable: "CustomModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
