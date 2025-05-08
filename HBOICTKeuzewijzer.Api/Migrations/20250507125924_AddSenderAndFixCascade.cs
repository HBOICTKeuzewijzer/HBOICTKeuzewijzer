using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBOICTKeuzewijzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderAndFixCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SenderApplicationUserId",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderApplicationUserId",
                table: "Messages",
                column: "SenderApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ApplicationUsers_SenderApplicationUserId",
                table: "Messages",
                column: "SenderApplicationUserId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ApplicationUsers_SenderApplicationUserId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderApplicationUserId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SenderApplicationUserId",
                table: "Messages");
        }
    }
}
