using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AllowSharedReleaseCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SoftwareProductCodes_Issuer_Code",
                table: "SoftwareProductCodes");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareBarcodes_Code",
                table: "SoftwareBarcodes");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProductCodes_Issuer_Code",
                table: "SoftwareProductCodes",
                columns: new[] { "Issuer", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProductCodes_ReleaseId_Issuer_Code",
                table: "SoftwareProductCodes",
                columns: new[] { "ReleaseId", "Issuer", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBarcodes_Code",
                table: "SoftwareBarcodes",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBarcodes_ReleaseId_Code",
                table: "SoftwareBarcodes",
                columns: new[] { "ReleaseId", "Code" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_SoftwareProductCodes_ReleaseId",
                table: "SoftwareProductCodes");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareBarcodes_ReleaseId",
                table: "SoftwareBarcodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProductCodes_ReleaseId",
                table: "SoftwareProductCodes",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBarcodes_ReleaseId",
                table: "SoftwareBarcodes",
                column: "ReleaseId");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareProductCodes_Issuer_Code",
                table: "SoftwareProductCodes");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareProductCodes_ReleaseId_Issuer_Code",
                table: "SoftwareProductCodes");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareBarcodes_Code",
                table: "SoftwareBarcodes");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareBarcodes_ReleaseId_Code",
                table: "SoftwareBarcodes");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProductCodes_Issuer_Code",
                table: "SoftwareProductCodes",
                columns: new[] { "Issuer", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBarcodes_Code",
                table: "SoftwareBarcodes",
                column: "Code",
                unique: true);
        }
    }
}
