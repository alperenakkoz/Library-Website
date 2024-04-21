using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationHoldRentAndWaitList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hold",
                columns: table => new
                {
                    HoldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LibraryUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BooksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hold", x => x.HoldId);
                    table.ForeignKey(
                        name: "FK_Hold_AspNetUsers_LibraryUserId",
                        column: x => x.LibraryUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hold_Books_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibraryUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notification_AspNetUsers_LibraryUserId",
                        column: x => x.LibraryUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rent",
                columns: table => new
                {
                    RentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LibraryUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BooksId = table.Column<int>(type: "int", nullable: false),
                    IsReturned = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rent", x => x.RentId);
                    table.ForeignKey(
                        name: "FK_Rent_AspNetUsers_LibraryUserId",
                        column: x => x.LibraryUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rent_Books_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WaitList",
                columns: table => new
                {
                    LibraryUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BooksId = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitList", x => new { x.LibraryUserId, x.BooksId });
                    table.ForeignKey(
                        name: "FK_WaitList_AspNetUsers_LibraryUserId",
                        column: x => x.LibraryUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WaitList_Books_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hold_BooksId",
                table: "Hold",
                column: "BooksId");

            migrationBuilder.CreateIndex(
                name: "IX_Hold_LibraryUserId",
                table: "Hold",
                column: "LibraryUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_LibraryUserId",
                table: "Notification",
                column: "LibraryUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rent_BooksId",
                table: "Rent",
                column: "BooksId");

            migrationBuilder.CreateIndex(
                name: "IX_Rent_LibraryUserId",
                table: "Rent",
                column: "LibraryUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitList_BooksId",
                table: "WaitList",
                column: "BooksId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hold");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Rent");

            migrationBuilder.DropTable(
                name: "WaitList");
        }
    }
}
