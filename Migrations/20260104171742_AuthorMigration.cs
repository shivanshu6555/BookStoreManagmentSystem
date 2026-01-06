using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStoreManagmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AuthorMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthorID",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorID",
                table: "Books");
        }
    }
}
