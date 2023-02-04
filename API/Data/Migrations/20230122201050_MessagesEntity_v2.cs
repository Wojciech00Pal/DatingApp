using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
    public partial class MessagesEntity_v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    senderID = table.Column<int>(type: "INTEGER", nullable: false),
                    senderUserName = table.Column<string>(type: "TEXT", nullable: true),
                    recipientID = table.Column<int>(type: "INTEGER", nullable: false),
                    recipientUserName = table.Column<string>(type: "TEXT", nullable: true),
                    content = table.Column<string>(type: "TEXT", nullable: true),
                    dateRead = table.Column<DateTime>(type: "TEXT", nullable: true),
                    messageSent = table.Column<DateTime>(type: "TEXT", nullable: true),
                    senderDelted = table.Column<bool>(type: "INTEGER", nullable: false),
                    recipientDelted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Messages_Users_recipientID",
                        column: x => x.recipientID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_senderID",
                        column: x => x.senderID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_recipientID",
                table: "Messages",
                column: "recipientID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_senderID",
                table: "Messages",
                column: "senderID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
