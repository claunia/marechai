/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.Database.Schemas;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace Marechai.Database.Models
{
    public class MarechaiContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        readonly ValueConverter<string, byte[]> hexToBytesConverter =
            new(v => HexStringToBytesConverter.StringToHex(v), v => HexStringToBytesConverter.HexToString(v));

        public MarechaiContext() {}

        public MarechaiContext(DbContextOptions<MarechaiContext> options) : base(options) {}

        public virtual DbSet<Audit>                               Audit                               { get; set; }
        public virtual DbSet<Book>                                Books                               { get; set; }
        public virtual DbSet<BooksByMachine>                      BooksByMachines                     { get; set; }
        public virtual DbSet<BooksByMachineFamily>                BooksByMachineFamilies              { get; set; }
        public virtual DbSet<BookScan>                            BookScans                           { get; set; }
        public virtual DbSet<BrowserTest>                         BrowserTests                        { get; set; }
        public virtual DbSet<CompaniesByBook>                     CompaniesByBooks                    { get; set; }
        public virtual DbSet<CompaniesByDocument>                 CompaniesByDocuments                { get; set; }
        public virtual DbSet<CompaniesByMagazine>                 CompaniesByMagazines                { get; set; }
        public virtual DbSet<CompaniesBySoftwareFamily>           CompaniesBySoftwareFamilies         { get; set; }
        public virtual DbSet<CompaniesBySoftwareVariant>          CompaniesBySoftwareVariants         { get; set; }
        public virtual DbSet<CompaniesBySoftwareVersion>          CompaniesBySoftwareVersions         { get; set; }
        public virtual DbSet<Company>                             Companies                           { get; set; }
        public virtual DbSet<CompanyDescription>                  CompanyDescriptions                 { get; set; }
        public virtual DbSet<CompanyLogo>                         CompanyLogos                        { get; set; }
        public virtual DbSet<CurrencyInflation>                   CurrenciesInflation                 { get; set; }
        public virtual DbSet<CurrencyPegging>                     CurrenciesPegging                   { get; set; }
        public virtual DbSet<DbFile>                              Files                               { get; set; }
        public virtual DbSet<Document>                            Documents                           { get; set; }
        public virtual DbSet<DocumentCompany>                     DocumentCompanies                   { get; set; }
        public virtual DbSet<DocumentPerson>                      DocumentPeople                      { get; set; }
        public virtual DbSet<DocumentRole>                        DocumentRoles                       { get; set; }
        public virtual DbSet<DocumentsByMachine>                  DocumentsByMachines                 { get; set; }
        public virtual DbSet<DocumentsByMachineFamily>            DocumentsByMachineFamilies          { get; set; }
        public virtual DbSet<DocumentScan>                        DocumentScans                       { get; set; }
        public virtual DbSet<Dump>                                Dumps                               { get; set; }
        public virtual DbSet<DumpHardware>                        DumpHardwares                       { get; set; }
        public virtual DbSet<FileDataStream>                      FileDataStreams                     { get; set; }
        public virtual DbSet<Filesystem>                          Filesystems                         { get; set; }
        public virtual DbSet<FilesystemsByLogicalPartition>       FilesystemsByLogicalPartition       { get; set; }
        public virtual DbSet<Forbidden>                           Forbidden                           { get; set; }
        public virtual DbSet<Gpu>                                 Gpus                                { get; set; }
        public virtual DbSet<GpusByMachine>                       GpusByMachine                       { get; set; }
        public virtual DbSet<GpusByOwnedMachine>                  GpusByOwnedMachine                  { get; set; }
        public virtual DbSet<InstructionSet>                      InstructionSets                     { get; set; }
        public virtual DbSet<InstructionSetExtension>             InstructionSetExtensions            { get; set; }
        public virtual DbSet<InstructionSetExtensionsByProcessor> InstructionSetExtensionsByProcessor { get; set; }
        public virtual DbSet<Iso31661Numeric>                     Iso31661Numeric                     { get; set; }
        public virtual DbSet<Iso4217>                             Iso4217                             { get; set; }
        public virtual DbSet<Iso639>                              Iso639                              { get; set; }
        public virtual DbSet<License>                             Licenses                            { get; set; }
        public virtual DbSet<Log>                                 Log                                 { get; set; }
        public virtual DbSet<LogicalPartition>                    LogicalPartitions                   { get; set; }
        public virtual DbSet<Machine>                             Machines                            { get; set; }
        public virtual DbSet<MachineFamily>                       MachineFamilies                     { get; set; }
        public virtual DbSet<MachinePhoto>                        MachinePhotos                       { get; set; }
        public virtual DbSet<Magazine>                            Magazines                           { get; set; }
        public virtual DbSet<MagazineIssue>                       MagazineIssues                      { get; set; }
        public virtual DbSet<MagazinesByMachine>                  MagazinesByMachines                 { get; set; }
        public virtual DbSet<MagazinesByMachineFamily>            MagazinesByMachinesFamilies         { get; set; }
        public virtual DbSet<MagazineScan>                        MagazineScans                       { get; set; }
        public virtual DbSet<MarechaiDb>                          MarechaiDb                          { get; set; }
        public virtual DbSet<MasteringText>                       MasteringTexts                      { get; set; }
        public virtual DbSet<Media>                               Media                               { get; set; }
        public virtual DbSet<MediaDump>                           MediaDumps                          { get; set; }
        public virtual DbSet<MediaDumpFileImage>                  MediaDumpFileImages                 { get; set; }
        public virtual DbSet<MediaDumpImage>                      MediaDumpImages                     { get; set; }
        public virtual DbSet<MediaDumpSubchannelImage>            MediaDumpSubchannelImages           { get; set; }
        public virtual DbSet<MediaDumpTrackImage>                 MediaDumpTrackImages                { get; set; }
        public virtual DbSet<MediaFile>                           MediaFiles                          { get; set; }
        public virtual DbSet<MediaTagDump>                        MediaTagDumps                       { get; set; }
        public virtual DbSet<MemoryByMachine>                     MemoryByMachine                     { get; set; }
        public virtual DbSet<MemoryByOwnedMachine>                MemoryByOwnedMachine                { get; set; }
        public virtual DbSet<MoneyDonation>                       MoneyDonations                      { get; set; }
        public virtual DbSet<News>                                News                                { get; set; }
        public virtual DbSet<OwnedMachine>                        OwnedMachines                       { get; set; }
        public virtual DbSet<OwnedMachinePhoto>                   OwnedMachinePhotos                  { get; set; }
        public virtual DbSet<PeopleByBook>                        PeopleByBooks                       { get; set; }
        public virtual DbSet<PeopleByDocument>                    PeopleByDocuments                   { get; set; }
        public virtual DbSet<PeopleByMagazine>                    PeopleByMagazines                   { get; set; }
        public virtual DbSet<Person>                              People                              { get; set; }
        public virtual DbSet<Processor>                           Processors                          { get; set; }
        public virtual DbSet<ProcessorsByMachine>                 ProcessorsByMachine                 { get; set; }
        public virtual DbSet<ProcessorsByOwnedMachine>            ProcessorsByOwnedMachine            { get; set; }
        public virtual DbSet<Resolution>                          Resolutions                         { get; set; }
        public virtual DbSet<ResolutionsByGpu>                    ResolutionsByGpu                    { get; set; }
        public virtual DbSet<ResolutionsByScreen>                 ResolutionsByScreen                 { get; set; }
        public virtual DbSet<Screen>                              Screens                             { get; set; }
        public virtual DbSet<ScreensByMachine>                    ScreensByMachine                    { get; set; }
        public virtual DbSet<SoftwareFamily>                      SoftwareFamilies                    { get; set; }
        public virtual DbSet<SoftwareVariant>                     SoftwareVariants                    { get; set; }
        public virtual DbSet<SoftwareVariantByCompilationMedia>   SoftwareVariantByCompilationMedia   { get; set; }
        public virtual DbSet<SoftwareVersion>                     SoftwareVersions                    { get; set; }
        public virtual DbSet<SoundByMachine>                      SoundByMachine                      { get; set; }
        public virtual DbSet<SoundByOwnedMachine>                 SoundByOwnedMachine                 { get; set; }
        public virtual DbSet<SoundSynth>                          SoundSynths                         { get; set; }
        public virtual DbSet<StandaloneFile>                      StandaloneFiles                     { get; set; }
        public virtual DbSet<StorageByMachine>                    StorageByMachine                    { get; set; }
        public virtual DbSet<StorageByOwnedMachine>               StorageByOwnedMachine               { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(optionsBuilder.IsConfigured)
                return;

            IConfigurationBuilder builder       = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot    configuration = builder.Build();

            optionsBuilder.
                UseMySql(configuration.GetConnectionString("DefaultConnection"),
                         new MariaDbServerVersion(new Version(10, 5, 0)), b => b.UseMicrosoftJson()).
                UseLazyLoadingProxies();
        }

        public async Task<int> SaveChangesWithUserAsync(string userId)
        {
            ChangeTracker.DetectChanges();
            List<Audit> audits = new();

            foreach(EntityEntry entry in ChangeTracker.Entries())
            {
                if(entry.Entity is Audit               ||
                   entry.State == EntityState.Detached ||
                   entry.State == EntityState.Unchanged)
                    continue;

                var audit = new Audit();
                audit.UserId = userId;
                audit.Table  = entry.Metadata.GetTableName();

                Dictionary<string, object> keys    = new();
                Dictionary<string, object> olds    = new();
                Dictionary<string, object> news    = new();
                List<string>               columns = new();

                foreach(PropertyEntry property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    string columnName   = property.Metadata.GetColumnName();

                    if(property.Metadata.IsPrimaryKey())
                    {
                        keys[propertyName] = property.CurrentValue;

                        continue;
                    }

                    switch(entry.State)
                    {
                        case EntityState.Deleted:
                            audit.Type         = AuditType.Deleted;
                            olds[propertyName] = property.CurrentValue;

                            break;
                        case EntityState.Modified:
                            if(property.IsModified)
                            {
                                audit.Type         = AuditType.Updated;
                                news[propertyName] = property.CurrentValue;
                                olds[propertyName] = property.OriginalValue;
                                columns.Add(columnName);
                            }

                            break;

                        case EntityState.Added:
                            audit.Type         = AuditType.Created;
                            news[propertyName] = property.CurrentValue;

                            break;
                    }
                }

                if(keys.Count > 0)
                    audit.Keys = keys;

                if(olds.Count > 0)
                    audit.OldValues = olds;

                if(news.Count > 0)
                    audit.NewValues = news;

                if(columns.Count > 0)
                    audit.AffectedColumns = columns;

                audits.Add(audit);
            }

            await Audit.AddRangeAsync(audits);

            return await SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasIndex(e => e.Title);

                entity.HasIndex(e => e.NativeTitle);

                entity.HasIndex(e => e.Published);

                entity.HasIndex(e => e.CountryId);

                entity.HasIndex(e => e.Synopsis).IsFullText();

                entity.HasIndex(e => e.Isbn);

                entity.HasIndex(e => e.Pages);

                entity.HasIndex(e => e.Edition);

                entity.HasOne(d => d.Previous).WithOne(d => d.Next).HasForeignKey<Book>(d => d.PreviousId).
                       OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Source).WithMany(d => d.Derivates).HasForeignKey(d => d.SourceId).
                       OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Country).WithMany(p => p.Books).HasForeignKey(d => d.CountryId);
            });

            modelBuilder.Entity<BooksByMachine>(entity =>
            {
                entity.HasIndex(e => e.BookId);

                entity.HasIndex(e => e.MachineId);

                entity.HasOne(d => d.Book).WithMany(p => p.Machines).HasForeignKey(d => d.BookId);

                entity.HasOne(d => d.Machine).WithMany(p => p.Books).HasForeignKey(d => d.MachineId);
            });

            modelBuilder.Entity<BooksByMachineFamily>(entity =>
            {
                entity.HasIndex(e => e.BookId);

                entity.HasIndex(e => e.MachineFamilyId);

                entity.HasOne(d => d.Book).WithMany(p => p.MachineFamilies).HasForeignKey(d => d.BookId);

                entity.HasOne(d => d.MachineFamily).WithMany(p => p.Books).HasForeignKey(d => d.MachineFamilyId);
            });

            modelBuilder.Entity<BrowserTest>(entity =>
            {
                entity.ToTable("browser_tests");

                entity.HasIndex(e => e.Browser).HasDatabaseName("idx_browser_tests_browser");

                entity.HasIndex(e => e.Os).HasDatabaseName("idx_browser_tests_os");

                entity.HasIndex(e => e.Platform).HasDatabaseName("idx_browser_tests_platform");

                entity.HasIndex(e => e.UserAgent).HasDatabaseName("idx_browser_tests_user_agent");

                entity.HasIndex(e => e.Version).HasDatabaseName("idx_browser_tests_version");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Agif).HasColumnName("agif").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Browser).IsRequired().HasColumnName("browser").HasColumnType("varchar(64)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Colors).HasColumnName("colors").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Flash).HasColumnName("flash").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Frames).HasColumnName("frames").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Gif87).HasColumnName("gif87").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Gif89).HasColumnName("gif89").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Jpeg).HasColumnName("jpeg").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Js).HasColumnName("js").HasColumnType("tinyint(1)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Os).IsRequired().HasColumnName("os").HasColumnType("varchar(32)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Platform).IsRequired().HasColumnName("platform").HasColumnType("varchar(8)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Png).HasColumnName("png").HasColumnType("tinyint(1)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Pngt).HasColumnName("pngt").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Table).HasColumnName("table").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.UserAgent).IsRequired().HasColumnName("user_agent").
                       HasColumnType("varchar(128)").HasDefaultValueSql("''");

                entity.Property(e => e.Version).IsRequired().HasColumnName("version").HasColumnType("varchar(16)").
                       HasDefaultValueSql("''");
            });

            modelBuilder.Entity<MarechaiDb>(entity =>
            {
                entity.ToTable("marechai_db");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Updated).HasColumnName("updated").HasColumnType("datetime").
                       HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Version).HasColumnName("version").HasColumnType("int(11)");
            });

            modelBuilder.Entity<CompaniesByBook>(entity =>
            {
                entity.HasIndex(e => e.BookId);

                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => e.RoleId);

                entity.HasOne(d => d.Book).WithMany(p => p.Companies).HasForeignKey(d => d.BookId);

                entity.HasOne(d => d.Company).WithMany(p => p.Books).HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<CompaniesByDocument>(entity =>
            {
                entity.HasIndex(e => e.DocumentId);

                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => e.RoleId);

                entity.HasOne(d => d.Document).WithMany(p => p.Companies).HasForeignKey(d => d.DocumentId);

                entity.HasOne(d => d.Company).WithMany(p => p.Documents).HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<CompaniesByMagazine>(entity =>
            {
                entity.HasIndex(e => e.MagazineId);

                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => e.RoleId);

                entity.HasOne(d => d.Magazine).WithMany(p => p.Companies).HasForeignKey(d => d.MagazineId);

                entity.HasOne(d => d.Company).WithMany(p => p.Magazines).HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies");

                entity.HasIndex(e => e.Address).HasDatabaseName("idx_companies_address");

                entity.HasIndex(e => e.City).HasDatabaseName("idx_companies_city");

                entity.HasIndex(e => e.CountryId).HasDatabaseName("idx_companies_country");

                entity.HasIndex(e => e.Facebook).HasDatabaseName("idx_companies_facebook");

                entity.HasIndex(e => e.Founded).HasDatabaseName("idx_companies_founded");

                entity.HasIndex(e => e.Name).HasDatabaseName("idx_companies_name");

                entity.HasIndex(e => e.PostalCode).HasDatabaseName("idx_companies_postal_code");

                entity.HasIndex(e => e.Province).HasDatabaseName("idx_companies_province");

                entity.HasIndex(e => e.Sold).HasDatabaseName("idx_companies_sold");

                entity.HasIndex(e => e.SoldToId).HasDatabaseName("idx_companies_sold_to");

                entity.HasIndex(e => e.Status).HasDatabaseName("idx_companies_status");

                entity.HasIndex(e => e.Twitter).HasDatabaseName("idx_companies_twitter");

                entity.HasIndex(e => e.Website).HasDatabaseName("idx_companies_website");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Address).HasColumnName("address").HasColumnType("varchar(80)");

                entity.Property(e => e.City).HasColumnName("city").HasColumnType("varchar(80)");

                entity.Property(e => e.CountryId).HasColumnName("country").HasColumnType("smallint(3)");

                entity.Property(e => e.Facebook).HasColumnName("facebook").HasColumnType("varchar(45)");

                entity.Property(e => e.Founded).HasColumnName("founded").HasColumnType("datetime");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(128)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasColumnType("varchar(25)");

                entity.Property(e => e.Province).HasColumnName("province").HasColumnType("varchar(80)");

                entity.Property(e => e.Sold).HasColumnName("sold").HasColumnType("datetime");

                entity.Property(e => e.SoldToId).HasColumnName("sold_to").HasColumnType("int(11)");

                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("int(11)");

                entity.Property(e => e.Twitter).HasColumnName("twitter").HasColumnType("varchar(45)");

                entity.Property(e => e.Website).HasColumnName("website").HasColumnType("varchar(255)");

                entity.HasOne(d => d.Country).WithMany(p => p.Companies).HasForeignKey(d => d.CountryId).
                       HasConstraintName("fk_companies_country");

                entity.HasOne(d => d.SoldTo).WithMany(p => p.InverseSoldToNavigation).HasForeignKey(d => d.SoldToId).
                       HasConstraintName("fk_companies_sold_to");

                entity.HasOne(d => d.DocumentCompany).WithOne(p => p.Company).
                       HasForeignKey<DocumentCompany>(d => d.CompanyId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CompanyDescription>().HasIndex(e => e.Text).IsFullText();

            modelBuilder.Entity<CompanyLogo>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.Id,
                    e.CompanyId,
                    LogoGuid = e.Guid
                });

                entity.ToTable("company_logos");

                entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_company_id");

                entity.HasIndex(e => e.Id).HasDatabaseName("idx_id").IsUnique();

                entity.HasIndex(e => e.Guid).HasDatabaseName("idx_guid");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)").ValueGeneratedOnAdd();

                entity.Property(e => e.CompanyId).HasColumnName("company_id").HasColumnType("int(11)");

                entity.Property(e => e.Guid).HasColumnName("logo_guid").HasColumnType("char(36)");

                entity.Property(e => e.Year).HasColumnName("year").HasColumnType("int(4)");

                entity.HasOne(d => d.Company).WithMany(p => p.Logos).HasForeignKey(d => d.CompanyId).
                       OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_company_logos_company1");
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasIndex(e => e.Title);

                entity.HasIndex(e => e.NativeTitle);

                entity.HasIndex(e => e.Published);

                entity.HasIndex(e => e.CountryId);

                entity.HasIndex(e => e.Synopsis).IsFullText();

                entity.HasOne(d => d.Country).WithMany(p => p.Documents).HasForeignKey(d => d.CountryId);
            });

            modelBuilder.Entity<DocumentCompany>(entity =>
            {
                entity.HasIndex(e => e.Name);

                entity.HasIndex(e => e.CompanyId).IsUnique();
            });

            modelBuilder.Entity<DocumentPerson>(entity =>
            {
                entity.HasIndex(e => e.Name);

                entity.HasIndex(e => e.Surname);

                entity.HasIndex(e => e.PersonId).IsUnique();

                entity.HasIndex(e => e.Alias);

                entity.HasIndex(e => e.DisplayName);

                entity.HasOne(d => d.Person).WithOne(p => p.DocumentPerson).
                       HasForeignKey<Person>(d => d.DocumentPersonId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DocumentRole>(entity =>
            {
                entity.HasIndex(e => e.Name);

                entity.HasIndex(e => e.Enabled);

                entity.Property(p => p.Enabled).HasDefaultValue(true);
            });

            modelBuilder.Entity<DocumentsByMachine>(entity =>
            {
                entity.HasIndex(e => e.DocumentId);

                entity.HasIndex(e => e.MachineId);

                entity.HasOne(d => d.Document).WithMany(p => p.Machines).HasForeignKey(d => d.DocumentId);

                entity.HasOne(d => d.Machine).WithMany(p => p.Documents).HasForeignKey(d => d.MachineId);
            });

            modelBuilder.Entity<DocumentsByMachineFamily>(entity =>
            {
                entity.ToTable("DocumentsByMachineFamily");

                entity.HasIndex(e => e.DocumentId);

                entity.HasIndex(e => e.MachineFamilyId);

                entity.HasOne(d => d.Document).WithMany(p => p.MachineFamilies).HasForeignKey(d => d.DocumentId);

                entity.HasOne(d => d.MachineFamily).WithMany(p => p.Documents).HasForeignKey(d => d.MachineFamilyId);
            });

            modelBuilder.Entity<Forbidden>(entity =>
            {
                entity.ToTable("forbidden");

                entity.HasIndex(e => e.Browser).HasDatabaseName("idx_forbidden_browser");

                entity.HasIndex(e => e.Date).HasDatabaseName("idx_forbidden_date");

                entity.HasIndex(e => e.Ip).HasDatabaseName("idx_forbidden_ip");

                entity.HasIndex(e => e.Referer).HasDatabaseName("idx_forbidden_referer");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Browser).IsRequired().HasColumnName("browser").HasColumnType("char(128)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("char(20)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Ip).IsRequired().HasColumnName("ip").HasColumnType("char(16)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Referer).IsRequired().HasColumnName("referer").HasColumnType("char(255)").
                       HasDefaultValueSql("''");
            });

            modelBuilder.Entity<Gpu>(entity =>
            {
                entity.ToTable("gpus");

                entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_gpus_company");

                entity.HasIndex(e => e.DieSize).HasDatabaseName("idx_gpus_die_size");

                entity.HasIndex(e => e.Introduced).HasDatabaseName("idx_gpus_introduced");

                entity.HasIndex(e => e.ModelCode).HasDatabaseName("idx_gpus_model_code");

                entity.HasIndex(e => e.Name).HasDatabaseName("idx_gpus_name");

                entity.HasIndex(e => e.Package).HasDatabaseName("idx_gpus_package");

                entity.HasIndex(e => e.Process).HasDatabaseName("idx_gpus_process");

                entity.HasIndex(e => e.ProcessNm).HasDatabaseName("idx_gpus_process_nm");

                entity.HasIndex(e => e.Transistors).HasDatabaseName("idx_gpus_transistors");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)");

                entity.Property(e => e.DieSize).HasColumnName("die_size");

                entity.Property(e => e.Introduced).HasColumnName("introduced").HasColumnType("datetime");

                entity.Property(e => e.ModelCode).HasColumnName("model_code").HasColumnType("varchar(45)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("char(128)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Package).HasColumnName("package").HasColumnType("varchar(45)");

                entity.Property(e => e.Process).HasColumnName("process").HasColumnType("varchar(45)");

                entity.Property(e => e.ProcessNm).HasColumnName("process_nm");

                entity.Property(e => e.Transistors).HasColumnName("transistors").HasColumnType("bigint(20)");

                entity.HasOne(d => d.Company).WithMany(p => p.Gpus).HasForeignKey(d => d.CompanyId).
                       HasConstraintName("fk_gpus_company");
            });

            modelBuilder.Entity<GpusByMachine>(entity =>
            {
                entity.ToTable("gpus_by_machine");

                entity.HasIndex(e => e.GpuId).HasDatabaseName("idx_gpus_by_machine_gpus");

                entity.HasIndex(e => e.MachineId).HasDatabaseName("idx_gpus_by_machine_machine");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.GpuId).HasColumnName("gpu").HasColumnType("int(11)");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.HasOne(d => d.Gpu).WithMany(p => p.GpusByMachine).HasForeignKey(d => d.GpuId).
                       HasConstraintName("fk_gpus_by_machine_gpu");

                entity.HasOne(d => d.Machine).WithMany(p => p.Gpus).HasForeignKey(d => d.MachineId).
                       HasConstraintName("fk_gpus_by_machine_machine");
            });

            modelBuilder.Entity<GpusByOwnedMachine>(entity =>
            {
                entity.HasIndex(e => e.GpuId);

                entity.HasIndex(e => e.OwnedMachineId);

                entity.HasOne(d => d.OwnedMachine).WithMany(p => p.Gpus).HasForeignKey(d => d.OwnedMachineId);
            });

            modelBuilder.Entity<InstructionSetExtension>(entity =>
            {
                entity.ToTable("instruction_set_extensions");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Extension).IsRequired().HasColumnName("extension").HasColumnType("varchar(45)");
            });

            modelBuilder.Entity<InstructionSetExtensionsByProcessor>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.Id,
                    e.ProcessorId,
                    e.ExtensionId
                });

                entity.ToTable("instruction_set_extensions_by_processor");

                entity.HasIndex(e => e.ExtensionId).HasDatabaseName("idx_setextension_extension");

                entity.HasIndex(e => e.ProcessorId).HasDatabaseName("idx_setextension_processor");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)").ValueGeneratedOnAdd();

                entity.Property(e => e.ProcessorId).HasColumnName("processor_id").HasColumnType("int(11)");

                entity.Property(e => e.ExtensionId).HasColumnName("extension_id").HasColumnType("int(11)");

                entity.HasOne(d => d.Extension).WithMany(p => p.InstructionSetExtensionsByProcessor).
                       HasForeignKey(d => d.ExtensionId).OnDelete(DeleteBehavior.ClientSetNull).
                       HasConstraintName("fk_extension_extension_id");

                entity.HasOne(d => d.Processor).WithMany(p => p.InstructionSetExtensions).
                       HasForeignKey(d => d.ProcessorId).OnDelete(DeleteBehavior.ClientSetNull).
                       HasConstraintName("fk_extension_processor_id");
            });

            modelBuilder.Entity<InstructionSet>(entity =>
            {
                entity.ToTable("instruction_sets");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("instruction_set").HasColumnType("varchar(45)");
            });

            modelBuilder.Entity<Iso31661Numeric>(entity =>
            {
                entity.ToTable("iso3166_1_numeric");

                entity.HasIndex(e => e.Name).HasDatabaseName("idx_name");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("smallint(3)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(64)");
            });

            modelBuilder.Entity<Iso639>(entity =>
            {
                entity.ToTable("ISO_639-3");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Part2B);

                entity.HasIndex(e => e.Part2T);

                entity.HasIndex(e => e.Part1);

                entity.HasIndex(e => e.Scope);

                entity.HasIndex(e => e.Type);

                entity.HasIndex(e => e.ReferenceName);

                entity.HasIndex(e => e.Comment);

                entity.Property(e => e.ReferenceName).HasColumnName("Ref_Name");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("log");

                entity.HasIndex(e => e.Browser).HasDatabaseName("idx_log_browser");

                entity.HasIndex(e => e.Date).HasDatabaseName("idx_log_date");

                entity.HasIndex(e => e.Ip).HasDatabaseName("idx_log_ip");

                entity.HasIndex(e => e.Referer).HasDatabaseName("idx_log_referer");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Browser).IsRequired().HasColumnName("browser").HasColumnType("char(128)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("char(20)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Ip).IsRequired().HasColumnName("ip").HasColumnType("char(16)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Referer).IsRequired().HasColumnName("referer").HasColumnType("char(255)").
                       HasDefaultValueSql("''");
            });

            modelBuilder.Entity<MachineFamily>(entity =>
            {
                entity.ToTable("machine_families");

                entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_machine_families_company");

                entity.HasIndex(e => e.Name).HasDatabaseName("idx_machine_families_name");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(255)");

                entity.HasOne(d => d.Company).WithMany(p => p.MachineFamilies).HasForeignKey(d => d.CompanyId).
                       HasConstraintName("fk_machine_families_company");
            });

            modelBuilder.Entity<Machine>(entity =>
            {
                entity.ToTable("machines");

                entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_machines_company");

                entity.HasIndex(e => e.FamilyId).HasDatabaseName("idx_machines_family");

                entity.HasIndex(e => e.Introduced).HasDatabaseName("idx_machines_introduced");

                entity.HasIndex(e => e.Model).HasDatabaseName("idx_machines_model");

                entity.HasIndex(e => e.Name).HasDatabaseName("idx_machines_name");

                entity.HasIndex(e => e.Type).HasDatabaseName("idx_machines_type");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.FamilyId).HasColumnName("family").HasColumnType("int(11)");

                entity.Property(e => e.Introduced).HasColumnName("introduced").HasColumnType("datetime");

                entity.Property(e => e.Model).HasColumnName("model").HasColumnType("varchar(50)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(255)");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Company).WithMany(p => p.Machines).HasForeignKey(d => d.CompanyId).
                       OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_machines_company");

                entity.HasOne(d => d.Family).WithMany(p => p.Machines).HasForeignKey(d => d.FamilyId).
                       HasConstraintName("fk_machines_family");
            });

            modelBuilder.Entity<OwnedMachine>(entity =>
            {
                entity.HasIndex(e => e.AcquisitionDate);

                entity.HasIndex(e => e.LostDate);

                entity.HasIndex(e => e.Status);

                entity.HasIndex(e => e.LastStatusDate);

                entity.HasIndex(e => e.Trade);

                entity.HasIndex(e => e.Boxed);

                entity.HasIndex(e => e.Manuals);

                entity.HasIndex(e => e.SerialNumber);

                entity.HasIndex(e => e.SerialNumberVisible);

                entity.Property(e => e.SerialNumberVisible).HasDefaultValue(true);

                entity.HasOne(d => d.User).WithMany(p => p.OwnedMachines).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MachinePhoto>(entity =>
            {
                entity.HasIndex(e => e.Aperture);

                entity.HasIndex(e => e.Author);

                entity.HasIndex(e => e.CameraManufacturer);

                entity.HasIndex(e => e.CameraModel);

                entity.HasIndex(e => e.ColorSpace);

                entity.HasIndex(e => e.Comments);

                entity.HasIndex(e => e.Contrast);

                entity.HasIndex(e => e.CreationDate);

                entity.HasIndex(e => e.DigitalZoomRatio);

                entity.HasIndex(e => e.ExifVersion);

                entity.HasIndex(e => e.ExposureTime);

                entity.HasIndex(e => e.ExposureMethod);

                entity.HasIndex(e => e.ExposureProgram);

                entity.HasIndex(e => e.Flash);

                entity.HasIndex(e => e.Focal);

                entity.HasIndex(e => e.FocalLength);

                entity.HasIndex(e => e.FocalLengthEquivalent);

                entity.HasIndex(e => e.HorizontalResolution);

                entity.HasIndex(e => e.IsoRating);

                entity.HasIndex(e => e.Lens);

                entity.HasIndex(e => e.LightSource);

                entity.HasIndex(e => e.MeteringMode);

                entity.HasIndex(e => e.ResolutionUnit);

                entity.HasIndex(e => e.Orientation);

                entity.HasIndex(e => e.Saturation);

                entity.HasIndex(e => e.SceneCaptureType);

                entity.HasIndex(e => e.SensingMethod);

                entity.HasIndex(e => e.Sharpness);

                entity.HasIndex(e => e.SoftwareUsed);

                entity.HasIndex(e => e.SubjectDistanceRange);

                entity.HasIndex(e => e.UploadDate);

                entity.HasIndex(e => e.VerticalResolution);

                entity.HasIndex(e => e.WhiteBalance);

                entity.HasOne(d => d.Machine).WithMany(p => p.Photos).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User).WithMany(p => p.Photos).OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.License).WithMany(p => p.Photos).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OwnedMachinePhoto>(entity =>
            {
                entity.HasIndex(e => e.Aperture);

                entity.HasIndex(e => e.Author);

                entity.HasIndex(e => e.CameraManufacturer);

                entity.HasIndex(e => e.CameraModel);

                entity.HasIndex(e => e.ColorSpace);

                entity.HasIndex(e => e.Comments);

                entity.HasIndex(e => e.Contrast);

                entity.HasIndex(e => e.CreationDate);

                entity.HasIndex(e => e.DigitalZoomRatio);

                entity.HasIndex(e => e.ExifVersion);

                entity.HasIndex(e => e.ExposureTime);

                entity.HasIndex(e => e.ExposureMethod);

                entity.HasIndex(e => e.ExposureProgram);

                entity.HasIndex(e => e.Flash);

                entity.HasIndex(e => e.Focal);

                entity.HasIndex(e => e.FocalLength);

                entity.HasIndex(e => e.FocalLengthEquivalent);

                entity.HasIndex(e => e.HorizontalResolution);

                entity.HasIndex(e => e.IsoRating);

                entity.HasIndex(e => e.Lens);

                entity.HasIndex(e => e.LightSource);

                entity.HasIndex(e => e.MeteringMode);

                entity.HasIndex(e => e.ResolutionUnit);

                entity.HasIndex(e => e.Orientation);

                entity.HasIndex(e => e.Saturation);

                entity.HasIndex(e => e.SceneCaptureType);

                entity.HasIndex(e => e.SensingMethod);

                entity.HasIndex(e => e.Sharpness);

                entity.HasIndex(e => e.SoftwareUsed);

                entity.HasIndex(e => e.SubjectDistanceRange);

                entity.HasIndex(e => e.UploadDate);

                entity.HasIndex(e => e.VerticalResolution);

                entity.HasIndex(e => e.WhiteBalance);

                entity.HasOne(d => d.OwnedMachine).WithMany(p => p.Photos).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User).WithMany(p => p.OwnedMachinePhotos).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.License).WithMany(p => p.OwnedMachinePhotos).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Magazine>(entity =>
            {
                entity.HasIndex(e => e.Title);

                entity.HasIndex(e => e.NativeTitle);

                entity.HasIndex(e => e.Published);

                entity.HasIndex(e => e.CountryId);

                entity.HasIndex(e => e.Synopsis).IsFullText();

                entity.HasIndex(e => e.Issn);

                entity.HasIndex(e => e.FirstPublication);

                entity.HasOne(d => d.Country).WithMany(p => p.Magazines).HasForeignKey(d => d.CountryId);
            });

            modelBuilder.Entity<MagazineIssue>(entity =>
            {
                entity.HasIndex(e => e.Caption);

                entity.HasIndex(e => e.NativeCaption);

                entity.HasIndex(e => e.Published);

                entity.HasIndex(e => e.ProductCode);

                entity.HasIndex(e => e.Pages);

                entity.HasOne(d => d.Magazine).WithMany(p => p.Issues).HasForeignKey(d => d.MagazineId);
            });

            modelBuilder.Entity<MagazinesByMachine>(entity =>
            {
                entity.HasIndex(e => e.MagazineId);

                entity.HasIndex(e => e.MachineId);

                entity.HasOne(d => d.Magazine).WithMany(p => p.Machines).HasForeignKey(d => d.MagazineId);

                entity.HasOne(d => d.Machine).WithMany(p => p.Magazines).HasForeignKey(d => d.MachineId);
            });

            modelBuilder.Entity<MagazinesByMachineFamily>(entity =>
            {
                entity.HasIndex(e => e.MagazineId);

                entity.HasIndex(e => e.MachineFamilyId);

                entity.HasOne(d => d.Magazine).WithMany(p => p.MachineFamilies).HasForeignKey(d => d.MagazineId);

                entity.HasOne(d => d.MachineFamily).WithMany(p => p.Magazines).HasForeignKey(d => d.MachineFamilyId);
            });

            modelBuilder.Entity<MemoryByMachine>(entity =>
            {
                entity.ToTable("memory_by_machine");

                entity.HasIndex(e => e.MachineId).HasDatabaseName("idx_memory_by_machine_machine");

                entity.HasIndex(e => e.Size).HasDatabaseName("idx_memory_by_machine_size");

                entity.HasIndex(e => e.Speed).HasDatabaseName("idx_memory_by_machine_speed");

                entity.HasIndex(e => e.Type).HasDatabaseName("idx_memory_by_machine_type");

                entity.HasIndex(e => e.Usage).HasDatabaseName("idx_memory_by_machine_usage");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.Property(e => e.Size).HasColumnName("size").HasColumnType("bigint(20)");

                entity.Property(e => e.Speed).HasColumnName("speed");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Usage).HasColumnName("usage").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Machine).WithMany(p => p.Memory).HasForeignKey(d => d.MachineId).
                       HasConstraintName("fk_memory_by_machine_machine");
            });

            modelBuilder.Entity<MemoryByOwnedMachine>(entity =>
            {
                entity.HasIndex(e => e.OwnedMachineId);

                entity.HasIndex(e => e.Size);

                entity.HasIndex(e => e.Speed);

                entity.HasIndex(e => e.Type);

                entity.HasIndex(e => e.Usage);

                entity.HasOne(d => d.OwnedMachine).WithMany(p => p.Memory).HasForeignKey(d => d.OwnedMachineId);
            });

            modelBuilder.Entity<MoneyDonation>(entity =>
            {
                entity.ToTable("money_donations");

                entity.HasIndex(e => e.Donator).HasDatabaseName("idx_money_donations_donator");

                entity.HasIndex(e => e.Quantity).HasDatabaseName("idx_money_donations_quantity");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Donator).IsRequired().HasColumnName("donator").HasColumnType("char(128)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Quantity).HasColumnName("quantity").HasColumnType("decimal(11,2)").
                       HasDefaultValueSql("'0.00'");
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.ToTable("news");

                entity.HasIndex(e => e.AddedId).HasDatabaseName("idx_news_ip");

                entity.HasIndex(e => e.Date).HasDatabaseName("idx_news_date");

                entity.HasIndex(e => e.Type).HasDatabaseName("idx_news_type");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.AddedId).HasColumnName("added_id").HasColumnType("int(11)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("datetime");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)").HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<PeopleByBook>(entity =>
            {
                entity.HasIndex(e => e.PersonId);

                entity.HasIndex(e => e.BookId);

                entity.HasOne(d => d.Person).WithMany(p => p.Books).HasForeignKey(d => d.PersonId);

                entity.HasOne(d => d.Book).WithMany(p => p.People).HasForeignKey(d => d.BookId);
            });

            modelBuilder.Entity<PeopleByCompany>(entity =>
            {
                entity.HasIndex(e => e.PersonId);

                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => e.Position);

                entity.HasIndex(e => e.Start);

                entity.HasIndex(e => e.End);

                entity.HasOne(d => d.Person).WithMany(p => p.Companies).HasForeignKey(d => d.PersonId);

                entity.HasOne(d => d.Company).WithMany(p => p.People).HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<PeopleByDocument>(entity =>
            {
                entity.HasIndex(e => e.PersonId);

                entity.HasIndex(e => e.DocumentId);

                entity.HasOne(d => d.Person).WithMany(p => p.Documents).HasForeignKey(d => d.PersonId);

                entity.HasOne(d => d.Document).WithMany(p => p.People).HasForeignKey(d => d.DocumentId);
            });

            modelBuilder.Entity<PeopleByMagazine>(entity =>
            {
                entity.HasIndex(e => e.PersonId);

                entity.HasIndex(e => e.MagazineId);

                entity.HasOne(d => d.Person).WithMany(p => p.Magazines).HasForeignKey(d => d.PersonId);

                entity.HasOne(d => d.Magazine).WithMany(p => p.People).HasForeignKey(d => d.MagazineId);
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasIndex(e => e.Name);

                entity.HasIndex(e => e.Surname);

                entity.HasIndex(e => e.CountryOfBirthId);

                entity.HasIndex(e => e.BirthDate);

                entity.HasIndex(e => e.DeathDate);

                entity.HasIndex(e => e.Webpage);

                entity.HasIndex(e => e.Twitter);

                entity.HasIndex(e => e.Facebook);

                entity.HasIndex(e => e.Photo);

                entity.HasIndex(e => e.Alias);

                entity.HasIndex(e => e.DisplayName);

                entity.HasOne(d => d.CountryOfBirth).WithMany(p => p.People).HasForeignKey(d => d.CountryOfBirthId);

                entity.HasOne(d => d.DocumentPerson).WithOne(p => p.Person).
                       HasForeignKey<DocumentPerson>(d => d.PersonId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Processor>(entity =>
            {
                entity.ToTable("processors");

                entity.HasIndex(e => e.AddrBus).HasDatabaseName("idx_processors_addr_bus");

                entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_processors_company");

                entity.HasIndex(e => e.Cores).HasDatabaseName("idx_processors_cores");

                entity.HasIndex(e => e.DataBus).HasDatabaseName("idx_processors_data_bus");

                entity.HasIndex(e => e.DieSize).HasDatabaseName("idx_processors_die_size");

                entity.HasIndex(e => e.FprSize).HasDatabaseName("idx_processors_FPR_size");

                entity.HasIndex(e => e.Fprs).HasDatabaseName("idx_processors_FPRs");

                entity.HasIndex(e => e.GprSize).HasDatabaseName("idx_processors_GPR_size");

                entity.HasIndex(e => e.Gprs).HasDatabaseName("idx_processors_GPRs");

                entity.HasIndex(e => e.InstructionSetId).HasDatabaseName("idx_processors_instruction_set");

                entity.HasIndex(e => e.Introduced).HasDatabaseName("idx_processors_introduced");

                entity.HasIndex(e => e.L1Data).HasDatabaseName("idx_processors_L1_data");

                entity.HasIndex(e => e.L1Instruction).HasDatabaseName("idx_processors_L1_instruction");

                entity.HasIndex(e => e.L2).HasDatabaseName("idx_processors_L2");

                entity.HasIndex(e => e.L3).HasDatabaseName("idx_processors_L3");

                entity.HasIndex(e => e.ModelCode).HasDatabaseName("idx_processors_model_code");

                entity.HasIndex(e => e.Name).HasDatabaseName("idx_processors_name");

                entity.HasIndex(e => e.Package).HasDatabaseName("idx_processors_package");

                entity.HasIndex(e => e.Process).HasDatabaseName("idx_processors_process");

                entity.HasIndex(e => e.ProcessNm).HasDatabaseName("idx_processors_process_nm");

                entity.HasIndex(e => e.SimdRegisters).HasDatabaseName("idx_processors_SIMD_registers");

                entity.HasIndex(e => e.SimdSize).HasDatabaseName("idx_processors_SIMD_size");

                entity.HasIndex(e => e.Speed).HasDatabaseName("idx_processors_speed");

                entity.HasIndex(e => e.ThreadsPerCore).HasDatabaseName("idx_processors_threads_per_core");

                entity.HasIndex(e => e.Transistors).HasDatabaseName("idx_processors_transistors");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.AddrBus).HasColumnName("addr_bus").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)");

                entity.Property(e => e.Cores).HasColumnName("cores").HasColumnType("int(11)");

                entity.Property(e => e.DataBus).HasColumnName("data_bus").HasColumnType("int(11)");

                entity.Property(e => e.DieSize).HasColumnName("die_size");

                entity.Property(e => e.FprSize).HasColumnName("FPR_size").HasColumnType("int(11)");

                entity.Property(e => e.Fprs).HasColumnName("FPRs").HasColumnType("int(11)");

                entity.Property(e => e.GprSize).HasColumnName("GPR_size").HasColumnType("int(11)");

                entity.Property(e => e.Gprs).HasColumnName("GPRs").HasColumnType("int(11)");

                entity.Property(e => e.InstructionSetId).HasColumnName("instruction_set").HasColumnType("int(11)");

                entity.Property(e => e.Introduced).HasColumnName("introduced").HasColumnType("datetime");

                entity.Property(e => e.L1Data).HasColumnName("L1_data");

                entity.Property(e => e.L1Instruction).HasColumnName("L1_instruction");

                entity.Property(e => e.ModelCode).HasColumnName("model_code").HasColumnType("varchar(45)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("char(50)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.Package).HasColumnName("package").HasColumnType("varchar(45)");

                entity.Property(e => e.Process).HasColumnName("process").HasColumnType("varchar(45)");

                entity.Property(e => e.ProcessNm).HasColumnName("process_nm");

                entity.Property(e => e.SimdRegisters).HasColumnName("SIMD_registers").HasColumnType("int(11)");

                entity.Property(e => e.SimdSize).HasColumnName("SIMD_size").HasColumnType("int(11)");

                entity.Property(e => e.Speed).HasColumnName("speed");

                entity.Property(e => e.ThreadsPerCore).HasColumnName("threads_per_core").HasColumnType("int(11)");

                entity.Property(e => e.Transistors).HasColumnName("transistors").HasColumnType("bigint(20)");

                entity.HasOne(d => d.Company).WithMany(p => p.Processors).HasForeignKey(d => d.CompanyId).
                       HasConstraintName("fk_processors_company");

                entity.HasOne(d => d.InstructionSet).WithMany(p => p.Processors).HasForeignKey(d => d.InstructionSetId).
                       HasConstraintName("fk_processors_instruction_set");
            });

            modelBuilder.Entity<ProcessorsByMachine>(entity =>
            {
                entity.ToTable("processors_by_machine");

                entity.HasIndex(e => e.MachineId).HasDatabaseName("idx_processors_by_machine_machine");

                entity.HasIndex(e => e.ProcessorId).HasDatabaseName("idx_processors_by_machine_processor");

                entity.HasIndex(e => e.Speed).HasDatabaseName("idx_processors_by_machine_speed");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.Property(e => e.ProcessorId).HasColumnName("processor").HasColumnType("int(11)");

                entity.Property(e => e.Speed).HasColumnName("speed");

                entity.HasOne(d => d.Machine).WithMany(p => p.Processors).HasForeignKey(d => d.MachineId).
                       HasConstraintName("fk_processors_by_machine_machine");

                entity.HasOne(d => d.Processor).WithMany(p => p.ProcessorsByMachine).HasForeignKey(d => d.ProcessorId).
                       HasConstraintName("fk_processors_by_machine_processor");
            });

            modelBuilder.Entity<ProcessorsByOwnedMachine>(entity =>
            {
                entity.HasIndex(e => e.OwnedMachineId);

                entity.HasIndex(e => e.ProcessorId);

                entity.HasIndex(e => e.Speed);

                entity.HasOne(d => d.OwnedMachine).WithMany(p => p.Processors).HasForeignKey(d => d.OwnedMachineId);
            });

            modelBuilder.Entity<Resolution>(entity =>
            {
                entity.ToTable("resolutions");

                entity.HasIndex(e => e.Colors).HasDatabaseName("idx_resolutions_colors");

                entity.HasIndex(e => e.Height).HasDatabaseName("idx_resolutions_height");

                entity.HasIndex(e => e.Palette).HasDatabaseName("idx_resolutions_palette");

                entity.HasIndex(e => e.Width).HasDatabaseName("idx_resolutions_width");

                entity.HasIndex(e => new
                {
                    e.Width,
                    e.Height
                }).HasDatabaseName("idx_resolutions_resolution");

                entity.HasIndex(e => new
                {
                    e.Width,
                    e.Height,
                    e.Colors
                }).HasDatabaseName("idx_resolutions_resolution_with_color");

                entity.HasIndex(e => new
                {
                    e.Width,
                    e.Height,
                    e.Colors,
                    e.Palette
                }).HasDatabaseName("idx_resolutions_resolution_with_color_and_palette");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Chars).HasColumnName("chars").HasColumnType("tinyint(1)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Colors).HasColumnName("colors").HasColumnType("bigint(20)");

                entity.Property(e => e.Height).HasColumnName("height").HasColumnType("int(11)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.Palette).HasColumnName("palette").HasColumnType("bigint(20)");

                entity.Property(e => e.Width).HasColumnName("width").HasColumnType("int(11)").HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<ResolutionsByGpu>(entity =>
            {
                entity.ToTable("resolutions_by_gpu");

                entity.HasIndex(e => e.GpuId).HasDatabaseName("idx_resolutions_by_gpu_gpu");

                entity.HasIndex(e => e.ResolutionId).HasDatabaseName("idx_resolutions_by_gpu_resolution");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.GpuId).HasColumnName("gpu").HasColumnType("int(11)");

                entity.Property(e => e.ResolutionId).HasColumnName("resolution").HasColumnType("int(11)");

                entity.HasOne(d => d.Gpu).WithMany(p => p.ResolutionsByGpu).HasForeignKey(d => d.GpuId).
                       HasConstraintName("fk_resolutions_by_gpu_gpu");

                entity.HasOne(d => d.Resolution).WithMany(p => p.ResolutionsByGpu).HasForeignKey(d => d.ResolutionId).
                       HasConstraintName("fk_resolutions_by_gpu_resolution");
            });

            modelBuilder.Entity<ResolutionsByScreen>(entity =>
            {
                entity.HasIndex(e => e.ScreenId);

                entity.HasIndex(e => e.ResolutionId);
            });

            modelBuilder.Entity<ScreensByMachine>(entity =>
            {
                entity.HasIndex(e => e.ScreenId);

                entity.HasIndex(e => e.MachineId);
            });

            modelBuilder.Entity<Screen>(entity =>
            {
                entity.HasIndex(e => e.Width);

                entity.HasIndex(e => e.Height);

                entity.HasIndex(e => e.Diagonal);

                entity.HasIndex(e => e.EffectiveColors);

                entity.HasIndex(e => e.Type);

                entity.HasOne(d => d.NativeResolution).WithMany(p => p.Screens);
            });

            modelBuilder.Entity<SoundByMachine>(entity =>
            {
                entity.ToTable("sound_by_machine");

                entity.HasIndex(e => e.MachineId).HasDatabaseName("idx_sound_by_machine_machine");

                entity.HasIndex(e => e.SoundSynthId).HasDatabaseName("idx_sound_by_machine_sound_synth");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.Property(e => e.SoundSynthId).HasColumnName("sound_synth").HasColumnType("int(11)");

                entity.HasOne(d => d.Machine).WithMany(p => p.Sound).HasForeignKey(d => d.MachineId).
                       HasConstraintName("fk_sound_by_machine_machine");

                entity.HasOne(d => d.SoundSynth).WithMany(p => p.SoundByMachine).HasForeignKey(d => d.SoundSynthId).
                       HasConstraintName("fk_sound_by_machine_sound_synth");
            });

            modelBuilder.Entity<SoundByOwnedMachine>(entity =>
            {
                entity.HasIndex(e => e.OwnedMachineId);

                entity.HasIndex(e => e.SoundSynthId);

                entity.HasOne(d => d.OwnedMachine).WithMany(p => p.Sound).HasForeignKey(d => d.OwnedMachineId);
            });

            modelBuilder.Entity<SoundSynth>(entity =>
            {
                entity.ToTable("sound_synths");

                entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_sound_synths_company");

                entity.HasIndex(e => e.Depth).HasDatabaseName("idx_sound_synths_depth");

                entity.HasIndex(e => e.Frequency).HasDatabaseName("idx_sound_synths_frequency");

                entity.HasIndex(e => e.Introduced).HasDatabaseName("idx_sound_synths_introduced");

                entity.HasIndex(e => e.ModelCode).HasDatabaseName("idx_sound_synths_model_code");

                entity.HasIndex(e => e.Name).HasDatabaseName("idx_sound_synths_name");

                entity.HasIndex(e => e.SquareWave).HasDatabaseName("idx_sound_synths_square_wave");

                entity.HasIndex(e => e.Type).HasDatabaseName("idx_sound_synths_type");

                entity.HasIndex(e => e.Voices).HasDatabaseName("idx_sound_synths_voices");

                entity.HasIndex(e => e.WhiteNoise).HasDatabaseName("idx_sound_synths_white_noise");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)");

                entity.Property(e => e.Depth).HasColumnName("depth").HasColumnType("int(11)");

                entity.Property(e => e.Frequency).HasColumnName("frequency");

                entity.Property(e => e.Introduced).HasColumnName("introduced").HasColumnType("datetime");

                entity.Property(e => e.ModelCode).HasColumnName("model_code").HasColumnType("varchar(45)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("char(50)").
                       HasDefaultValueSql("''");

                entity.Property(e => e.SquareWave).HasColumnName("square_wave").HasColumnType("int(11)");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)");

                entity.Property(e => e.Voices).HasColumnName("voices").HasColumnType("int(11)");

                entity.Property(e => e.WhiteNoise).HasColumnName("white_noise").HasColumnType("int(11)");

                entity.HasOne(d => d.Company).WithMany(p => p.SoundSynths).HasForeignKey(d => d.CompanyId).
                       HasConstraintName("fk_sound_synths_company");
            });

            modelBuilder.Entity<StorageByMachine>(entity =>
            {
                entity.ToTable("storage_by_machine");

                entity.HasIndex(e => e.Capacity).HasDatabaseName("idx_storage_capacity");

                entity.HasIndex(e => e.Interface).HasDatabaseName("idx_storage_interface");

                entity.HasIndex(e => e.MachineId).HasDatabaseName("idx_storage_machine");

                entity.HasIndex(e => e.Type).HasDatabaseName("idx_storage_type");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.Capacity).HasColumnName("capacity").HasColumnType("bigint(20)");

                entity.Property(e => e.Interface).HasColumnName("interface").HasColumnType("int(11)").
                       HasDefaultValueSql("'0'");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Machine).WithMany(p => p.Storage).HasForeignKey(d => d.MachineId).
                       HasConstraintName("fk_storage_by_machine_machine");
            });

            modelBuilder.Entity<StorageByOwnedMachine>(entity =>
            {
                entity.HasIndex(e => e.Capacity);

                entity.HasIndex(e => e.Interface);

                entity.HasIndex(e => e.OwnedMachineId);

                entity.HasIndex(e => e.Type);

                entity.HasOne(d => d.OwnedMachine).WithMany(p => p.Storage).HasForeignKey(d => d.OwnedMachineId);
            });

            modelBuilder.Entity<License>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.SPDX);
                entity.HasIndex(e => e.FsfApproved);
                entity.HasIndex(e => e.OsiApproved);
            });

            modelBuilder.Entity<Audit>(entity =>
            {
                entity.HasIndex(d => d.Table);
                entity.HasIndex(d => d.Type);
            });

            modelBuilder.Entity<Iso4217>(entity =>
            {
                entity.HasIndex(d => d.Numeric);
                entity.HasIndex(d => d.Withdrawn);
            });

            modelBuilder.Entity<CurrencyInflation>(entity =>
            {
                entity.HasIndex(d => d.Year);
            });

            modelBuilder.Entity<CurrencyPegging>(entity =>
            {
                entity.HasIndex(d => d.Start);
                entity.HasIndex(d => d.End);
            });

            modelBuilder.Entity<DumpHardware>(entity =>
            {
                entity.HasIndex(e => e.Manufacturer);
                entity.HasIndex(e => e.Model);
                entity.HasIndex(e => e.Revision);
                entity.HasIndex(e => e.Firmware);
                entity.HasIndex(e => e.Serial);
                entity.HasIndex(e => e.SoftwareName);
                entity.HasIndex(e => e.SoftwareVersion);
                entity.HasIndex(e => e.SoftwareOperatingSystem);

                entity.HasOne(e => e.Dump).WithMany(e => e.DumpHardware).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DbFile>(entity =>
            {
                entity.Property(e => e.Md5).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha1).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha256).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha3).HasConversion(hexToBytesConverter);

                entity.HasIndex(e => e.Size);
                entity.HasIndex(e => e.Md5);
                entity.HasIndex(e => e.Sha1);
                entity.HasIndex(e => e.Sha256);
                entity.HasIndex(e => e.Sha3);
                entity.HasIndex(e => e.Spamsum);
                entity.HasIndex(e => e.Mime);
                entity.HasIndex(e => e.Magic);
                entity.HasIndex(e => e.AccoustId);
                entity.HasIndex(e => e.Infected);
                entity.HasIndex(e => e.Malware);
                entity.HasIndex(e => e.Hack);
                entity.HasIndex(e => e.HackGroup);
            });

            modelBuilder.Entity<FileDataStream>(entity =>
            {
                entity.HasIndex(d => d.Name);
                entity.HasIndex(d => d.Size);
            });

            modelBuilder.Entity<Filesystem>(entity =>
            {
                entity.HasIndex(d => d.Type);
                entity.HasIndex(d => d.CreationDate);
                entity.HasIndex(d => d.ModificationDate);
                entity.HasIndex(d => d.BackupDate);
                entity.HasIndex(d => d.Serial);
                entity.HasIndex(d => d.Label);
                entity.HasIndex(d => d.SystemIdentifier);
                entity.HasIndex(d => d.VolumeSetIdentifier);
                entity.HasIndex(d => d.PublisherIdentifier);
                entity.HasIndex(d => d.DataPreparerIdentifier);
                entity.HasIndex(d => d.ApplicationIdentifier);
            });

            modelBuilder.Entity<LogicalPartition>(entity =>
            {
                entity.HasIndex(e => e.Description);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Scheme);
                entity.HasIndex(e => e.FirstSector);
                entity.HasIndex(e => e.LastSector);
            });

            modelBuilder.Entity<FilesystemsByLogicalPartition>(entity =>
            {
                entity.HasOne(d => d.Partition).WithMany(p => p.Filesystems).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Filesystem).WithMany(p => p.Partitions).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.CopyProtection);
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.SerialNumber);
                entity.HasIndex(e => e.Barcode);
                entity.HasIndex(e => e.CatalogueNumber);
                entity.HasIndex(e => e.Manufacturer);
                entity.HasIndex(e => e.Model);
                entity.HasIndex(e => e.Revision);
                entity.HasIndex(e => e.Firmware);

                entity.HasOne(d => d.MagazineIssue).WithMany(p => p.Coverdiscs).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<LogicalPartitionsByMedia>(entity =>
            {
                entity.HasOne(d => d.Partition).WithMany(p => p.Media).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Media).WithMany(p => p.LogicalPartitions).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MediaDump>(entity =>
            {
                entity.HasOne(d => d.Media).WithMany(p => p.MediaDumps).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Format);
            });

            modelBuilder.Entity<MediaDumpFileImage>(entity =>
            {
                entity.Property(e => e.Md5).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha1).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha256).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha3).HasConversion(hexToBytesConverter);

                entity.HasOne(d => d.MediaDump).WithMany(p => p.Files).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Size);
                entity.HasIndex(e => e.Md5);
                entity.HasIndex(e => e.Sha1);
                entity.HasIndex(e => e.Sha256);
                entity.HasIndex(e => e.Sha3);
                entity.HasIndex(e => e.Spamsum);
            });

            modelBuilder.Entity<FilesystemsByMediaDumpFile>(entity =>
            {
                entity.HasOne(d => d.MediaDumpFileImage).WithMany(p => p.Filesystems).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Filesystem).WithMany(p => p.MediaDumpFileImages).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MediaDumpImage>(entity =>
            {
                entity.Property(e => e.Md5).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha1).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha256).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha3).HasConversion(hexToBytesConverter);

                entity.HasOne(d => d.MediaDump).WithOne(p => p.Image).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Size);
                entity.HasIndex(e => e.Md5);
                entity.HasIndex(e => e.Sha1);
                entity.HasIndex(e => e.Sha256);
                entity.HasIndex(e => e.Sha3);
                entity.HasIndex(e => e.Spamsum);
                entity.HasIndex(e => e.AccoustId);
            });

            modelBuilder.Entity<MediaDumpSubchannelImage>(entity =>
            {
                entity.Property(e => e.Md5).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha1).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha256).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha3).HasConversion(hexToBytesConverter);

                entity.HasOne(d => d.MediaDump).WithOne(p => p.Subchannel).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Track).WithOne(p => p.Subchannel).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Size);
                entity.HasIndex(e => e.Md5);
                entity.HasIndex(e => e.Sha1);
                entity.HasIndex(e => e.Sha256);
                entity.HasIndex(e => e.Sha3);
                entity.HasIndex(e => e.Spamsum);
            });

            modelBuilder.Entity<MediaDumpTrackImage>(entity =>
            {
                entity.Property(e => e.Md5).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha1).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha256).HasConversion(hexToBytesConverter);
                entity.Property(e => e.Sha3).HasConversion(hexToBytesConverter);

                entity.HasOne(d => d.MediaDump).WithMany(p => p.Tracks).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Size);
                entity.HasIndex(e => e.Format);
                entity.HasIndex(e => e.Md5);
                entity.HasIndex(e => e.Sha1);
                entity.HasIndex(e => e.Sha256);
                entity.HasIndex(e => e.Sha3);
                entity.HasIndex(e => e.Spamsum);
            });

            modelBuilder.Entity<MediaFile>(entity =>
            {
                entity.HasIndex(e => e.Path);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.IsDirectory);
                entity.HasIndex(e => e.CreationDate);
                entity.HasIndex(e => e.AccessDate);
                entity.HasIndex(e => e.StatusChangeDate);
                entity.HasIndex(e => e.BackupDate);
                entity.HasIndex(e => e.LastWriteDate);
                entity.HasIndex(e => e.GroupId);
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<FileDataStreamsByMediaFile>(entity =>
            {
                entity.HasOne(d => d.MediaFile).WithMany(p => p.DataStreams).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FilesByFilesystem>(entity =>
            {
                entity.HasOne(d => d.Filesystem).WithMany(p => p.Files).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Dump>(entity =>
            {
                entity.HasIndex(e => e.Dumper);
                entity.HasIndex(e => e.DumpingGroup);
                entity.HasIndex(e => e.DumpDate);

                entity.HasOne(e => e.User).WithMany(e => e.Dumps).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Media).WithMany(e => e.Dumps).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.MediaDump).WithMany(e => e.Dumps).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SoftwareFamily>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Introduced);

                entity.HasOne(e => e.Parent).WithMany(e => e.Children).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CompaniesBySoftwareFamily>(entity =>
            {
                entity.ToTable("CompaniesBySoftwareFamily");

                entity.HasOne(d => d.Company).WithMany(p => p.SoftwareFamilies).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareFamily).WithMany(p => p.Companies).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PeopleBySoftwareFamily>(entity =>
            {
                entity.HasOne(d => d.Person).WithMany(p => p.SoftwareFamilies).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareFamily).WithMany(p => p.People).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SoftwareVersion>(entity =>
            {
                entity.ToTable("SoftwareVersion");

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Introduced);
                entity.HasIndex(e => e.Codename);
                entity.HasIndex(e => e.Version);

                entity.HasOne(e => e.Family).WithMany(e => e.Versions).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Previous).WithOne(e => e.Next).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CompaniesBySoftwareVersion>(entity =>
            {
                entity.ToTable("CompaniesBySoftwareVersion");

                entity.HasOne(d => d.Company).WithMany(p => p.SoftwareVersions).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVersion).WithMany(p => p.Companies).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PeopleBySoftwareVersion>(entity =>
            {
                entity.HasOne(d => d.Person).WithMany(p => p.SoftwareVersions).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVersion).WithMany(p => p.People).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SoftwareVariant>(entity =>
            {
                entity.HasIndex(e => e.Introduced);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CatalogueNumber);
                entity.HasIndex(e => e.DistributionMode);
                entity.HasIndex(e => e.MinimumMemory);
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.ProductCode);
                entity.HasIndex(e => e.RecommendedMemory);
                entity.HasIndex(e => e.RequiredStorage);
                entity.HasIndex(e => e.SerialNumber);
                entity.HasIndex(e => e.Version);

                entity.HasOne(e => e.Parent).WithMany(e => e.Derivates).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.SoftwareVersion).WithMany(e => e.Variants).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CompaniesBySoftwareVariant>(entity =>
            {
                entity.ToTable("CompaniesBySoftwareVariant");

                entity.HasOne(d => d.Company).WithMany(p => p.SoftwareVariants).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.Companies).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GpusBySoftwareVariant>(entity =>
            {
                entity.HasIndex(e => e.Minimum);
                entity.HasIndex(e => e.Recommended);

                entity.HasOne(d => d.Gpu).WithMany(p => p.Software).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.Gpus).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InstructionSetsBySoftwareVariant>(entity =>
            {
                entity.HasOne(d => d.InstructionSet).WithMany(p => p.Software).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.Architectures).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LanguagesBySoftwareVariant>(entity =>
            {
                entity.HasOne(d => d.Language).WithMany(p => p.Software).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.Languages).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MachineFamiliesBySoftwareVariant>(entity =>
            {
                entity.HasOne(d => d.MachineFamily).WithMany(p => p.Software).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.MachineFamilies).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MachinesBySoftwareVariant>(entity =>
            {
                entity.HasOne(d => d.Machine).WithMany(p => p.Software).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.Machines).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MediaBySoftwareVariant>(entity =>
            {
                entity.HasOne(d => d.Media).WithMany(p => p.Software).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.Media).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PeopleBySoftwareVariant>(entity =>
            {
                entity.HasOne(d => d.Person).WithMany(p => p.SoftwareVariants).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.People).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProcessorsBySoftwareVariant>(entity =>
            {
                entity.HasIndex(e => e.Minimum);
                entity.HasIndex(e => e.Recommended);
                entity.HasIndex(e => e.Speed);

                entity.HasOne(d => d.Processor).WithMany(p => p.Software).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.Processors).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RequiredOperatingSystemsBySofwareVariant>(entity =>
            {
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.RequiredOperatingSystems).
                       OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RequiredSoftwareBySoftwareVariant>(entity =>
            {
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.RequiredSoftware).
                       OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SoundBySoftwareVariant>(entity =>
            {
                entity.HasOne(d => d.SoundSynth).WithMany(p => p.Software).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.SupportedSound).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FileDataStreamsByStandaloneFile>(entity =>
            {
                entity.HasOne(d => d.StandaloneFile).WithMany(p => p.DataStreams).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StandaloneFile>(entity =>
            {
                entity.HasIndex(e => e.Path);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.IsDirectory);
                entity.HasIndex(e => e.CreationDate);
                entity.HasIndex(e => e.AccessDate);
                entity.HasIndex(e => e.StatusChangeDate);
                entity.HasIndex(e => e.BackupDate);
                entity.HasIndex(e => e.LastWriteDate);
                entity.HasIndex(e => e.GroupId);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(d => d.SoftwareVariant).WithMany(p => p.Files).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MasteringText>(entity =>
            {
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Text);
                entity.HasOne(d => d.Media).WithMany(p => p.MasteringTexts).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MediaTagDump>(entity =>
            {
                entity.HasIndex(e => e.Type);

                entity.HasOne(e => e.MediaDump).WithMany(e => e.Tags).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SoftwareVariantByCompilationMedia>(entity =>
            {
                entity.HasIndex(e => e.Path);
            });

            modelBuilder.Entity<BookScan>(entity =>
            {
                entity.HasIndex(e => e.Author);

                entity.HasIndex(e => e.ColorSpace);

                entity.HasIndex(e => e.Comments);

                entity.HasIndex(e => e.CreationDate);

                entity.HasIndex(e => e.ExifVersion);

                entity.HasIndex(e => e.HorizontalResolution);

                entity.HasIndex(e => e.ResolutionUnit);

                entity.HasIndex(e => e.ScannerManufacturer);

                entity.HasIndex(e => e.ScannerModel);

                entity.HasIndex(e => e.SoftwareUsed);

                entity.HasIndex(e => e.UploadDate);

                entity.HasIndex(e => e.VerticalResolution);

                entity.HasIndex(e => e.Type);

                entity.HasIndex(e => e.Page);

                entity.HasOne(d => d.Book).WithMany(p => p.Scans).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User).WithMany(p => p.BookScans).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DocumentScan>(entity =>
            {
                entity.HasIndex(e => e.Author);

                entity.HasIndex(e => e.ColorSpace);

                entity.HasIndex(e => e.Comments);

                entity.HasIndex(e => e.CreationDate);

                entity.HasIndex(e => e.ExifVersion);

                entity.HasIndex(e => e.HorizontalResolution);

                entity.HasIndex(e => e.ResolutionUnit);

                entity.HasIndex(e => e.ScannerManufacturer);

                entity.HasIndex(e => e.ScannerModel);

                entity.HasIndex(e => e.SoftwareUsed);

                entity.HasIndex(e => e.UploadDate);

                entity.HasIndex(e => e.VerticalResolution);

                entity.HasIndex(e => e.Type);

                entity.HasIndex(e => e.Page);

                entity.HasOne(d => d.Document).WithMany(p => p.Scans).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User).WithMany(p => p.DocumentScans).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MagazineScan>(entity =>
            {
                entity.HasIndex(e => e.Author);

                entity.HasIndex(e => e.ColorSpace);

                entity.HasIndex(e => e.Comments);

                entity.HasIndex(e => e.CreationDate);

                entity.HasIndex(e => e.ExifVersion);

                entity.HasIndex(e => e.HorizontalResolution);

                entity.HasIndex(e => e.ResolutionUnit);

                entity.HasIndex(e => e.ScannerManufacturer);

                entity.HasIndex(e => e.ScannerModel);

                entity.HasIndex(e => e.SoftwareUsed);

                entity.HasIndex(e => e.UploadDate);

                entity.HasIndex(e => e.VerticalResolution);

                entity.HasIndex(e => e.Type);

                entity.HasIndex(e => e.Page);

                entity.HasOne(d => d.Magazine).WithMany(p => p.Scans).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User).WithMany(p => p.MagazineScans).OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}