using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Translator",
                columns: table => new
                {
                    TranslatorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TranslatorName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translator", x => x.TranslatorId);
                });

            migrationBuilder.CreateTable(
                name: "BookTranslator",
                columns: table => new
                {
                    BooksId = table.Column<int>(type: "int", nullable: false),
                    TranslatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTranslator", x => new { x.BooksId, x.TranslatorId });
                    table.ForeignKey(
                        name: "FK_BookTranslator_Books_BooksId",
                        column: x => x.BooksId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookTranslator_Translator_TranslatorId",
                        column: x => x.TranslatorId,
                        principalTable: "Translator",
                        principalColumn: "TranslatorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookTranslator_TranslatorId",
                table: "BookTranslator",
                column: "TranslatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookTranslator");

            migrationBuilder.DropTable(
                name: "Translator");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Books");
        }
    }
}
