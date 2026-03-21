using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareRoleAndCompanyJunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create SoftwareRoles table FIRST (needed before FK references)
            migrationBuilder.Sql(
                "CREATE TABLE IF NOT EXISTS `SoftwareRoles` (" +
                "`Id` char(3) CHARACTER SET utf8mb4 NOT NULL, " +
                "`Name` varchar(255) CHARACTER SET utf8mb4 NULL, " +
                "`Enabled` bit(1) NOT NULL DEFAULT TRUE, " +
                "CONSTRAINT `PK_SoftwareRoles` PRIMARY KEY (`Id`)" +
                ") CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS `IX_SoftwareRoles_Enabled` ON `SoftwareRoles` (`Enabled`);");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS `IX_SoftwareRoles_Name` ON `SoftwareRoles` (`Name`);");

            // Step 2: Seed SoftwareRoles with initial data
            migrationBuilder.Sql(
                "INSERT IGNORE INTO SoftwareRoles (Id, Name, Enabled) VALUES " +
                "('dev', 'Developer', 1), " +
                "('pub', 'Publisher', 1), " +
                "('dis', 'Distributor', 1), " +
                "('por', 'Porter', 1), " +
                "('loc', 'Localizer', 1), " +
                "('mfg', 'Manufacturer', 1)");

            // Step 3: Disable FK checks for the entire SoftwareCompanyRoles modification
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=0;");

            // Step 4: Add RoleId column if it doesn't exist, migrate data, drop Role
            migrationBuilder.Sql(
                "SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCompanyRoles' AND COLUMN_NAME = 'RoleId'); " +
                "SET @sql = IF(@col_exists = 0, " +
                "'ALTER TABLE SoftwareCompanyRoles ADD COLUMN RoleId char(3) CHARACTER SET utf8mb4 NULL', " +
                "'SELECT 1'); " +
                "PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;");

            // Step 5: Migrate existing Role string values to RoleId FK values
            migrationBuilder.Sql(
                "UPDATE SoftwareCompanyRoles SET RoleId = CASE " +
                "WHEN LOWER(Role) = 'developer' THEN 'dev' " +
                "WHEN LOWER(Role) = 'publisher' THEN 'pub' " +
                "WHEN LOWER(Role) = 'distributor' THEN 'dis' " +
                "WHEN LOWER(Role) = 'porter' THEN 'por' " +
                "WHEN LOWER(Role) = 'localizer' THEN 'loc' " +
                "WHEN LOWER(Role) = 'manufacturer' THEN 'mfg' " +
                "ELSE 'dev' END " +
                "WHERE RoleId IS NULL;");

            // Step 6: Make RoleId non-nullable
            migrationBuilder.Sql(
                "ALTER TABLE SoftwareCompanyRoles MODIFY COLUMN RoleId char(3) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';");

            // Step 7: Drop old PK, drop Role column, add new PK
            migrationBuilder.Sql(
                "ALTER TABLE SoftwareCompanyRoles DROP PRIMARY KEY;");

            migrationBuilder.Sql(
                "SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCompanyRoles' AND COLUMN_NAME = 'Role'); " +
                "SET @sql = IF(@col_exists > 0, " +
                "'ALTER TABLE SoftwareCompanyRoles DROP COLUMN Role', " +
                "'SELECT 1'); " +
                "PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;");

            migrationBuilder.Sql(
                "ALTER TABLE SoftwareCompanyRoles ADD PRIMARY KEY (SoftwareId, CompanyId, RoleId);");

            // Step 8: Re-enable FK checks
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS=1;");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS `IX_SoftwareCompanyRoles_RoleId` ON `SoftwareCompanyRoles` (`RoleId`);");

            migrationBuilder.Sql(
                "SET @fk_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS " +
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SoftwareCompanyRoles' " +
                "AND CONSTRAINT_NAME = 'FK_SoftwareCompanyRoles_SoftwareRoles_RoleId'); " +
                "SET @sql = IF(@fk_exists = 0, " +
                "'ALTER TABLE SoftwareCompanyRoles ADD CONSTRAINT FK_SoftwareCompanyRoles_SoftwareRoles_RoleId " +
                "FOREIGN KEY (RoleId) REFERENCES SoftwareRoles(Id) ON DELETE CASCADE', " +
                "'SELECT 1'); " +
                "PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;");

            // Step 9: Create CompanyBy* junction tables
            migrationBuilder.CreateTable(
                name: "CompaniesBySoftwareFamilies",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: false),
                    SoftwareFamilyId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesBySoftwareFamilies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareFamilies_SoftwareFamilies_SoftwareFamilyId",
                        column: x => x.SoftwareFamilyId,
                        principalTable: "SoftwareFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareFamilies_SoftwareRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SoftwareRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareFamilies_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesBySoftwareVariants",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: false),
                    SoftwareVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesBySoftwareVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariants_SoftwareRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SoftwareRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariants_SoftwareVariants_SoftwareVariant~",
                        column: x => x.SoftwareVariantId,
                        principalTable: "SoftwareVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVariants_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompaniesBySoftwareVersions",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int(11)", nullable: false),
                    SoftwareVersionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleId = table.Column<string>(type: "char(3)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompaniesBySoftwareVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVersions_SoftwareRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SoftwareRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVersions_SoftwareVersions_SoftwareVersion~",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompaniesBySoftwareVersions_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareCompanyRoles_RoleId",
                table: "SoftwareCompanyRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareFamilies_CompanyId",
                table: "CompaniesBySoftwareFamilies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareFamilies_RoleId",
                table: "CompaniesBySoftwareFamilies",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareFamilies_SoftwareFamilyId",
                table: "CompaniesBySoftwareFamilies",
                column: "SoftwareFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariants_CompanyId",
                table: "CompaniesBySoftwareVariants",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariants_RoleId",
                table: "CompaniesBySoftwareVariants",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVariants_SoftwareVariantId",
                table: "CompaniesBySoftwareVariants",
                column: "SoftwareVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVersions_CompanyId",
                table: "CompaniesBySoftwareVersions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVersions_RoleId",
                table: "CompaniesBySoftwareVersions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CompaniesBySoftwareVersions_SoftwareVersionId",
                table: "CompaniesBySoftwareVersions",
                column: "SoftwareVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareCompanyRoles_SoftwareRoles_RoleId",
                table: "SoftwareCompanyRoles");

            migrationBuilder.DropTable(
                name: "CompaniesBySoftwareFamilies");

            migrationBuilder.DropTable(
                name: "CompaniesBySoftwareVariants");

            migrationBuilder.DropTable(
                name: "CompaniesBySoftwareVersions");

            migrationBuilder.DropTable(
                name: "SoftwareRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SoftwareCompanyRoles",
                table: "SoftwareCompanyRoles");

            migrationBuilder.DropIndex(
                name: "IX_SoftwareCompanyRoles_RoleId",
                table: "SoftwareCompanyRoles");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "SoftwareCompanyRoles");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "SoftwareCompanyRoles",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SoftwareCompanyRoles",
                table: "SoftwareCompanyRoles",
                columns: new[] { "SoftwareId", "CompanyId", "Role" });
        }
    }
}
