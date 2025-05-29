using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBOICTKeuzewijzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class chatmodulecascadechanges : Migration
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

            migrationBuilder.AddColumn<bool>(
                name: "IsPropaedeutic",
                table: "Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
                name: "IX_Semesters_CustomModuleId",
                table: "Semesters",
                column: "CustomModuleId",
                unique: true,
                filter: "[CustomModuleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Slb_SlbApplicationUserId",
                table: "Slb",
                column: "SlbApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Slb_StudentApplicationUserId",
                table: "Slb",
                column: "StudentApplicationUserId");

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

            migrationBuilder.DropTable(
                name: "Slb");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_CustomModuleId",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "CustomModuleId",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "IsPropaedeutic",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "SlbRead",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "StudentRead",
                table: "Messages");

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
