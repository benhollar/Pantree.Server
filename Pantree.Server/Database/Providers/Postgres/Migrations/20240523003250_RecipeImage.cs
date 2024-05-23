using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pantree.Server.Database.Providers.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RecipeImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBlob",
                table: "Recipes",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "Recipes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBlob",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "Recipes");
        }
    }
}
