using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBOICTKeuzewijzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class cascadesseveralentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_ApplicationUsers_SlbApplicationUserId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_Modules_ModuleId",
                table: "Semesters");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomModuleId",
                table: "Semesters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ECs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomModules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_CustomModuleId",
                table: "Semesters",
                column: "CustomModuleId",
                unique: true,
                filter: "[CustomModuleId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_ApplicationUsers_SlbApplicationUserId",
                table: "Chats",
                column: "SlbApplicationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_CustomModules_CustomModuleId",
                table: "Semesters",
                column: "CustomModuleId",
                principalTable: "CustomModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_Modules_ModuleId",
                table: "Semesters",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_ApplicationUsers_SlbApplicationUserId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_CustomModules_CustomModuleId",
                table: "Semesters");

            migrationBuilder.DropForeignKey(
                name: "FK_Semesters_Modules_ModuleId",
                table: "Semesters");

            migrationBuilder.DropTable(
                name: "CustomModules");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_CustomModuleId",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "CustomModuleId",
                table: "Semesters");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_ApplicationUsers_SlbApplicationUserId",
                table: "Chats",
                column: "SlbApplicationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Semesters_Modules_ModuleId",
                table: "Semesters",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");
        }
    }
}
