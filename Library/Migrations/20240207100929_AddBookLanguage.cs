using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class AddBookLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookLanguageId",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BookLanguage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookLanguage", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_BookLanguageId",
                table: "Books",
                column: "BookLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_BookLanguage_BookLanguageId",
                table: "Books",
                column: "BookLanguageId",
                principalTable: "BookLanguage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_BookLanguage_BookLanguageId",
                table: "Books");

            migrationBuilder.DropTable(
                name: "BookLanguage");

            migrationBuilder.DropIndex(
                name: "IX_Books_BookLanguageId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "BookLanguageId",
                table: "Books");
        }
    }
}
