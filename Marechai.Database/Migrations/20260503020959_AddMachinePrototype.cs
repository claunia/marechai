using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMachinePrototype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Prototype",
                table: "machines",
                type: "bit(1)",
                nullable: false,
                defaultValue: false);

            // Migrate existing year-1000 sentinel values to proper Prototype flag
            migrationBuilder.Sql("UPDATE machines SET Prototype = 1, Introduced = NULL WHERE YEAR(Introduced) = 1000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prototype",
                table: "machines");
        }
    }
}
