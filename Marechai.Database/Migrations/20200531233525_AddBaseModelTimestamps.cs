using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marechai.Database.Migrations
{
    public partial class AddBaseModelTimestamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>("CreatedOn", "StorageByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "StorageByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "storage_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "storage_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "SoundByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "SoundByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "sound_synths", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "sound_synths", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "sound_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "sound_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "ScreensByMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "ScreensByMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "Screens", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "Screens", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "ResolutionsByScreen", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "ResolutionsByScreen", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "resolutions_by_gpu", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "resolutions_by_gpu", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "resolutions", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "resolutions", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "ProcessorsByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "ProcessorsByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "processors_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "processors_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "processors", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "processors", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "PeopleByMagazines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "PeopleByMagazines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "PeopleByDocuments", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "PeopleByDocuments", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "PeopleByCompany", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "PeopleByCompany", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "PeopleByBooks", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "PeopleByBooks", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "People", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "People", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "OwnedMachines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "OwnedMachines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "OwnedMachinePhotos", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "OwnedMachinePhotos", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "news", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "news", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "money_donations", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "money_donations", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "MemoryByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "MemoryByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "memory_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "memory_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "MagazinesByMachinesFamilies", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "MagazinesByMachinesFamilies", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "MagazinesByMachines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "MagazinesByMachines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "Magazines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "Magazines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "MagazineIssues", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "MagazineIssues", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "machines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "machines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "MachinePhotos", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "MachinePhotos", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "machine_families", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "machine_families", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "log", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "log", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "Licenses", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "Licenses", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "instruction_sets", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "instruction_sets", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.
                AddColumn<DateTime>("CreatedOn", "instruction_set_extensions_by_processor", nullable: false).
                Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.
                AddColumn<DateTime>("UpdatedOn", "instruction_set_extensions_by_processor", nullable: false).
                Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "instruction_set_extensions", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "instruction_set_extensions", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "GpusByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "GpusByOwnedMachine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "gpus_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "gpus_by_machine", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "gpus", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "gpus", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "forbidden", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "forbidden", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "DocumentsByMachines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "DocumentsByMachines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "DocumentsByMachineFamily", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "DocumentsByMachineFamily", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "Documents", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "Documents", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "DocumentPeople", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "DocumentPeople", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "DocumentCompanies", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "DocumentCompanies", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "CompanyDescriptions", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "CompanyDescriptions", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "company_logos", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "company_logos", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "CompaniesByMagazines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "CompaniesByMagazines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "CompaniesByDocuments", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "CompaniesByDocuments", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "CompaniesByBooks", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "CompaniesByBooks", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "companies", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "companies", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "browser_tests", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "browser_tests", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "BooksByMachines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "BooksByMachines", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "BooksByMachineFamilies", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "BooksByMachineFamilies", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<DateTime>("CreatedOn", "Books", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>("UpdatedOn", "Books", nullable: false).
                             Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("CreatedOn", "StorageByOwnedMachine");

            migrationBuilder.DropColumn("UpdatedOn", "StorageByOwnedMachine");

            migrationBuilder.DropColumn("CreatedOn", "storage_by_machine");

            migrationBuilder.DropColumn("UpdatedOn", "storage_by_machine");

            migrationBuilder.DropColumn("CreatedOn", "SoundByOwnedMachine");

            migrationBuilder.DropColumn("UpdatedOn", "SoundByOwnedMachine");

            migrationBuilder.DropColumn("CreatedOn", "sound_synths");

            migrationBuilder.DropColumn("UpdatedOn", "sound_synths");

            migrationBuilder.DropColumn("CreatedOn", "sound_by_machine");

            migrationBuilder.DropColumn("UpdatedOn", "sound_by_machine");

            migrationBuilder.DropColumn("CreatedOn", "ScreensByMachine");

            migrationBuilder.DropColumn("UpdatedOn", "ScreensByMachine");

            migrationBuilder.DropColumn("CreatedOn", "Screens");

            migrationBuilder.DropColumn("UpdatedOn", "Screens");

            migrationBuilder.DropColumn("CreatedOn", "ResolutionsByScreen");

            migrationBuilder.DropColumn("UpdatedOn", "ResolutionsByScreen");

            migrationBuilder.DropColumn("CreatedOn", "resolutions_by_gpu");

            migrationBuilder.DropColumn("UpdatedOn", "resolutions_by_gpu");

            migrationBuilder.DropColumn("CreatedOn", "resolutions");

            migrationBuilder.DropColumn("UpdatedOn", "resolutions");

            migrationBuilder.DropColumn("CreatedOn", "ProcessorsByOwnedMachine");

            migrationBuilder.DropColumn("UpdatedOn", "ProcessorsByOwnedMachine");

            migrationBuilder.DropColumn("CreatedOn", "processors_by_machine");

            migrationBuilder.DropColumn("UpdatedOn", "processors_by_machine");

            migrationBuilder.DropColumn("CreatedOn", "processors");

            migrationBuilder.DropColumn("UpdatedOn", "processors");

            migrationBuilder.DropColumn("CreatedOn", "PeopleByMagazines");

            migrationBuilder.DropColumn("UpdatedOn", "PeopleByMagazines");

            migrationBuilder.DropColumn("CreatedOn", "PeopleByDocuments");

            migrationBuilder.DropColumn("UpdatedOn", "PeopleByDocuments");

            migrationBuilder.DropColumn("CreatedOn", "PeopleByCompany");

            migrationBuilder.DropColumn("UpdatedOn", "PeopleByCompany");

            migrationBuilder.DropColumn("CreatedOn", "PeopleByBooks");

            migrationBuilder.DropColumn("UpdatedOn", "PeopleByBooks");

            migrationBuilder.DropColumn("CreatedOn", "People");

            migrationBuilder.DropColumn("UpdatedOn", "People");

            migrationBuilder.DropColumn("CreatedOn", "OwnedMachines");

            migrationBuilder.DropColumn("UpdatedOn", "OwnedMachines");

            migrationBuilder.DropColumn("CreatedOn", "OwnedMachinePhotos");

            migrationBuilder.DropColumn("UpdatedOn", "OwnedMachinePhotos");

            migrationBuilder.DropColumn("CreatedOn", "news");

            migrationBuilder.DropColumn("UpdatedOn", "news");

            migrationBuilder.DropColumn("CreatedOn", "money_donations");

            migrationBuilder.DropColumn("UpdatedOn", "money_donations");

            migrationBuilder.DropColumn("CreatedOn", "MemoryByOwnedMachine");

            migrationBuilder.DropColumn("UpdatedOn", "MemoryByOwnedMachine");

            migrationBuilder.DropColumn("CreatedOn", "memory_by_machine");

            migrationBuilder.DropColumn("UpdatedOn", "memory_by_machine");

            migrationBuilder.DropColumn("CreatedOn", "MagazinesByMachinesFamilies");

            migrationBuilder.DropColumn("UpdatedOn", "MagazinesByMachinesFamilies");

            migrationBuilder.DropColumn("CreatedOn", "MagazinesByMachines");

            migrationBuilder.DropColumn("UpdatedOn", "MagazinesByMachines");

            migrationBuilder.DropColumn("CreatedOn", "Magazines");

            migrationBuilder.DropColumn("UpdatedOn", "Magazines");

            migrationBuilder.DropColumn("CreatedOn", "MagazineIssues");

            migrationBuilder.DropColumn("UpdatedOn", "MagazineIssues");

            migrationBuilder.DropColumn("CreatedOn", "machines");

            migrationBuilder.DropColumn("UpdatedOn", "machines");

            migrationBuilder.DropColumn("CreatedOn", "MachinePhotos");

            migrationBuilder.DropColumn("UpdatedOn", "MachinePhotos");

            migrationBuilder.DropColumn("CreatedOn", "machine_families");

            migrationBuilder.DropColumn("UpdatedOn", "machine_families");

            migrationBuilder.DropColumn("CreatedOn", "log");

            migrationBuilder.DropColumn("UpdatedOn", "log");

            migrationBuilder.DropColumn("CreatedOn", "Licenses");

            migrationBuilder.DropColumn("UpdatedOn", "Licenses");

            migrationBuilder.DropColumn("CreatedOn", "instruction_sets");

            migrationBuilder.DropColumn("UpdatedOn", "instruction_sets");

            migrationBuilder.DropColumn("CreatedOn", "instruction_set_extensions_by_processor");

            migrationBuilder.DropColumn("UpdatedOn", "instruction_set_extensions_by_processor");

            migrationBuilder.DropColumn("CreatedOn", "instruction_set_extensions");

            migrationBuilder.DropColumn("UpdatedOn", "instruction_set_extensions");

            migrationBuilder.DropColumn("CreatedOn", "GpusByOwnedMachine");

            migrationBuilder.DropColumn("UpdatedOn", "GpusByOwnedMachine");

            migrationBuilder.DropColumn("CreatedOn", "gpus_by_machine");

            migrationBuilder.DropColumn("UpdatedOn", "gpus_by_machine");

            migrationBuilder.DropColumn("CreatedOn", "gpus");

            migrationBuilder.DropColumn("UpdatedOn", "gpus");

            migrationBuilder.DropColumn("CreatedOn", "forbidden");

            migrationBuilder.DropColumn("UpdatedOn", "forbidden");

            migrationBuilder.DropColumn("CreatedOn", "DocumentsByMachines");

            migrationBuilder.DropColumn("UpdatedOn", "DocumentsByMachines");

            migrationBuilder.DropColumn("CreatedOn", "DocumentsByMachineFamily");

            migrationBuilder.DropColumn("UpdatedOn", "DocumentsByMachineFamily");

            migrationBuilder.DropColumn("CreatedOn", "Documents");

            migrationBuilder.DropColumn("UpdatedOn", "Documents");

            migrationBuilder.DropColumn("CreatedOn", "DocumentPeople");

            migrationBuilder.DropColumn("UpdatedOn", "DocumentPeople");

            migrationBuilder.DropColumn("CreatedOn", "DocumentCompanies");

            migrationBuilder.DropColumn("UpdatedOn", "DocumentCompanies");

            migrationBuilder.DropColumn("CreatedOn", "CompanyDescriptions");

            migrationBuilder.DropColumn("UpdatedOn", "CompanyDescriptions");

            migrationBuilder.DropColumn("CreatedOn", "company_logos");

            migrationBuilder.DropColumn("UpdatedOn", "company_logos");

            migrationBuilder.DropColumn("CreatedOn", "CompaniesByMagazines");

            migrationBuilder.DropColumn("UpdatedOn", "CompaniesByMagazines");

            migrationBuilder.DropColumn("CreatedOn", "CompaniesByDocuments");

            migrationBuilder.DropColumn("UpdatedOn", "CompaniesByDocuments");

            migrationBuilder.DropColumn("CreatedOn", "CompaniesByBooks");

            migrationBuilder.DropColumn("UpdatedOn", "CompaniesByBooks");

            migrationBuilder.DropColumn("CreatedOn", "companies");

            migrationBuilder.DropColumn("UpdatedOn", "companies");

            migrationBuilder.DropColumn("CreatedOn", "browser_tests");

            migrationBuilder.DropColumn("UpdatedOn", "browser_tests");

            migrationBuilder.DropColumn("CreatedOn", "BooksByMachines");

            migrationBuilder.DropColumn("UpdatedOn", "BooksByMachines");

            migrationBuilder.DropColumn("CreatedOn", "BooksByMachineFamilies");

            migrationBuilder.DropColumn("UpdatedOn", "BooksByMachineFamilies");

            migrationBuilder.DropColumn("CreatedOn", "Books");

            migrationBuilder.DropColumn("UpdatedOn", "Books");
        }
    }
}