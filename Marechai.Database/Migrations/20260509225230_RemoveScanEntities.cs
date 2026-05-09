using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveScanEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookScans");

            migrationBuilder.DropTable(
                name: "DocumentScans");

            migrationBuilder.DropTable(
                name: "MagazineScans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ColorSpace = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Comments = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExifVersion = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorizontalResolution = table.Column<double>(type: "double", nullable: true),
                    OriginalExtension = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Page = table.Column<uint>(type: "int unsigned", nullable: true),
                    ResolutionUnit = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    ScannerManufacturer = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScannerModel = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareUsed = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<uint>(type: "int unsigned", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    UploadDate = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false),
                    VerticalResolution = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookScans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BookScans_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DocumentScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ColorSpace = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Comments = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExifVersion = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorizontalResolution = table.Column<double>(type: "double", nullable: true),
                    OriginalExtension = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Page = table.Column<uint>(type: "int unsigned", nullable: true),
                    ResolutionUnit = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    ScannerManufacturer = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScannerModel = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareUsed = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<uint>(type: "int unsigned", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    UploadDate = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false),
                    VerticalResolution = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentScans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DocumentScans_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MagazineScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MagazineId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ColorSpace = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    Comments = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExifVersion = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorizontalResolution = table.Column<double>(type: "double", nullable: true),
                    OriginalExtension = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Page = table.Column<uint>(type: "int unsigned", nullable: true),
                    ResolutionUnit = table.Column<ushort>(type: "smallint unsigned", nullable: true),
                    ScannerManufacturer = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScannerModel = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareUsed = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<uint>(type: "int unsigned", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    UploadDate = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false),
                    VerticalResolution = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MagazineScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MagazineScans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MagazineScans_MagazineIssues_MagazineId",
                        column: x => x.MagazineId,
                        principalTable: "MagazineIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_Author",
                table: "BookScans",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_BookId",
                table: "BookScans",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_ColorSpace",
                table: "BookScans",
                column: "ColorSpace");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_Comments",
                table: "BookScans",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_CreationDate",
                table: "BookScans",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_ExifVersion",
                table: "BookScans",
                column: "ExifVersion");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_HorizontalResolution",
                table: "BookScans",
                column: "HorizontalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_Page",
                table: "BookScans",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_ResolutionUnit",
                table: "BookScans",
                column: "ResolutionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_ScannerManufacturer",
                table: "BookScans",
                column: "ScannerManufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_ScannerModel",
                table: "BookScans",
                column: "ScannerModel");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_SoftwareUsed",
                table: "BookScans",
                column: "SoftwareUsed");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_Type",
                table: "BookScans",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_UploadDate",
                table: "BookScans",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_UserId",
                table: "BookScans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookScans_VerticalResolution",
                table: "BookScans",
                column: "VerticalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_Author",
                table: "DocumentScans",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_ColorSpace",
                table: "DocumentScans",
                column: "ColorSpace");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_Comments",
                table: "DocumentScans",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_CreationDate",
                table: "DocumentScans",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_DocumentId",
                table: "DocumentScans",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_ExifVersion",
                table: "DocumentScans",
                column: "ExifVersion");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_HorizontalResolution",
                table: "DocumentScans",
                column: "HorizontalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_Page",
                table: "DocumentScans",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_ResolutionUnit",
                table: "DocumentScans",
                column: "ResolutionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_ScannerManufacturer",
                table: "DocumentScans",
                column: "ScannerManufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_ScannerModel",
                table: "DocumentScans",
                column: "ScannerModel");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_SoftwareUsed",
                table: "DocumentScans",
                column: "SoftwareUsed");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_Type",
                table: "DocumentScans",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_UploadDate",
                table: "DocumentScans",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_UserId",
                table: "DocumentScans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentScans_VerticalResolution",
                table: "DocumentScans",
                column: "VerticalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_Author",
                table: "MagazineScans",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_ColorSpace",
                table: "MagazineScans",
                column: "ColorSpace");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_Comments",
                table: "MagazineScans",
                column: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_CreationDate",
                table: "MagazineScans",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_ExifVersion",
                table: "MagazineScans",
                column: "ExifVersion");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_HorizontalResolution",
                table: "MagazineScans",
                column: "HorizontalResolution");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_MagazineId",
                table: "MagazineScans",
                column: "MagazineId");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_Page",
                table: "MagazineScans",
                column: "Page");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_ResolutionUnit",
                table: "MagazineScans",
                column: "ResolutionUnit");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_ScannerManufacturer",
                table: "MagazineScans",
                column: "ScannerManufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_ScannerModel",
                table: "MagazineScans",
                column: "ScannerModel");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_SoftwareUsed",
                table: "MagazineScans",
                column: "SoftwareUsed");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_Type",
                table: "MagazineScans",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_UploadDate",
                table: "MagazineScans",
                column: "UploadDate");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_UserId",
                table: "MagazineScans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineScans_VerticalResolution",
                table: "MagazineScans",
                column: "VerticalResolution");
        }
    }
}
