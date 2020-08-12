using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddDocumentScans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("BookScans", table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Author               = table.Column<string>(nullable: true),
                ColorSpace           = table.Column<ushort>(nullable: true),
                Comments             = table.Column<string>(nullable: true),
                CreationDate         = table.Column<DateTime>(nullable: true),
                ExifVersion          = table.Column<string>(nullable: true),
                HorizontalResolution = table.Column<double>(nullable: true),
                ResolutionUnit       = table.Column<ushort>(nullable: true),
                ScannerManufacturer  = table.Column<string>(nullable: true),
                ScannerModel         = table.Column<string>(nullable: true),
                SoftwareUsed         = table.Column<string>(nullable: true),
                UploadDate = table.Column<DateTime>(nullable: false).
                                   Annotation("MySql:ValueGenerationStrategy",
                                              MySqlValueGenerationStrategy.ComputedColumn),
                VerticalResolution = table.Column<double>(nullable: true),
                OriginalExtension  = table.Column<string>(nullable: true),
                UserId             = table.Column<string>(nullable: true),
                Type               = table.Column<uint>(nullable: false),
                Page               = table.Column<uint>(nullable: true),
                BookId             = table.Column<long>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_BookScans", x => x.Id);

                table.ForeignKey("FK_BookScans_Books_BookId", x => x.BookId, "Books", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_BookScans_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.SetNull);
            });

            migrationBuilder.CreateTable("DocumentScans", table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Author               = table.Column<string>(nullable: true),
                ColorSpace           = table.Column<ushort>(nullable: true),
                Comments             = table.Column<string>(nullable: true),
                CreationDate         = table.Column<DateTime>(nullable: true),
                ExifVersion          = table.Column<string>(nullable: true),
                HorizontalResolution = table.Column<double>(nullable: true),
                ResolutionUnit       = table.Column<ushort>(nullable: true),
                ScannerManufacturer  = table.Column<string>(nullable: true),
                ScannerModel         = table.Column<string>(nullable: true),
                SoftwareUsed         = table.Column<string>(nullable: true),
                UploadDate = table.Column<DateTime>(nullable: false).
                                   Annotation("MySql:ValueGenerationStrategy",
                                              MySqlValueGenerationStrategy.ComputedColumn),
                VerticalResolution = table.Column<double>(nullable: true),
                OriginalExtension  = table.Column<string>(nullable: true),
                UserId             = table.Column<string>(nullable: true),
                Type               = table.Column<uint>(nullable: false),
                Page               = table.Column<uint>(nullable: true),
                DocumentId         = table.Column<long>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_DocumentScans", x => x.Id);

                table.ForeignKey("FK_DocumentScans_Documents_DocumentId", x => x.DocumentId, "Documents", "Id",
                                 onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_DocumentScans_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.SetNull);
            });

            migrationBuilder.CreateTable("MagazineScans", table => new
            {
                Id = table.Column<Guid>(nullable: false),
                CreatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.IdentityColumn),
                UpdatedOn = table.Column<DateTime>(nullable: false).
                                  Annotation("MySql:ValueGenerationStrategy",
                                             MySqlValueGenerationStrategy.ComputedColumn),
                Author               = table.Column<string>(nullable: true),
                ColorSpace           = table.Column<ushort>(nullable: true),
                Comments             = table.Column<string>(nullable: true),
                CreationDate         = table.Column<DateTime>(nullable: true),
                ExifVersion          = table.Column<string>(nullable: true),
                HorizontalResolution = table.Column<double>(nullable: true),
                ResolutionUnit       = table.Column<ushort>(nullable: true),
                ScannerManufacturer  = table.Column<string>(nullable: true),
                ScannerModel         = table.Column<string>(nullable: true),
                SoftwareUsed         = table.Column<string>(nullable: true),
                UploadDate = table.Column<DateTime>(nullable: false).
                                   Annotation("MySql:ValueGenerationStrategy",
                                              MySqlValueGenerationStrategy.ComputedColumn),
                VerticalResolution = table.Column<double>(nullable: true),
                OriginalExtension  = table.Column<string>(nullable: true),
                UserId             = table.Column<string>(nullable: true),
                Type               = table.Column<uint>(nullable: false),
                Page               = table.Column<uint>(nullable: true),
                MagazineId         = table.Column<long>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_MagazineScans", x => x.Id);

                table.ForeignKey("FK_MagazineScans_MagazineIssues_MagazineId", x => x.MagazineId, "MagazineIssues",
                                 "Id", onDelete: ReferentialAction.Cascade);

                table.ForeignKey("FK_MagazineScans_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id",
                                 onDelete: ReferentialAction.SetNull);
            });

            migrationBuilder.CreateIndex("IX_BookScans_Author", "BookScans", "Author");

            migrationBuilder.CreateIndex("IX_BookScans_BookId", "BookScans", "BookId");

            migrationBuilder.CreateIndex("IX_BookScans_ColorSpace", "BookScans", "ColorSpace");

            migrationBuilder.CreateIndex("IX_BookScans_Comments", "BookScans", "Comments");

            migrationBuilder.CreateIndex("IX_BookScans_CreationDate", "BookScans", "CreationDate");

            migrationBuilder.CreateIndex("IX_BookScans_ExifVersion", "BookScans", "ExifVersion");

            migrationBuilder.CreateIndex("IX_BookScans_HorizontalResolution", "BookScans", "HorizontalResolution");

            migrationBuilder.CreateIndex("IX_BookScans_Page", "BookScans", "Page");

            migrationBuilder.CreateIndex("IX_BookScans_ResolutionUnit", "BookScans", "ResolutionUnit");

            migrationBuilder.CreateIndex("IX_BookScans_ScannerManufacturer", "BookScans", "ScannerManufacturer");

            migrationBuilder.CreateIndex("IX_BookScans_ScannerModel", "BookScans", "ScannerModel");

            migrationBuilder.CreateIndex("IX_BookScans_SoftwareUsed", "BookScans", "SoftwareUsed");

            migrationBuilder.CreateIndex("IX_BookScans_Type", "BookScans", "Type");

            migrationBuilder.CreateIndex("IX_BookScans_UploadDate", "BookScans", "UploadDate");

            migrationBuilder.CreateIndex("IX_BookScans_UserId", "BookScans", "UserId");

            migrationBuilder.CreateIndex("IX_BookScans_VerticalResolution", "BookScans", "VerticalResolution");

            migrationBuilder.CreateIndex("IX_DocumentScans_Author", "DocumentScans", "Author");

            migrationBuilder.CreateIndex("IX_DocumentScans_ColorSpace", "DocumentScans", "ColorSpace");

            migrationBuilder.CreateIndex("IX_DocumentScans_Comments", "DocumentScans", "Comments");

            migrationBuilder.CreateIndex("IX_DocumentScans_CreationDate", "DocumentScans", "CreationDate");

            migrationBuilder.CreateIndex("IX_DocumentScans_DocumentId", "DocumentScans", "DocumentId");

            migrationBuilder.CreateIndex("IX_DocumentScans_ExifVersion", "DocumentScans", "ExifVersion");

            migrationBuilder.CreateIndex("IX_DocumentScans_HorizontalResolution", "DocumentScans",
                                         "HorizontalResolution");

            migrationBuilder.CreateIndex("IX_DocumentScans_Page", "DocumentScans", "Page");

            migrationBuilder.CreateIndex("IX_DocumentScans_ResolutionUnit", "DocumentScans", "ResolutionUnit");

            migrationBuilder.CreateIndex("IX_DocumentScans_ScannerManufacturer", "DocumentScans",
                                         "ScannerManufacturer");

            migrationBuilder.CreateIndex("IX_DocumentScans_ScannerModel", "DocumentScans", "ScannerModel");

            migrationBuilder.CreateIndex("IX_DocumentScans_SoftwareUsed", "DocumentScans", "SoftwareUsed");

            migrationBuilder.CreateIndex("IX_DocumentScans_Type", "DocumentScans", "Type");

            migrationBuilder.CreateIndex("IX_DocumentScans_UploadDate", "DocumentScans", "UploadDate");

            migrationBuilder.CreateIndex("IX_DocumentScans_UserId", "DocumentScans", "UserId");

            migrationBuilder.CreateIndex("IX_DocumentScans_VerticalResolution", "DocumentScans", "VerticalResolution");

            migrationBuilder.CreateIndex("IX_MagazineScans_Author", "MagazineScans", "Author");

            migrationBuilder.CreateIndex("IX_MagazineScans_ColorSpace", "MagazineScans", "ColorSpace");

            migrationBuilder.CreateIndex("IX_MagazineScans_Comments", "MagazineScans", "Comments");

            migrationBuilder.CreateIndex("IX_MagazineScans_CreationDate", "MagazineScans", "CreationDate");

            migrationBuilder.CreateIndex("IX_MagazineScans_ExifVersion", "MagazineScans", "ExifVersion");

            migrationBuilder.CreateIndex("IX_MagazineScans_HorizontalResolution", "MagazineScans",
                                         "HorizontalResolution");

            migrationBuilder.CreateIndex("IX_MagazineScans_MagazineId", "MagazineScans", "MagazineId");

            migrationBuilder.CreateIndex("IX_MagazineScans_Page", "MagazineScans", "Page");

            migrationBuilder.CreateIndex("IX_MagazineScans_ResolutionUnit", "MagazineScans", "ResolutionUnit");

            migrationBuilder.CreateIndex("IX_MagazineScans_ScannerManufacturer", "MagazineScans",
                                         "ScannerManufacturer");

            migrationBuilder.CreateIndex("IX_MagazineScans_ScannerModel", "MagazineScans", "ScannerModel");

            migrationBuilder.CreateIndex("IX_MagazineScans_SoftwareUsed", "MagazineScans", "SoftwareUsed");

            migrationBuilder.CreateIndex("IX_MagazineScans_Type", "MagazineScans", "Type");

            migrationBuilder.CreateIndex("IX_MagazineScans_UploadDate", "MagazineScans", "UploadDate");

            migrationBuilder.CreateIndex("IX_MagazineScans_UserId", "MagazineScans", "UserId");

            migrationBuilder.CreateIndex("IX_MagazineScans_VerticalResolution", "MagazineScans", "VerticalResolution");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("BookScans");

            migrationBuilder.DropTable("DocumentScans");

            migrationBuilder.DropTable("MagazineScans");
        }
    }
}