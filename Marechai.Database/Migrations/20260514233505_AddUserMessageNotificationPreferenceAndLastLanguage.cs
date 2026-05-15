using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMessageNotificationPreferenceAndLastLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastLanguageVisited",
                table: "AspNetUsers",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Default to TRUE so existing accounts opt in to notifications when the column is added,
            // matching the C# property initialiser used by RegisterAsync for new accounts.
            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnNewMessage",
                table: "AspNetUsers",
                type: "bit(1)",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLanguageVisited",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NotifyOnNewMessage",
                table: "AspNetUsers");
        }
    }
}
