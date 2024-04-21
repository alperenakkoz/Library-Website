using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class AddImageOnBookAndAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ISBN",
                table: "Books",
                newName: "ImagePath");

            migrationBuilder.AddColumn<string>(
                name: "ISBN10",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ISBN13",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Author",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Author",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ISBN10",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ISBN13",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Author");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Author");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Books",
                newName: "ISBN");
        }
    }
}
