using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class RevampSoftwareEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure database and tables use correct charset/collation for foreign key compatibility
            migrationBuilder.Sql("ALTER DATABASE `marechai` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;");
            migrationBuilder.Sql("ALTER TABLE `ISO_639-3` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareFamilies_SoftwareFamilies_ParentId",
                table: "SoftwareFamilies");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVariants_SoftwareVariants_ParentId",
                table: "SoftwareVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVariants_SoftwareVersion_SoftwareVersionId",
                table: "SoftwareVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVersion_Licenses_LicenseId",
                table: "SoftwareVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVersion_SoftwareFamilies_FamilyId",
                table: "SoftwareVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVersion_SoftwareVersion_PreviousId",
                table: "SoftwareVersion");

            migrationBuilder.DropTable(
                name: "CompaniesBySoftwareFamily");

            migrationBuilder.DropTable(
                name: "CompaniesBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "CompaniesBySoftwareVersion");

            migrationBuilder.DropTable(
                name: "GpusBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "InstructionSetsBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "LanguagesBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "MachineFamiliesBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "MachinesBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "MediaBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "PeopleBySoftwareFamily");

            migrationBuilder.DropTable(
                name: "PeopleBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "PeopleBySoftwareVersion");

            migrationBuilder.DropTable(
                name: "ProcessorsBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "RequiredOperatingSystemsBySofwareVariant");

            migrationBuilder.DropTable(
                name: "RequiredSoftwareBySoftwareVariant");

            migrationBuilder.DropTable(
                name: "SoftwareVariantByCompilationMedia");

            migrationBuilder.DropTable(
                name: "SoundBySoftwareVariant");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_CatalogueNumber",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_DistributionMode",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_Introduced",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_MinimumMemory",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_Name",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_ParentId",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_PartNumber",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_ProductCode",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_RecommendedMemory",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_RequiredStorage",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_SerialNumber",
                table: "SoftwareVariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_Version",
                table: "SoftwareVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SoftwareVersion",
                table: "SoftwareVersion");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVersion_Codename",
                table: "SoftwareVersion");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVersion_Introduced",
                table: "SoftwareVersion");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVersion_Name",
                table: "SoftwareVersion");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVersion_PreviousId",
                table: "SoftwareVersion");

            migrationBuilder.DropColumn(
                name: "CatalogueNumber",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "DistributionMode",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "Introduced",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "MinimumMemory",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "PartNumber",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "RecommendedMemory",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "RequiredStorage",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "Introduced",
                table: "SoftwareVersion");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SoftwareVersion");

            migrationBuilder.RenameTable(
                name: "SoftwareVersion",
                newName: "SoftwareVersions");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "SoftwareVersions",
                newName: "VersionString");

            migrationBuilder.RenameColumn(
                name: "PreviousId",
                table: "SoftwareVersions",
                newName: "ParentVersionId");

            migrationBuilder.RenameColumn(
                name: "FamilyId",
                table: "SoftwareVersions",
                newName: "SoftwareId");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVersion_Version",
                table: "SoftwareVersions",
                newName: "IX_SoftwareVersions_VersionString");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVersion_LicenseId",
                table: "SoftwareVersions",
                newName: "IX_SoftwareVersions_LicenseId");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVersion_FamilyId",
                table: "SoftwareVersions",
                newName: "IX_SoftwareVersions_SoftwareId");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "storage_by_machine",
                type: "int(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<int>(
                name: "interface",
                table: "storage_by_machine",
                type: "int(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<ulong>(
                name: "SoftwareVersionId",
                table: "SoftwareVariants",
                type: "bigint unsigned",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned");

            migrationBuilder.UpdateData(
                table: "SoftwareVariants",
                keyColumn: "Name",
                keyValue: null,
                column: "Name",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SoftwareVariants",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<ulong>(
                name: "SoftwareId",
                table: "SoftwareVariants",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AlterColumn<sbyte>(
                name: "chars",
                table: "resolutions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "news",
                type: "int(11)",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<int>(
                name: "usage",
                table: "memory_by_machine",
                type: "int(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "memory_by_machine",
                type: "int(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "machines",
                type: "int(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "table",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "pngt",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "png",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "js",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "jpeg",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "gif89",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "gif87",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "frames",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "flash",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "colors",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<sbyte>(
                name: "agif",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: (sbyte)0,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValueSql: "'0'");

            migrationBuilder.AlterColumn<string>(
                name: "Codename",
                table: "SoftwareVersions",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicVersion",
                table: "SoftwareVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SoftwareVersions",
                table: "SoftwareVersions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SoftwareOSCompatibility",
                columns: table => new
                {
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    OSVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareOSCompatibility", x => new { x.SoftwareVersionId, x.OSVersionId });
                    table.ForeignKey(
                        name: "FK_SoftwareOSCompatibility_SoftwareVersions_OSVersionId",
                        column: x => x.OSVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwareOSCompatibility_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwarePlatforms",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwarePlatforms", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareRequirements",
                columns: table => new
                {
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RequiredSoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RequirementType = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareRequirements", x => new { x.SoftwareVersionId, x.RequiredSoftwareVersionId, x.RequirementType });
                    table.ForeignKey(
                        name: "FK_SoftwareRequirements_SoftwareVersions_RequiredSoftwareVersio~",
                        column: x => x.RequiredSoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwareRequirements_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Softwares",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FamilyId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    IsOperatingSystem = table.Column<bool>(type: "bit(1)", nullable: false),
                    IsGame = table.Column<bool>(type: "bit(1)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Softwares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Softwares_SoftwareFamilies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "SoftwareFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareSubvariants",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareSubvariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareSubvariants_SoftwareVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareVariantLanguages",
                columns: table => new
                {
                    VariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci", maxLength: 3, nullable: false),
                    LanguageId = table.Column<string>(type: "char(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVariantLanguages", x => new { x.VariantId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_SoftwareVariantLanguages_ISO_639-3_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoftwareVariantLanguages_SoftwareVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareCompanyRoles",
                columns: table => new
                {
                    SoftwareId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: false),
                    Role = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareCompanyRoles", x => new { x.SoftwareId, x.CompanyId, x.Role });
                    table.ForeignKey(
                        name: "FK_SoftwareCompanyRoles_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareCompanyRoles_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareReleases",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    VariantId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    SubvariantId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    PlatformId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    RegionId = table.Column<short>(type: "smallint(3)", nullable: false),
                    PublisherId = table.Column<int>(type: "int(11)", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareReleases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareReleases_SoftwarePlatforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "SoftwarePlatforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoftwareReleases_SoftwareSubvariants_SubvariantId",
                        column: x => x.SubvariantId,
                        principalTable: "SoftwareSubvariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoftwareReleases_SoftwareVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoftwareReleases_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareReleases_companies_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwareReleases_iso3166_1_numeric_RegionId",
                        column: x => x.RegionId,
                        principalTable: "iso3166_1_numeric",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareSubvariantLanguages",
                columns: table => new
                {
                    SubvariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci", maxLength: 3, nullable: false),
                    LanguageId = table.Column<string>(type: "char(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareSubvariantLanguages", x => new { x.SubvariantId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_SoftwareSubvariantLanguages_ISO_639-3_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoftwareSubvariantLanguages_SoftwareSubvariants_SubvariantId",
                        column: x => x.SubvariantId,
                        principalTable: "SoftwareSubvariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareBarcodes",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareBarcodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareBarcodes_SoftwareReleases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareProductCodes",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReleaseId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Issuer = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareProductCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareProductCodes_SoftwareReleases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "SoftwareReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_SoftwareId_Name",
                table: "SoftwareVariants",
                columns: new[] { "SoftwareId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersions_ParentVersionId",
                table: "SoftwareVersions",
                column: "ParentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBarcodes_Code",
                table: "SoftwareBarcodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareBarcodes_ReleaseId",
                table: "SoftwareBarcodes",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCompanyRoles_CompanyId",
                table: "SoftwareCompanyRoles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareOSCompatibility_OSVersionId",
                table: "SoftwareOSCompatibility",
                column: "OSVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePlatforms_Name",
                table: "SoftwarePlatforms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProductCodes_Issuer_Code",
                table: "SoftwareProductCodes",
                columns: new[] { "Issuer", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProductCodes_ReleaseId",
                table: "SoftwareProductCodes",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_PlatformId",
                table: "SoftwareReleases",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_PublisherId",
                table: "SoftwareReleases",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_RegionId",
                table: "SoftwareReleases",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_SoftwareVersionId_RegionId_PlatformId",
                table: "SoftwareReleases",
                columns: new[] { "SoftwareVersionId", "RegionId", "PlatformId" });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_SubvariantId",
                table: "SoftwareReleases",
                column: "SubvariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareReleases_VariantId",
                table: "SoftwareReleases",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareRequirements_RequiredSoftwareVersionId",
                table: "SoftwareRequirements",
                column: "RequiredSoftwareVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Softwares_FamilyId",
                table: "Softwares",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Softwares_Name",
                table: "Softwares",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareSubvariantLanguages_LanguageId",
                table: "SoftwareSubvariantLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareSubvariants_VariantId_Name",
                table: "SoftwareSubvariants",
                columns: new[] { "VariantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariantLanguages_LanguageId",
                table: "SoftwareVariantLanguages",
                column: "LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareFamilies_SoftwareFamilies_ParentId",
                table: "SoftwareFamilies",
                column: "ParentId",
                principalTable: "SoftwareFamilies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVariants_SoftwareVersions_SoftwareVersionId",
                table: "SoftwareVariants",
                column: "SoftwareVersionId",
                principalTable: "SoftwareVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVariants_Softwares_SoftwareId",
                table: "SoftwareVariants",
                column: "SoftwareId",
                principalTable: "Softwares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVersions_Licenses_LicenseId",
                table: "SoftwareVersions",
                column: "LicenseId",
                principalTable: "Licenses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVersions_SoftwareVersions_ParentVersionId",
                table: "SoftwareVersions",
                column: "ParentVersionId",
                principalTable: "SoftwareVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVersions_Softwares_SoftwareId",
                table: "SoftwareVersions",
                column: "SoftwareId",
                principalTable: "Softwares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareFamilies_SoftwareFamilies_ParentId",
                table: "SoftwareFamilies");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVariants_SoftwareVersions_SoftwareVersionId",
                table: "SoftwareVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVariants_Softwares_SoftwareId",
                table: "SoftwareVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVersions_Licenses_LicenseId",
                table: "SoftwareVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVersions_SoftwareVersions_ParentVersionId",
                table: "SoftwareVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareVersions_Softwares_SoftwareId",
                table: "SoftwareVersions");

            migrationBuilder.DropTable(
                name: "SoftwareBarcodes");

            migrationBuilder.DropTable(
                name: "SoftwareCompanyRoles");

            migrationBuilder.DropTable(
                name: "SoftwareOSCompatibility");

            migrationBuilder.DropTable(
                name: "SoftwareProductCodes");

            migrationBuilder.DropTable(
                name: "SoftwareRequirements");

            migrationBuilder.DropTable(
                name: "SoftwareSubvariantLanguages");

            migrationBuilder.DropTable(
                name: "SoftwareVariantLanguages");

            migrationBuilder.DropTable(
                name: "Softwares");

            migrationBuilder.DropTable(
                name: "SoftwareReleases");

            migrationBuilder.DropTable(
                name: "SoftwarePlatforms");

            migrationBuilder.DropTable(
                name: "SoftwareSubvariants");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVariants_SoftwareId_Name",
                table: "SoftwareVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SoftwareVersions",
                table: "SoftwareVersions");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareVersions_ParentVersionId",
                table: "SoftwareVersions");

            migrationBuilder.DropColumn(
                name: "SoftwareId",
                table: "SoftwareVariants");

            migrationBuilder.DropColumn(
                name: "PublicVersion",
                table: "SoftwareVersions");

            migrationBuilder.RenameTable(
                name: "SoftwareVersions",
                newName: "SoftwareVersion");

            migrationBuilder.RenameColumn(
                name: "VersionString",
                table: "SoftwareVersion",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "SoftwareId",
                table: "SoftwareVersion",
                newName: "FamilyId");

            migrationBuilder.RenameColumn(
                name: "ParentVersionId",
                table: "SoftwareVersion",
                newName: "PreviousId");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVersions_VersionString",
                table: "SoftwareVersion",
                newName: "IX_SoftwareVersion_Version");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVersions_SoftwareId",
                table: "SoftwareVersion",
                newName: "IX_SoftwareVersion_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareVersions_LicenseId",
                table: "SoftwareVersion",
                newName: "IX_SoftwareVersion_LicenseId");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "storage_by_machine",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "interface",
                table: "storage_by_machine",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<ulong>(
                name: "SoftwareVersionId",
                table: "SoftwareVariants",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "bigint unsigned",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SoftwareVariants",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CatalogueNumber",
                table: "SoftwareVariants",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<uint>(
                name: "DistributionMode",
                table: "SoftwareVariants",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<DateTime>(
                name: "Introduced",
                table: "SoftwareVariants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "MinimumMemory",
                table: "SoftwareVariants",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "ParentId",
                table: "SoftwareVariants",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartNumber",
                table: "SoftwareVariants",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "SoftwareVariants",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<ulong>(
                name: "RecommendedMemory",
                table: "SoftwareVariants",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "RequiredStorage",
                table: "SoftwareVariants",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "SoftwareVariants",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "SoftwareVariants",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<sbyte>(
                name: "chars",
                table: "resolutions",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "news",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "usage",
                table: "memory_by_machine",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "memory_by_machine",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "machines",
                type: "int(11)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(int),
                oldType: "int(11)",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "table",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "pngt",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "png",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "js",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "jpeg",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "gif89",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "gif87",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "frames",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "flash",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "colors",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<sbyte>(
                name: "agif",
                table: "browser_tests",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'",
                oldClrType: typeof(sbyte),
                oldType: "tinyint(1)",
                oldDefaultValue: (sbyte)0);

            migrationBuilder.AlterColumn<string>(
                name: "Codename",
                table: "SoftwareVersion",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "Introduced",
                table: "SoftwareVersion",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SoftwareVersion",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SoftwareVersion",
                table: "SoftwareVersion",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CompaniesBySoftwareFamily",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareFamilyId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesBySoftwareFamily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareFamily_DocumentRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DocumentRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareFamily_SoftwareFamilies_SoftwareFamilyId",
                        column: x => x.SoftwareFamilyId,
                        principalTable: "SoftwareFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareFamily_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariant_DocumentRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DocumentRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariant_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesBySoftwareVersion",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesBySoftwareVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVersion_DocumentRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DocumentRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVersion_SoftwareVersion_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVersion_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GpusBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GpuId = table.Column<int>(type: "int(11)", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Minimum = table.Column<bool>(type: "bit(1)", nullable: true),
                    Recommended = table.Column<bool>(type: "bit(1)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpusBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GpusBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GpusBySoftwareVariant_gpus_GpuId",
                        column: x => x.GpuId,
                        principalTable: "gpus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InstructionSetsBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InstructionSetId = table.Column<int>(type: "int(11)", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructionSetsBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructionSetsBySoftwareVariant_SoftwareVariants_SoftwareVa~",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstructionSetsBySoftwareVariant_instruction_sets_Instructio~",
                        column: x => x.InstructionSetId,
                        principalTable: "instruction_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LanguagesBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LanguageId = table.Column<string>(type: "char(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguagesBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LanguagesBySoftwareVariant_ISO_639-3_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "ISO_639-3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguagesBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MachineFamiliesBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MachineFamilyId = table.Column<int>(type: "int(11)", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineFamiliesBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineFamiliesBySoftwareVariant_SoftwareVariants_SoftwareVa~",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MachineFamiliesBySoftwareVariant_machine_families_MachineFam~",
                        column: x => x.MachineFamilyId,
                        principalTable: "machine_families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MachinesBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MachineId = table.Column<int>(type: "int(11)", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachinesBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachinesBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MachinesBySoftwareVariant_machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "machines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MediaBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MediaId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaBySoftwareVariant_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeopleBySoftwareFamily",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareFamilyId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleBySoftwareFamily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareFamily_DocumentRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DocumentRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareFamily_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareFamily_SoftwareFamilies_SoftwareFamilyId",
                        column: x => x.SoftwareFamilyId,
                        principalTable: "SoftwareFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeopleBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareVariant_DocumentRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DocumentRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareVariant_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PeopleBySoftwareVersion",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleBySoftwareVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareVersion_DocumentRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DocumentRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareVersion_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeopleBySoftwareVersion_SoftwareVersion_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProcessorsBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProcessorId = table.Column<int>(type: "int(11)", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Minimum = table.Column<bool>(type: "bit(1)", nullable: true),
                    Recommended = table.Column<bool>(type: "bit(1)", nullable: true),
                    Speed = table.Column<float>(type: "float", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessorsBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessorsBySoftwareVariant_SoftwareVariants_SoftwareVariant~",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessorsBySoftwareVariant_processors_ProcessorId",
                        column: x => x.ProcessorId,
                        principalTable: "processors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequiredOperatingSystemsBySofwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OperatingSystemId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredOperatingSystemsBySofwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequiredOperatingSystemsBySofwareVariant_SoftwareVariants_So~",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequiredOperatingSystemsBySofwareVariant_SoftwareVersion_Ope~",
                        column: x => x.OperatingSystemId,
                        principalTable: "SoftwareVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequiredSoftwareBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredSoftwareBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequiredSoftwareBySoftwareVariant_SoftwareVariants_SoftwareV~",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequiredSoftwareBySoftwareVariant_SoftwareVersion_SoftwareVe~",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoftwareVariantByCompilationMedia",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MediaId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Path = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PathSeparator = table.Column<string>(type: "varchar(1)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVariantByCompilationMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareVariantByCompilationMedia_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareVariantByCompilationMedia_SoftwareVariants_SoftwareV~",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SoundBySoftwareVariant",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    SoundSynthId = table.Column<int>(type: "int(11)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundBySoftwareVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoundBySoftwareVariant_SoftwareVariants_SoftwareVariantId",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoundBySoftwareVariant_sound_synths_SoundSynthId",
                        column: x => x.SoundSynthId,
                        principalTable: "sound_synths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_CatalogueNumber",
                table: "SoftwareVariants",
                column: "CatalogueNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_DistributionMode",
                table: "SoftwareVariants",
                column: "DistributionMode");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_Introduced",
                table: "SoftwareVariants",
                column: "Introduced");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_MinimumMemory",
                table: "SoftwareVariants",
                column: "MinimumMemory");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_Name",
                table: "SoftwareVariants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_ParentId",
                table: "SoftwareVariants",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_PartNumber",
                table: "SoftwareVariants",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_ProductCode",
                table: "SoftwareVariants",
                column: "ProductCode");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_RecommendedMemory",
                table: "SoftwareVariants",
                column: "RecommendedMemory");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_RequiredStorage",
                table: "SoftwareVariants",
                column: "RequiredStorage");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_SerialNumber",
                table: "SoftwareVariants",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariants_Version",
                table: "SoftwareVariants",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersion_Codename",
                table: "SoftwareVersion",
                column: "Codename");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersion_Introduced",
                table: "SoftwareVersion",
                column: "Introduced");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersion_Name",
                table: "SoftwareVersion",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersion_PreviousId",
                table: "SoftwareVersion",
                column: "PreviousId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareFamily_CompanyId",
                table: "CompaniesBySoftwareFamily",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareFamily_RoleId",
                table: "CompaniesBySoftwareFamily",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareFamily_SoftwareFamilyId",
                table: "CompaniesBySoftwareFamily",
                column: "SoftwareFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariant_CompanyId",
                table: "CompaniesBySoftwareVariant",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariant_RoleId",
                table: "CompaniesBySoftwareVariant",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariant_SoftwareVariantId",
                table: "CompaniesBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVersion_CompanyId",
                table: "CompaniesBySoftwareVersion",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVersion_RoleId",
                table: "CompaniesBySoftwareVersion",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVersion_SoftwareVersionId",
                table: "CompaniesBySoftwareVersion",
                column: "SoftwareVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_GpusBySoftwareVariant_GpuId",
                table: "GpusBySoftwareVariant",
                column: "GpuId");

            migrationBuilder.CreateIndex(
                name: "IX_GpusBySoftwareVariant_Minimum",
                table: "GpusBySoftwareVariant",
                column: "Minimum");

            migrationBuilder.CreateIndex(
                name: "IX_GpusBySoftwareVariant_Recommended",
                table: "GpusBySoftwareVariant",
                column: "Recommended");

            migrationBuilder.CreateIndex(
                name: "IX_GpusBySoftwareVariant_SoftwareVariantId",
                table: "GpusBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructionSetsBySoftwareVariant_InstructionSetId",
                table: "InstructionSetsBySoftwareVariant",
                column: "InstructionSetId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructionSetsBySoftwareVariant_SoftwareVariantId",
                table: "InstructionSetsBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguagesBySoftwareVariant_LanguageId",
                table: "LanguagesBySoftwareVariant",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguagesBySoftwareVariant_SoftwareVariantId",
                table: "LanguagesBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineFamiliesBySoftwareVariant_MachineFamilyId",
                table: "MachineFamiliesBySoftwareVariant",
                column: "MachineFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineFamiliesBySoftwareVariant_SoftwareVariantId",
                table: "MachineFamiliesBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_MachinesBySoftwareVariant_MachineId",
                table: "MachinesBySoftwareVariant",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachinesBySoftwareVariant_SoftwareVariantId",
                table: "MachinesBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaBySoftwareVariant_MediaId",
                table: "MediaBySoftwareVariant",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaBySoftwareVariant_SoftwareVariantId",
                table: "MediaBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareFamily_PersonId",
                table: "PeopleBySoftwareFamily",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareFamily_RoleId",
                table: "PeopleBySoftwareFamily",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareFamily_SoftwareFamilyId",
                table: "PeopleBySoftwareFamily",
                column: "SoftwareFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareVariant_PersonId",
                table: "PeopleBySoftwareVariant",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareVariant_RoleId",
                table: "PeopleBySoftwareVariant",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareVariant_SoftwareVariantId",
                table: "PeopleBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareVersion_PersonId",
                table: "PeopleBySoftwareVersion",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareVersion_RoleId",
                table: "PeopleBySoftwareVersion",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleBySoftwareVersion_SoftwareVersionId",
                table: "PeopleBySoftwareVersion",
                column: "SoftwareVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorsBySoftwareVariant_Minimum",
                table: "ProcessorsBySoftwareVariant",
                column: "Minimum");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorsBySoftwareVariant_ProcessorId",
                table: "ProcessorsBySoftwareVariant",
                column: "ProcessorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorsBySoftwareVariant_Recommended",
                table: "ProcessorsBySoftwareVariant",
                column: "Recommended");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorsBySoftwareVariant_SoftwareVariantId",
                table: "ProcessorsBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessorsBySoftwareVariant_Speed",
                table: "ProcessorsBySoftwareVariant",
                column: "Speed");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredOperatingSystemsBySofwareVariant_OperatingSystemId",
                table: "RequiredOperatingSystemsBySofwareVariant",
                column: "OperatingSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredOperatingSystemsBySofwareVariant_SoftwareVariantId",
                table: "RequiredOperatingSystemsBySofwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredSoftwareBySoftwareVariant_SoftwareVariantId",
                table: "RequiredSoftwareBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredSoftwareBySoftwareVariant_SoftwareVersionId",
                table: "RequiredSoftwareBySoftwareVariant",
                column: "SoftwareVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariantByCompilationMedia_MediaId",
                table: "SoftwareVariantByCompilationMedia",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariantByCompilationMedia_Path",
                table: "SoftwareVariantByCompilationMedia",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVariantByCompilationMedia_SoftwareVariantId",
                table: "SoftwareVariantByCompilationMedia",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundBySoftwareVariant_SoftwareVariantId",
                table: "SoundBySoftwareVariant",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundBySoftwareVariant_SoundSynthId",
                table: "SoundBySoftwareVariant",
                column: "SoundSynthId");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareFamilies_SoftwareFamilies_ParentId",
                table: "SoftwareFamilies",
                column: "ParentId",
                principalTable: "SoftwareFamilies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVariants_SoftwareVariants_ParentId",
                table: "SoftwareVariants",
                column: "ParentId",
                principalTable: "SoftwareVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVariants_SoftwareVersion_SoftwareVersionId",
                table: "SoftwareVariants",
                column: "SoftwareVersionId",
                principalTable: "SoftwareVersion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVersion_Licenses_LicenseId",
                table: "SoftwareVersion",
                column: "LicenseId",
                principalTable: "Licenses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVersion_SoftwareFamilies_FamilyId",
                table: "SoftwareVersion",
                column: "FamilyId",
                principalTable: "SoftwareFamilies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareVersion_SoftwareVersion_PreviousId",
                table: "SoftwareVersion",
                column: "PreviousId",
                principalTable: "SoftwareVersion",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
