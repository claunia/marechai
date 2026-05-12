using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestionSubkey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suggestions_EntityType_EntityId",
                table: "Suggestions");

            migrationBuilder.AddColumn<string>(
                name: "Subkey",
                table: "Suggestions",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_EntityType_EntityId_Subkey_Status",
                table: "Suggestions",
                columns: new[] { "EntityType", "EntityId", "Subkey", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suggestions_EntityType_EntityId_Subkey_Status",
                table: "Suggestions");

            migrationBuilder.DropColumn(
                name: "Subkey",
                table: "Suggestions");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_EntityType_EntityId",
                table: "Suggestions",
                columns: new[] { "EntityType", "EntityId" });
        }
    }
}
