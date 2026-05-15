using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_PeopleBySoftware_RoleId",
                table: "PeopleBySoftware",
                newName: "idx_people_by_software_role");

            migrationBuilder.RenameIndex(
                name: "IX_PeopleBySoftware_PersonId",
                table: "PeopleBySoftware",
                newName: "idx_people_by_software_person");

            migrationBuilder.RenameIndex(
                name: "IX_GenresBySoftware_GenreId",
                table: "GenresBySoftware",
                newName: "idx_genres_by_software_genre");

            migrationBuilder.CreateIndex(
                name: "idx_software_covers_type_release",
                table: "SoftwareCovers",
                columns: new[] { "Type", "SoftwareReleaseId" });

            migrationBuilder.CreateIndex(
                name: "idx_machines_type_introduced",
                table: "machines",
                columns: new[] { "type", "introduced" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_software_covers_type_release",
                table: "SoftwareCovers");

            migrationBuilder.DropIndex(
                name: "idx_machines_type_introduced",
                table: "machines");

            migrationBuilder.RenameIndex(
                name: "idx_people_by_software_role",
                table: "PeopleBySoftware",
                newName: "IX_PeopleBySoftware_RoleId");

            migrationBuilder.RenameIndex(
                name: "idx_people_by_software_person",
                table: "PeopleBySoftware",
                newName: "IX_PeopleBySoftware_PersonId");

            migrationBuilder.RenameIndex(
                name: "idx_genres_by_software_genre",
                table: "GenresBySoftware",
                newName: "IX_GenresBySoftware_GenreId");
        }
    }
}
