using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class WidenSoftwareCriticReviewUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReviewUrl",
                table: "SoftwareCriticReviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReviewUrl",
                table: "SoftwareCriticReviews",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
