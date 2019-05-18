/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : cicmContext.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Describes the database for Entity Framework.
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cicm.Database.Models
{
    public class cicmContext : IdentityDbContext
    {
        public cicmContext() { }

        public cicmContext(DbContextOptions<cicmContext> options) : base(options) { }

        public virtual DbSet<Admin>                               Admins                              { get; set; }
        public virtual DbSet<BrowserTest>                         BrowserTests                        { get; set; }
        public virtual DbSet<CicmDb>                              CicmDb                              { get; set; }
        public virtual DbSet<Company>                             Companies                           { get; set; }
        public virtual DbSet<CompanyDescription>                  CompanyDescriptions                 { get; set; }
        public virtual DbSet<CompanyLogo>                         CompanyLogos                        { get; set; }
        public virtual DbSet<Forbidden>                           Forbidden                           { get; set; }
        public virtual DbSet<Gpu>                                 Gpus                                { get; set; }
        public virtual DbSet<GpusByMachine>                       GpusByMachine                       { get; set; }
        public virtual DbSet<InstructionSetExtension>             InstructionSetExtensions            { get; set; }
        public virtual DbSet<InstructionSetExtensionsByProcessor> InstructionSetExtensionsByProcessor { get; set; }
        public virtual DbSet<InstructionSet>                      InstructionSets                     { get; set; }
        public virtual DbSet<Iso31661Numeric>                     Iso31661Numeric                     { get; set; }
        public virtual DbSet<Log>                                 Log                                 { get; set; }
        public virtual DbSet<MachineFamily>                       MachineFamilies                     { get; set; }
        public virtual DbSet<Machine>                             Machines                            { get; set; }
        public virtual DbSet<MemoryByMachine>                     MemoryByMachine                     { get; set; }
        public virtual DbSet<MoneyDonation>                       MoneyDonations                      { get; set; }
        public virtual DbSet<News>                                News                                { get; set; }
        public virtual DbSet<OwnedComputer>                       OwnedComputers                      { get; set; }
        public virtual DbSet<OwnedConsole>                        OwnedConsoles                       { get; set; }
        public virtual DbSet<Processor>                           Processors                          { get; set; }
        public virtual DbSet<ProcessorsByMachine>                 ProcessorsByMachine                 { get; set; }
        public virtual DbSet<Resolution>                          Resolutions                         { get; set; }
        public virtual DbSet<ResolutionsByGpu>                    ResolutionsByGpu                    { get; set; }
        public virtual DbSet<SoundByMachine>                      SoundByMachine                      { get; set; }
        public virtual DbSet<SoundSynth>                          SoundSynths                         { get; set; }
        public virtual DbSet<StorageByMachine>                    StorageByMachine                    { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(optionsBuilder.IsConfigured) return;

            #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseMySql("server=localhost;port=3306;user=cicm;password=cicmpass;database=cicm");
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admins");

                entity.HasIndex(e => e.User).HasName("idx_admins_user");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Password).IsRequired().HasColumnName("password").HasColumnType("char(50)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.User).IsRequired().HasColumnName("user").HasColumnType("char(50)")
                      .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<BrowserTest>(entity =>
            {
                entity.ToTable("browser_tests");

                entity.HasIndex(e => e.Browser).HasName("idx_browser_tests_browser");

                entity.HasIndex(e => e.Os).HasName("idx_browser_tests_os");

                entity.HasIndex(e => e.Platform).HasName("idx_browser_tests_platform");

                entity.HasIndex(e => e.UserAgent).HasName("idx_browser_tests_user_agent");

                entity.HasIndex(e => e.Version).HasName("idx_browser_tests_version");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Agif).HasColumnName("agif").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Browser).IsRequired().HasColumnName("browser").HasColumnType("varchar(64)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Colors).HasColumnName("colors").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Flash).HasColumnName("flash").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Frames).HasColumnName("frames").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Gif87).HasColumnName("gif87").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Gif89).HasColumnName("gif89").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Jpeg).HasColumnName("jpeg").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Js).HasColumnName("js").HasColumnType("tinyint(1)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Os).IsRequired().HasColumnName("os").HasColumnType("varchar(32)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Platform).IsRequired().HasColumnName("platform").HasColumnType("varchar(8)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Png).HasColumnName("png").HasColumnType("tinyint(1)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Pngt).HasColumnName("pngt").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Table).HasColumnName("table").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserAgent).IsRequired().HasColumnName("user_agent").HasColumnType("varchar(128)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Version).IsRequired().HasColumnName("version").HasColumnType("varchar(16)")
                      .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<CicmDb>(entity =>
            {
                entity.ToTable("cicm_db");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Updated).HasColumnName("updated").HasColumnType("datetime")
                      .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Version).HasColumnName("version").HasColumnType("int(11)");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies");

                entity.HasIndex(e => e.Address).HasName("idx_companies_address");

                entity.HasIndex(e => e.City).HasName("idx_companies_city");

                entity.HasIndex(e => e.CountryId).HasName("idx_companies_country");

                entity.HasIndex(e => e.Facebook).HasName("idx_companies_facebook");

                entity.HasIndex(e => e.Founded).HasName("idx_companies_founded");

                entity.HasIndex(e => e.Name).HasName("idx_companies_name");

                entity.HasIndex(e => e.PostalCode).HasName("idx_companies_postal_code");

                entity.HasIndex(e => e.Province).HasName("idx_companies_province");

                entity.HasIndex(e => e.Sold).HasName("idx_companies_sold");

                entity.HasIndex(e => e.SoldToId).HasName("idx_companies_sold_to");

                entity.HasIndex(e => e.Status).HasName("idx_companies_status");

                entity.HasIndex(e => e.Twitter).HasName("idx_companies_twitter");

                entity.HasIndex(e => e.Website).HasName("idx_companies_website");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Address).HasColumnName("address").HasColumnType("varchar(80)");

                entity.Property(e => e.City).HasColumnName("city").HasColumnType("varchar(80)");

                entity.Property(e => e.CountryId).HasColumnName("country").HasColumnType("smallint(3)");

                entity.Property(e => e.Facebook).HasColumnName("facebook").HasColumnType("varchar(45)");

                entity.Property(e => e.Founded).HasColumnName("founded").HasColumnType("datetime");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(128)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasColumnType("varchar(25)");

                entity.Property(e => e.Province).HasColumnName("province").HasColumnType("varchar(80)");

                entity.Property(e => e.Sold).HasColumnName("sold").HasColumnType("datetime");

                entity.Property(e => e.SoldToId).HasColumnName("sold_to").HasColumnType("int(11)");

                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("int(11)");

                entity.Property(e => e.Twitter).HasColumnName("twitter").HasColumnType("varchar(45)");

                entity.Property(e => e.Website).HasColumnName("website").HasColumnType("varchar(255)");

                entity.HasOne(d => d.Country).WithMany(p => p.Companies).HasForeignKey(d => d.CountryId)
                      .HasConstraintName("fk_companies_country");

                entity.HasOne(d => d.SoldTo).WithMany(p => p.InverseSoldToNavigation).HasForeignKey(d => d.SoldToId)
                      .HasConstraintName("fk_companies_sold_to");
            });

            modelBuilder.Entity<CompanyDescription>().HasIndex(e => e.Text).ForMySqlIsFullText();

            modelBuilder.Entity<CompanyLogo>(entity =>
            {
                entity.HasKey(e => new {e.Id, e.CompanyId, LogoGuid = e.Guid});

                entity.ToTable("company_logos");

                entity.HasIndex(e => e.CompanyId).HasName("idx_company_id");

                entity.HasIndex(e => e.Id).HasName("idx_id").IsUnique();

                entity.HasIndex(e => e.Guid).HasName("idx_guid");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)").ValueGeneratedOnAdd();

                entity.Property(e => e.CompanyId).HasColumnName("company_id").HasColumnType("int(11)");

                entity.Property(e => e.Guid).HasColumnName("logo_guid").HasColumnType("char(36)");

                entity.Property(e => e.Year).HasColumnName("year").HasColumnType("int(4)");

                entity.HasOne(d => d.Company).WithMany(p => p.Logos).HasForeignKey(d => d.CompanyId)
                      .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_company_logos_company1");
            });

            modelBuilder.Entity<Forbidden>(entity =>
            {
                entity.ToTable("forbidden");

                entity.HasIndex(e => e.Browser).HasName("idx_forbidden_browser");

                entity.HasIndex(e => e.Date).HasName("idx_forbidden_date");

                entity.HasIndex(e => e.Ip).HasName("idx_forbidden_ip");

                entity.HasIndex(e => e.Referer).HasName("idx_forbidden_referer");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Browser).IsRequired().HasColumnName("browser").HasColumnType("char(128)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("char(20)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Ip).IsRequired().HasColumnName("ip").HasColumnType("char(16)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Referer).IsRequired().HasColumnName("referer").HasColumnType("char(255)")
                      .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<Gpu>(entity =>
            {
                entity.ToTable("gpus");

                entity.HasIndex(e => e.CompanyId).HasName("idx_gpus_company");

                entity.HasIndex(e => e.DieSize).HasName("idx_gpus_die_size");

                entity.HasIndex(e => e.Introduced).HasName("idx_gpus_introduced");

                entity.HasIndex(e => e.ModelCode).HasName("idx_gpus_model_code");

                entity.HasIndex(e => e.Name).HasName("idx_gpus_name");

                entity.HasIndex(e => e.Package).HasName("idx_gpus_package");

                entity.HasIndex(e => e.Process).HasName("idx_gpus_process");

                entity.HasIndex(e => e.ProcessNm).HasName("idx_gpus_process_nm");

                entity.HasIndex(e => e.Transistors).HasName("idx_gpus_transistors");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)");

                entity.Property(e => e.DieSize).HasColumnName("die_size");

                entity.Property(e => e.Introduced).HasColumnName("introduced").HasColumnType("datetime");

                entity.Property(e => e.ModelCode).HasColumnName("model_code").HasColumnType("varchar(45)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("char(128)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Package).HasColumnName("package").HasColumnType("varchar(45)");

                entity.Property(e => e.Process).HasColumnName("process").HasColumnType("varchar(45)");

                entity.Property(e => e.ProcessNm).HasColumnName("process_nm");

                entity.Property(e => e.Transistors).HasColumnName("transistors").HasColumnType("bigint(20)");

                entity.HasOne(d => d.Company).WithMany(p => p.Gpus).HasForeignKey(d => d.CompanyId)
                      .HasConstraintName("fk_gpus_company");
            });

            modelBuilder.Entity<GpusByMachine>(entity =>
            {
                entity.ToTable("gpus_by_machine");

                entity.HasIndex(e => e.GpuId).HasName("idx_gpus_by_machine_gpus");

                entity.HasIndex(e => e.MachineId).HasName("idx_gpus_by_machine_machine");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.GpuId).HasColumnName("gpu").HasColumnType("int(11)");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.HasOne(d => d.Gpu).WithMany(p => p.GpusByMachine).HasForeignKey(d => d.GpuId)
                      .HasConstraintName("fk_gpus_by_machine_gpu");

                entity.HasOne(d => d.Machine).WithMany(p => p.Gpus).HasForeignKey(d => d.MachineId)
                      .HasConstraintName("fk_gpus_by_machine_machine");
            });

            modelBuilder.Entity<InstructionSetExtension>(entity =>
            {
                entity.ToTable("instruction_set_extensions");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Extension).IsRequired().HasColumnName("extension").HasColumnType("varchar(45)");
            });

            modelBuilder.Entity<InstructionSetExtensionsByProcessor>(entity =>
            {
                entity.HasKey(e => new {e.Id, e.ProcessorId, e.ExtensionId});

                entity.ToTable("instruction_set_extensions_by_processor");

                entity.HasIndex(e => e.ExtensionId).HasName("idx_setextension_extension");

                entity.HasIndex(e => e.ProcessorId).HasName("idx_setextension_processor");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)").ValueGeneratedOnAdd();

                entity.Property(e => e.ProcessorId).HasColumnName("processor_id").HasColumnType("int(11)");

                entity.Property(e => e.ExtensionId).HasColumnName("extension_id").HasColumnType("int(11)");

                entity.HasOne(d => d.Extension).WithMany(p => p.InstructionSetExtensionsByProcessor)
                      .HasForeignKey(d => d.ExtensionId).OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("fk_extension_extension_id");

                entity.HasOne(d => d.Processor).WithMany(p => p.InstructionSetExtensions)
                      .HasForeignKey(d => d.ProcessorId).OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("fk_extension_processor_id");
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

                entity.HasIndex(e => e.Name).HasName("idx_name");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("smallint(3)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(64)");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("log");

                entity.HasIndex(e => e.Browser).HasName("idx_log_browser");

                entity.HasIndex(e => e.Date).HasName("idx_log_date");

                entity.HasIndex(e => e.Ip).HasName("idx_log_ip");

                entity.HasIndex(e => e.Referer).HasName("idx_log_referer");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Browser).IsRequired().HasColumnName("browser").HasColumnType("char(128)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("char(20)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Ip).IsRequired().HasColumnName("ip").HasColumnType("char(16)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Referer).IsRequired().HasColumnName("referer").HasColumnType("char(255)")
                      .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<MachineFamily>(entity =>
            {
                entity.ToTable("machine_families");

                entity.HasIndex(e => e.CompanyId).HasName("idx_machine_families_company");

                entity.HasIndex(e => e.Name).HasName("idx_machine_families_name");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(255)");

                entity.HasOne(d => d.Company).WithMany(p => p.MachineFamilies).HasForeignKey(d => d.CompanyId)
                      .HasConstraintName("fk_machine_families_company");
            });

            modelBuilder.Entity<Machine>(entity =>
            {
                entity.ToTable("machines");

                entity.HasIndex(e => e.CompanyId).HasName("idx_machines_company");

                entity.HasIndex(e => e.FamilyId).HasName("idx_machines_family");

                entity.HasIndex(e => e.Introduced).HasName("idx_machines_introduced");

                entity.HasIndex(e => e.Model).HasName("idx_machines_model");

                entity.HasIndex(e => e.Name).HasName("idx_machines_name");

                entity.HasIndex(e => e.Type).HasName("idx_machines_type");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.FamilyId).HasColumnName("family").HasColumnType("int(11)");

                entity.Property(e => e.Introduced).HasColumnName("introduced").HasColumnType("datetime");

                entity.Property(e => e.Model).HasColumnName("model").HasColumnType("varchar(50)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(255)");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Company).WithMany(p => p.Machines).HasForeignKey(d => d.CompanyId)
                      .OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_machines_company");

                entity.HasOne(d => d.Family).WithMany(p => p.Machines).HasForeignKey(d => d.FamilyId)
                      .HasConstraintName("fk_machines_family");
            });

            modelBuilder.Entity<MemoryByMachine>(entity =>
            {
                entity.ToTable("memory_by_machine");

                entity.HasIndex(e => e.MachineId).HasName("idx_memory_by_machine_machine");

                entity.HasIndex(e => e.Size).HasName("idx_memory_by_machine_size");

                entity.HasIndex(e => e.Speed).HasName("idx_memory_by_machine_speed");

                entity.HasIndex(e => e.Type).HasName("idx_memory_by_machine_type");

                entity.HasIndex(e => e.Usage).HasName("idx_memory_by_machine_usage");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.Property(e => e.Size).HasColumnName("size").HasColumnType("bigint(20)");

                entity.Property(e => e.Speed).HasColumnName("speed");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Usage).HasColumnName("usage").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Machine).WithMany(p => p.Memory).HasForeignKey(d => d.MachineId)
                      .HasConstraintName("fk_memory_by_machine_machine");
            });

            modelBuilder.Entity<MoneyDonation>(entity =>
            {
                entity.ToTable("money_donations");

                entity.HasIndex(e => e.Donator).HasName("idx_money_donations_donator");

                entity.HasIndex(e => e.Quantity).HasName("idx_money_donations_quantity");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Donator).IsRequired().HasColumnName("donator").HasColumnType("char(128)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Quantity).HasColumnName("quantity").HasColumnType("decimal(11,2)")
                      .HasDefaultValueSql("'0.00'");
            });

            modelBuilder.Entity<News>(entity =>
            {
                entity.ToTable("news");

                entity.HasIndex(e => e.AddedId).HasName("idx_news_ip");

                entity.HasIndex(e => e.Date).HasName("idx_news_date");

                entity.HasIndex(e => e.Type).HasName("idx_news_type");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.AddedId).HasColumnName("added_id").HasColumnType("int(11)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("datetime");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)").HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<OwnedComputer>(entity =>
            {
                entity.ToTable("owned_computers");

                entity.HasIndex(e => e.Boxed).HasName("idx_owned_computers_boxed");

                entity.HasIndex(e => e.Cap1).HasName("idx_owned_computers_cap1");

                entity.HasIndex(e => e.Cap2).HasName("idx_owned_computers_cap2");

                entity.HasIndex(e => e.Cpu1).HasName("idx_owned_computers_cpu1");

                entity.HasIndex(e => e.Cpu2).HasName("idx_owned_computers_cpu2");

                entity.HasIndex(e => e.Date).HasName("idx_owned_computers_date");

                entity.HasIndex(e => e.DbId).HasName("idx_owned_computers_db_id");

                entity.HasIndex(e => e.Disk1).HasName("idx_owned_computers_disk1");

                entity.HasIndex(e => e.Disk2).HasName("idx_owned_computers_disk2");

                entity.HasIndex(e => e.Manuals).HasName("idx_owned_computers_manuals");

                entity.HasIndex(e => e.Mhz1).HasName("idx_owned_computers_mhz1");

                entity.HasIndex(e => e.Mhz2).HasName("idx_owned_computers_mhz2");

                entity.HasIndex(e => e.Ram).HasName("idx_owned_computers_ram");

                entity.HasIndex(e => e.Rigid).HasName("idx_owned_computers_rigid");

                entity.HasIndex(e => e.Status).HasName("idx_owned_computers_status");

                entity.HasIndex(e => e.Trade).HasName("idx_owned_computers_trade");

                entity.HasIndex(e => e.Vram).HasName("idx_owned_computers_vram");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Boxed).HasColumnName("boxed").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Cap1).HasColumnName("cap1").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Cap2).HasColumnName("cap2").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Cpu1).HasColumnName("cpu1").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Cpu2).HasColumnName("cpu2").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("varchar(20)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.DbId).HasColumnName("db_id").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Disk1).HasColumnName("disk1").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Disk2).HasColumnName("disk2").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Manuals).HasColumnName("manuals").HasColumnType("int(11)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Mhz1).HasColumnName("mhz1").HasColumnType("decimal(10,0)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Mhz2).HasColumnName("mhz2").HasColumnType("decimal(10,0)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Ram).HasColumnName("ram").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Rigid).IsRequired().HasColumnName("rigid").HasColumnType("varchar(64)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("int(11)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Trade).HasColumnName("trade").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Vram).HasColumnName("vram").HasColumnType("int(11)").HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<OwnedConsole>(entity =>
            {
                entity.ToTable("owned_consoles");

                entity.HasIndex(e => e.Boxed).HasName("idx_owned_consoles_boxed");

                entity.HasIndex(e => e.Date).HasName("idx_owned_consoles_date");

                entity.HasIndex(e => e.DbId).HasName("idx_owned_consoles_db_id");

                entity.HasIndex(e => e.Manuals).HasName("idx_owned_consoles_manuals");

                entity.HasIndex(e => e.Status).HasName("idx_owned_consoles_status");

                entity.HasIndex(e => e.Trade).HasName("idx_owned_consoles_trade");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Boxed).HasColumnName("boxed").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("char(20)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.DbId).HasColumnName("db_id").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.Property(e => e.Manuals).HasColumnName("manuals").HasColumnType("int(11)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Status).HasColumnName("status").HasColumnType("int(11)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Trade).HasColumnName("trade").HasColumnType("int(11)").HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<Processor>(entity =>
            {
                entity.ToTable("processors");

                entity.HasIndex(e => e.AddrBus).HasName("idx_processors_addr_bus");

                entity.HasIndex(e => e.CompanyId).HasName("idx_processors_company");

                entity.HasIndex(e => e.Cores).HasName("idx_processors_cores");

                entity.HasIndex(e => e.DataBus).HasName("idx_processors_data_bus");

                entity.HasIndex(e => e.DieSize).HasName("idx_processors_die_size");

                entity.HasIndex(e => e.FprSize).HasName("idx_processors_FPR_size");

                entity.HasIndex(e => e.Fprs).HasName("idx_processors_FPRs");

                entity.HasIndex(e => e.GprSize).HasName("idx_processors_GPR_size");

                entity.HasIndex(e => e.Gprs).HasName("idx_processors_GPRs");

                entity.HasIndex(e => e.InstructionSetId).HasName("idx_processors_instruction_set");

                entity.HasIndex(e => e.Introduced).HasName("idx_processors_introduced");

                entity.HasIndex(e => e.L1Data).HasName("idx_processors_L1_data");

                entity.HasIndex(e => e.L1Instruction).HasName("idx_processors_L1_instruction");

                entity.HasIndex(e => e.L2).HasName("idx_processors_L2");

                entity.HasIndex(e => e.L3).HasName("idx_processors_L3");

                entity.HasIndex(e => e.ModelCode).HasName("idx_processors_model_code");

                entity.HasIndex(e => e.Name).HasName("idx_processors_name");

                entity.HasIndex(e => e.Package).HasName("idx_processors_package");

                entity.HasIndex(e => e.Process).HasName("idx_processors_process");

                entity.HasIndex(e => e.ProcessNm).HasName("idx_processors_process_nm");

                entity.HasIndex(e => e.SimdRegisters).HasName("idx_processors_SIMD_registers");

                entity.HasIndex(e => e.SimdSize).HasName("idx_processors_SIMD_size");

                entity.HasIndex(e => e.Speed).HasName("idx_processors_speed");

                entity.HasIndex(e => e.ThreadsPerCore).HasName("idx_processors_threads_per_core");

                entity.HasIndex(e => e.Transistors).HasName("idx_processors_transistors");

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

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("char(50)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.Package).HasColumnName("package").HasColumnType("varchar(45)");

                entity.Property(e => e.Process).HasColumnName("process").HasColumnType("varchar(45)");

                entity.Property(e => e.ProcessNm).HasColumnName("process_nm");

                entity.Property(e => e.SimdRegisters).HasColumnName("SIMD_registers").HasColumnType("int(11)");

                entity.Property(e => e.SimdSize).HasColumnName("SIMD_size").HasColumnType("int(11)");

                entity.Property(e => e.Speed).HasColumnName("speed");

                entity.Property(e => e.ThreadsPerCore).HasColumnName("threads_per_core").HasColumnType("int(11)");

                entity.Property(e => e.Transistors).HasColumnName("transistors").HasColumnType("bigint(20)");

                entity.HasOne(d => d.Company).WithMany(p => p.Processors).HasForeignKey(d => d.CompanyId)
                      .HasConstraintName("fk_processors_company");

                entity.HasOne(d => d.InstructionSet).WithMany(p => p.Processors).HasForeignKey(d => d.InstructionSetId)
                      .HasConstraintName("fk_processors_instruction_set");
            });

            modelBuilder.Entity<ProcessorsByMachine>(entity =>
            {
                entity.ToTable("processors_by_machine");

                entity.HasIndex(e => e.MachineId).HasName("idx_processors_by_machine_machine");

                entity.HasIndex(e => e.ProcessorId).HasName("idx_processors_by_machine_processor");

                entity.HasIndex(e => e.Speed).HasName("idx_processors_by_machine_speed");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.Property(e => e.ProcessorId).HasColumnName("processor").HasColumnType("int(11)");

                entity.Property(e => e.Speed).HasColumnName("speed");

                entity.HasOne(d => d.Machine).WithMany(p => p.Processors).HasForeignKey(d => d.MachineId)
                      .HasConstraintName("fk_processors_by_machine_machine");

                entity.HasOne(d => d.Processor).WithMany(p => p.ProcessorsByMachine).HasForeignKey(d => d.ProcessorId)
                      .HasConstraintName("fk_processors_by_machine_processor");
            });

            modelBuilder.Entity<Resolution>(entity =>
            {
                entity.ToTable("resolutions");

                entity.HasIndex(e => e.Colors).HasName("idx_resolutions_colors");

                entity.HasIndex(e => e.Height).HasName("idx_resolutions_height");

                entity.HasIndex(e => e.Palette).HasName("idx_resolutions_palette");

                entity.HasIndex(e => e.Width).HasName("idx_resolutions_width");

                entity.HasIndex(e => new {e.Width, e.Height}).HasName("idx_resolutions_resolution");

                entity.HasIndex(e => new {e.Width, e.Height, e.Colors})
                      .HasName("idx_resolutions_resolution_with_color");

                entity.HasIndex(e => new {e.Width, e.Height, e.Colors, e.Palette})
                      .HasName("idx_resolutions_resolution_with_color_and_palette");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.Chars).HasColumnName("chars").HasColumnType("tinyint(1)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Colors).HasColumnName("colors").HasColumnType("bigint(20)");

                entity.Property(e => e.Height).HasColumnName("height").HasColumnType("int(11)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.Palette).HasColumnName("palette").HasColumnType("bigint(20)");

                entity.Property(e => e.Width).HasColumnName("width").HasColumnType("int(11)").HasDefaultValueSql("'0'");
            });

            modelBuilder.Entity<ResolutionsByGpu>(entity =>
            {
                entity.ToTable("resolutions_by_gpu");

                entity.HasIndex(e => e.GpuId).HasName("idx_resolutions_by_gpu_gpu");

                entity.HasIndex(e => e.ResolutionId).HasName("idx_resolutions_by_gpu_resolution");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.GpuId).HasColumnName("gpu").HasColumnType("int(11)");

                entity.Property(e => e.ResolutionId).HasColumnName("resolution").HasColumnType("int(11)");

                entity.HasOne(d => d.Gpu).WithMany(p => p.ResolutionsByGpu).HasForeignKey(d => d.GpuId)
                      .HasConstraintName("fk_resolutions_by_gpu_gpu");

                entity.HasOne(d => d.Resolution).WithMany(p => p.ResolutionsByGpu).HasForeignKey(d => d.ResolutionId)
                      .HasConstraintName("fk_resolutions_by_gpu_resolution");
            });

            modelBuilder.Entity<SoundByMachine>(entity =>
            {
                entity.ToTable("sound_by_machine");

                entity.HasIndex(e => e.MachineId).HasName("idx_sound_by_machine_machine");

                entity.HasIndex(e => e.SoundSynthId).HasName("idx_sound_by_machine_sound_synth");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.Property(e => e.SoundSynthId).HasColumnName("sound_synth").HasColumnType("int(11)");

                entity.HasOne(d => d.Machine).WithMany(p => p.Sound).HasForeignKey(d => d.MachineId)
                      .HasConstraintName("fk_sound_by_machine_machine");

                entity.HasOne(d => d.SoundSynth).WithMany(p => p.SoundByMachine).HasForeignKey(d => d.SoundSynthId)
                      .HasConstraintName("fk_sound_by_machine_sound_synth");
            });

            modelBuilder.Entity<SoundSynth>(entity =>
            {
                entity.ToTable("sound_synths");

                entity.HasIndex(e => e.CompanyId).HasName("idx_sound_synths_company");

                entity.HasIndex(e => e.Depth).HasName("idx_sound_synths_depth");

                entity.HasIndex(e => e.Frequency).HasName("idx_sound_synths_frequency");

                entity.HasIndex(e => e.Introduced).HasName("idx_sound_synths_introduced");

                entity.HasIndex(e => e.ModelCode).HasName("idx_sound_synths_model_code");

                entity.HasIndex(e => e.Name).HasName("idx_sound_synths_name");

                entity.HasIndex(e => e.SquareWave).HasName("idx_sound_synths_square_wave");

                entity.HasIndex(e => e.Type).HasName("idx_sound_synths_type");

                entity.HasIndex(e => e.Voices).HasName("idx_sound_synths_voices");

                entity.HasIndex(e => e.WhiteNoise).HasName("idx_sound_synths_white_noise");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

                entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)");

                entity.Property(e => e.Depth).HasColumnName("depth").HasColumnType("int(11)");

                entity.Property(e => e.Frequency).HasColumnName("frequency");

                entity.Property(e => e.Introduced).HasColumnName("introduced").HasColumnType("datetime");

                entity.Property(e => e.ModelCode).HasColumnName("model_code").HasColumnType("varchar(45)");

                entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("char(50)")
                      .HasDefaultValueSql("''");

                entity.Property(e => e.SquareWave).HasColumnName("square_wave").HasColumnType("int(11)");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)");

                entity.Property(e => e.Voices).HasColumnName("voices").HasColumnType("int(11)");

                entity.Property(e => e.WhiteNoise).HasColumnName("white_noise").HasColumnType("int(11)");

                entity.HasOne(d => d.Company).WithMany(p => p.SoundSynths).HasForeignKey(d => d.CompanyId)
                      .HasConstraintName("fk_sound_synths_company");
            });

            modelBuilder.Entity<StorageByMachine>(entity =>
            {
                entity.ToTable("storage_by_machine");

                entity.HasIndex(e => e.Capacity).HasName("idx_storage_capacity");

                entity.HasIndex(e => e.Interface).HasName("idx_storage_interface");

                entity.HasIndex(e => e.MachineId).HasName("idx_storage_machine");

                entity.HasIndex(e => e.Type).HasName("idx_storage_type");

                entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

                entity.Property(e => e.Capacity).HasColumnName("capacity").HasColumnType("bigint(20)");

                entity.Property(e => e.Interface).HasColumnName("interface").HasColumnType("int(11)")
                      .HasDefaultValueSql("'0'");

                entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

                entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)").HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Machine).WithMany(p => p.Storage).HasForeignKey(d => d.MachineId)
                      .HasConstraintName("fk_storage_by_machine_machine");
            });
        }
    }
}