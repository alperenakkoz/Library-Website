using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class minorLanguageUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "BookLanguage",
                newName: "LanguageName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BookLanguage",
                newName: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LanguageName",
                table: "BookLanguage",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "BookLanguage",
                newName: "Id");
        }
    }
}
