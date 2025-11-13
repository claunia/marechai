using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddMagazineIssueNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if the column already exists before adding it (idempotent)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'MagazineIssues'
                    AND COLUMN_NAME = 'IssueNumber'
                )
                THEN
                    ALTER TABLE `MagazineIssues` ADD `IssueNumber` int unsigned NULL;
                END IF;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Check if the column exists before dropping it (idempotent)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'MagazineIssues'
                    AND COLUMN_NAME = 'IssueNumber'
                )
                THEN
                    ALTER TABLE `MagazineIssues` DROP COLUMN `IssueNumber`;
                END IF;
            ");
        }
    }
}