using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBOICTKeuzewijzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class slbtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Slb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlbApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slb", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Slb_ApplicationUsers_SlbApplicationUserId",
                        column: x => x.SlbApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Slb_ApplicationUsers_StudentApplicationUserId",
                        column: x => x.StudentApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Slb_SlbApplicationUserId",
                table: "Slb",
                column: "SlbApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Slb_StudentApplicationUserId",
                table: "Slb",
                column: "StudentApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Slb");
        }
    }
}
