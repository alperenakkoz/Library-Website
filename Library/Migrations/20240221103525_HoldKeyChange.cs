using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class HoldKeyChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Hold",
                table: "Hold");

            migrationBuilder.DropIndex(
                name: "IX_Hold_LibraryUserId",
                table: "Hold");

            migrationBuilder.DropColumn(
                name: "HoldId",
                table: "Hold");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hold",
                table: "Hold",
                columns: new[] { "LibraryUserId", "BooksId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Hold",
                table: "Hold");

            migrationBuilder.AddColumn<int>(
                name: "HoldId",
                table: "Hold",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hold",
                table: "Hold",
                column: "HoldId");

            migrationBuilder.CreateIndex(
                name: "IX_Hold_LibraryUserId",
                table: "Hold",
                column: "LibraryUserId");
        }
    }
}
