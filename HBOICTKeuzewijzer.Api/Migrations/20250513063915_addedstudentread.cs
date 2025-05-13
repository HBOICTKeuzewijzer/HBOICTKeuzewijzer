using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBOICTKeuzewijzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class addedstudentread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SlbRead",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StudentRead",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlbRead",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "StudentRead",
                table: "Messages");
        }
    }
}
