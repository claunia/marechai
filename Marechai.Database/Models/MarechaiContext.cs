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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Schemas;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace Marechai.Database.Models;

public class MarechaiContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    readonly ValueConverter<string, byte[]> _hexToBytesConverter =
        new(v => HexStringToBytesConverter.StringToHex(v), v => HexStringToBytesConverter.HexToString(v));

    /// <summary>
    ///     Maps to the MariaDB NaturalSortKey() function for natural sorting of strings containing numeric segments.
    ///     This method is for EF Core LINQ-to-SQL translation only and must not be called directly.
    /// </summary>
    [DbFunction("NaturalSortKey", Schema = null)]
    public static string NaturalSortKey(string value) => throw new NotSupportedException("This method is for EF Core LINQ-to-SQL translation only.");

    /// <summary>
    ///     Maps to the MariaDB NormalizeForDuplicate() function used by the /admin/software/duplicates page to group
    ///     pseudoduplicate Software rows. Strips every parenthesised and bracketed group, collapses whitespace, and
    ///     lowercases. This method is for EF Core LINQ-to-SQL translation only and must not be called directly.
    /// </summary>
    [DbFunction("NormalizeForDuplicate", Schema = null)]
    public static string NormalizeForDuplicate(string value) => throw new NotSupportedException("This method is for EF Core LINQ-to-SQL translation only.");

    /// <summary>
    ///     Maps to the MariaDB NormalizeForMatch() function used by the /admin/software/orphan-addons page to
    ///     compare DLC names by their leading word(s). Strips parenthesised/bracketed groups, then replaces every
    ///     non-alphanumeric non-whitespace character (including colons and dashes) with a space, collapses
    ///     whitespace, trims and lowercases. Designed for prefix LIKE comparisons against tokenized name prefixes.
    ///     This method is for EF Core LINQ-to-SQL translation only and must not be called directly.
    /// </summary>
    [DbFunction("NormalizeForMatch", Schema = null)]
    public static string NormalizeForMatch(string value) => throw new NotSupportedException("This method is for EF Core LINQ-to-SQL translation only.");

    public MarechaiContext() {}

    public MarechaiContext(DbContextOptions<MarechaiContext> options) : base(options) {}

    public virtual DbSet<Audit>                               Audit                               { get; set; }
    public virtual DbSet<Book>                                Books                               { get; set; }
    public virtual DbSet<BooksByMachine>                      BooksByMachines                     { get; set; }
    public virtual DbSet<BooksByMachineFamily>                BooksByMachineFamilies              { get; set; }
    public virtual DbSet<BookSynopsis>                        BookSynopses                        { get; set; }
    public virtual DbSet<BrowserTest>                         BrowserTests                        { get; set; }
    public virtual DbSet<CompaniesByBook>                     CompaniesByBooks                    { get; set; }
    public virtual DbSet<CompaniesByDocument>                 CompaniesByDocuments                { get; set; }
    public virtual DbSet<CompaniesByMagazine>                 CompaniesByMagazines                { get; set; }
    public virtual DbSet<Company>                             Companies                           { get; set; }
    public virtual DbSet<CompanyDescription>                  CompanyDescriptions                 { get; set; }
    public virtual DbSet<CompanyLogo>                         CompanyLogos                        { get; set; }
    public virtual DbSet<CurrencyInflation>                   CurrenciesInflation                 { get; set; }
    public virtual DbSet<CurrencyPegging>                     CurrenciesPegging                   { get; set; }
    public virtual DbSet<DbFile>                              Files                               { get; set; }
    public virtual DbSet<Document>                            Documents                           { get; set; }
    public virtual DbSet<DocumentRole>                        DocumentRoles                       { get; set; }
    public virtual DbSet<DocumentsByMachine>                  DocumentsByMachines                 { get; set; }
    public virtual DbSet<DocumentsByMachineFamily>            DocumentsByMachineFamilies          { get; set; }
    public virtual DbSet<DocumentSynopsis>                    DocumentSynopses                    { get; set; }
    public virtual DbSet<Dump>                                Dumps                               { get; set; }
    public virtual DbSet<DumpHardware>                        DumpHardwares                       { get; set; }
    public virtual DbSet<FileDataStream>                      FileDataStreams                     { get; set; }
    public virtual DbSet<Filesystem>                          Filesystems                         { get; set; }
    public virtual DbSet<FilesystemsByLogicalPartition>       FilesystemsByLogicalPartition       { get; set; }
    public virtual DbSet<Forbidden>                           Forbidden                           { get; set; }
    public virtual DbSet<Gpu>                                 Gpus                                { get; set; }
    public virtual DbSet<GpusByMachine>                       GpusByMachine                       { get; set; }
    public virtual DbSet<IgdbAlternativeName>                 IgdbAlternativeNames                { get; set; }
    public virtual DbSet<IgdbCompany>                         IgdbCompanies                       { get; set; }
    public virtual DbSet<IgdbGame>                            IgdbGames                           { get; set; }
    public virtual DbSet<IgdbGameType>                        IgdbGameTypes                       { get; set; }
    public virtual DbSet<IgdbInvolvedCompany>                 IgdbInvolvedCompanies               { get; set; }
    public virtual DbSet<IgdbPlatform>                        IgdbPlatforms                       { get; set; }
    public virtual DbSet<InstructionSet>                      InstructionSets                     { get; set; }
    public virtual DbSet<InstructionSetExtension>             InstructionSetExtensions            { get; set; }
    public virtual DbSet<InstructionSetExtensionsByProcessor> InstructionSetExtensionsByProcessor { get; set; }
    public virtual DbSet<InvitationCode>                      InvitationCodes                     { get; set; }
    public virtual DbSet<Iso31661Numeric>                     Iso31661Numeric                     { get; set; }
    public virtual DbSet<Iso4217>                             Iso4217                             { get; set; }
    public virtual DbSet<Iso639>                              Iso639                              { get; set; }
    public virtual DbSet<License>                             Licenses                            { get; set; }
    public virtual DbSet<Log>                                 Log                                 { get; set; }
    public virtual DbSet<LogicalPartition>                    LogicalPartitions                   { get; set; }
    public virtual DbSet<Machine>                             Machines                            { get; set; }
    public virtual DbSet<MachineDescription>                  MachineDescriptions                 { get; set; }
    public virtual DbSet<MachineFamily>                       MachineFamilies                     { get; set; }
    public virtual DbSet<MachinePromoArt>                     MachinePromoArt                     { get; set; }
    public virtual DbSet<SoundSynthDescription>               SoundSynthDescriptions              { get; set; }
    public virtual DbSet<ProcessorDescription>                ProcessorDescriptions               { get; set; }
    public virtual DbSet<GpuDescription>                      GpuDescriptions                     { get; set; }
    public virtual DbSet<GpuPhoto>                            GpuPhotos                           { get; set; }
    public virtual DbSet<MachinePhoto>                        MachinePhotos                       { get; set; }
    public virtual DbSet<ProcessorPhoto>                      ProcessorPhotos                     { get; set; }
    public virtual DbSet<SoundSynthPhoto>                     SoundSynthPhotos                    { get; set; }
    public virtual DbSet<Magazine>                            Magazines                           { get; set; }
    public virtual DbSet<MagazineIssue>                       MagazineIssues                      { get; set; }
    public virtual DbSet<MagazinesByMachine>                  MagazinesByMachines                 { get; set; }
    public virtual DbSet<MagazinesByMachineFamily>            MagazinesByMachinesFamilies         { get; set; }
    public virtual DbSet<MagazinesBySoftware>                 MagazinesBySoftware                 { get; set; }
    public virtual DbSet<MagazineSynopsis>                    MagazineSynopses                    { get; set; }
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
    public virtual DbSet<MoneyDonation>                       MoneyDonations                      { get; set; }
    public virtual DbSet<News>                                News                                { get; set; }
    public virtual DbSet<OwnedMachine>                        OwnedMachines                       { get; set; }
    public virtual DbSet<PeopleByBook>                        PeopleByBooks                       { get; set; }
    public virtual DbSet<PeopleByCompany>                     PeopleByCompanies                   { get; set; }
    public virtual DbSet<PeopleByDocument>                    PeopleByDocuments                   { get; set; }
    public virtual DbSet<PeopleByMagazine>                    PeopleByMagazines                   { get; set; }
    public virtual DbSet<Person>                              People                              { get; set; }
    public virtual DbSet<PersonDescription>                   PersonDescriptions                  { get; set; }
    public virtual DbSet<Processor>                           Processors                          { get; set; }
    public virtual DbSet<ProcessorsByMachine>                 ProcessorsByMachine                 { get; set; }
    public virtual DbSet<Resolution>                          Resolutions                         { get; set; }
    public virtual DbSet<ResolutionsByGpu>                    ResolutionsByGpu                    { get; set; }
    public virtual DbSet<ResolutionsByScreen>                 ResolutionsByScreen                 { get; set; }
    public virtual DbSet<Screen>                              Screens                             { get; set; }
    public virtual DbSet<ScreensByMachine>                    ScreensByMachine                    { get; set; }
    public virtual DbSet<SoftwareFamily>                      SoftwareFamilies                    { get; set; }
    public virtual DbSet<Software>                            Softwares                           { get; set; }
    public virtual DbSet<SoftwareVersion>                     SoftwareVersions                    { get; set; }
    public virtual DbSet<SoftwarePlatform>                    SoftwarePlatforms                   { get; set; }
    public virtual DbSet<SoftwareRelease>                     SoftwareReleases                    { get; set; }
    public virtual DbSet<SoftwareBarcode>                     SoftwareBarcodes                    { get; set; }
    public virtual DbSet<SoftwareProductCode>                 SoftwareProductCodes                { get; set; }
    public virtual DbSet<SoftwareRequirement>                 SoftwareRequirements                { get; set; }
    public virtual DbSet<SoftwareOSCompatibility>             SoftwareOSCompatibility             { get; set; }
    public virtual DbSet<SoftwareCompanyRole>                 SoftwareCompanyRoles                { get; set; }
    public virtual DbSet<SoftwareRole>                        SoftwareRoles                       { get; set; }
    public virtual DbSet<CompanyBySoftwareVersion>            CompaniesBySoftwareVersions         { get; set; }
    public virtual DbSet<CompanyBySoftwareFamily>             CompaniesBySoftwareFamilies         { get; set; }
    public virtual DbSet<MinimumGpuBySoftwareRelease>        MinimumGpuBySoftwareRelease         { get; set; }
    public virtual DbSet<RecommendedGpuBySoftwareRelease>    RecommendedGpuBySoftwareRelease     { get; set; }
    public virtual DbSet<SoundSynthBySoftwareRelease>        SoundSynthBySoftwareRelease         { get; set; }
    public virtual DbSet<SoftwareCompilation>                 SoftwareCompilations                { get; set; }
    public virtual DbSet<SoftwareVersionBySoftwareCompilation> SoftwareVersionBySoftwareCompilation { get; set; }
    public virtual DbSet<SoftwareBySoftwareCompilation>       SoftwareBySoftwareCompilation        { get; set; }
    public virtual DbSet<SoftwareCompilationBySoftwareCompilation> SoftwareCompilationBySoftwareCompilation { get; set; }
    public virtual DbSet<UnM49>                              UnM49                               { get; set; }
    public virtual DbSet<UnM49BySoftwareRelease>             UnM49BySoftwareRelease              { get; set; }
    public virtual DbSet<LanguageBySoftwareRelease>          LanguageBySoftwareRelease            { get; set; }
    public virtual DbSet<SoftwareScreenshot>                  SoftwareScreenshots                  { get; set; }
    public virtual DbSet<SoftwareScreenshotCaptionTranslation> SoftwareScreenshotCaptionTranslations { get; set; }
    public virtual DbSet<SoftwareScreenshotGroup>             SoftwareScreenshotGroups             { get; set; }
    public virtual DbSet<SoftwareScreenshotGroupTranslation>  SoftwareScreenshotGroupTranslations  { get; set; }
    public virtual DbSet<SoftwareCover>                      SoftwareCovers                       { get; set; }
    public virtual DbSet<SoftwareCoverCaptionTranslation>    SoftwareCoverCaptionTranslations     { get; set; }
    public virtual DbSet<PeopleBySoftwareRoleTranslation>    PeopleBySoftwareRoleTranslations     { get; set; }
    public virtual DbSet<SoftwarePromoArt>                   SoftwarePromoArt                     { get; set; }
    public virtual DbSet<SoftwarePromoArtGroup>              SoftwarePromoArtGroups               { get; set; }
    public virtual DbSet<SoftwarePromoArtGroupTranslation>   SoftwarePromoArtGroupTranslations    { get; set; }
    public virtual DbSet<SoftwareDescription>                  SoftwareDescriptions                 { get; set; }
    public virtual DbSet<SoftwareAlternativeTitle>              SoftwareAlternativeTitles            { get; set; }
    public virtual DbSet<SoftwareAlternativeTitleCommentTranslation> SoftwareAlternativeTitleCommentTranslations { get; set; }
    public virtual DbSet<ExternalSite>                        ExternalSites                        { get; set; }
    public virtual DbSet<SoftwareExternalId>                  SoftwareExternalIds                  { get; set; }
    public virtual DbSet<SoftwareSimilarTo>                   SoftwareSimilarTo                    { get; set; }
    public virtual DbSet<SoundByMachine>                      SoundByMachine                      { get; set; }
    public virtual DbSet<SoundSynth>                          SoundSynths                         { get; set; }
    public virtual DbSet<StandaloneFile>                      StandaloneFiles                     { get; set; }
    public virtual DbSet<StorageByMachine>                    StorageByMachine                    { get; set; }
    public virtual DbSet<SoftwarePlatformsByMachine>          SoftwarePlatformsByMachine          { get; set; }
    public virtual DbSet<CollectedBook>                       CollectedBooks                      { get; set; }
    public virtual DbSet<CollectedDocument>                   CollectedDocuments                  { get; set; }
    public virtual DbSet<CollectedMagazineIssue>              CollectedMagazineIssues             { get; set; }
    public virtual DbSet<CollectedSoftwareRelease>            CollectedSoftwareReleases           { get; set; }
    public virtual DbSet<SoftwareGenre>                      SoftwareGenres                      { get; set; }
    public virtual DbSet<SoftwareGenreTranslation>           SoftwareGenreTranslations           { get; set; }
    public virtual DbSet<GenreBySoftware>                    GenresBySoftware                    { get; set; }
    public virtual DbSet<PeopleBySoftware>                   PeopleBySoftware                    { get; set; }
    public virtual DbSet<SoftwareAttribute>                  SoftwareAttributes                  { get; set; }
    public virtual DbSet<SoftwareAttributeString>            SoftwareAttributeStrings            { get; set; }
    public virtual DbSet<SoftwareAttributeStringTranslation> SoftwareAttributeStringTranslations { get; set; }
    public virtual DbSet<MobyGamesImportState>               MobyGamesImportStates               { get; set; }
    public virtual DbSet<MobyGamesDiscoveredGame>            MobyGamesDiscoveredGames            { get; set; }
    public virtual DbSet<MobyGamesRejection>                 MobyGamesRejections                 { get; set; }
    public virtual DbSet<MobyGamesCoverDownloadState>        MobyGamesCoverDownloadStates        { get; set; }
    public virtual DbSet<MobyGamesPromoArtDownloadState>    MobyGamesPromoArtDownloadStates     { get; set; }
    public virtual DbSet<MobyGamesScreenshotDownloadState>  MobyGamesScreenshotDownloadStates   { get; set; }
    public virtual DbSet<MobyGamesVideoImportState>          MobyGamesVideoImportStates          { get; set; }
    public virtual DbSet<SoftwareVideo>                      SoftwareVideos                      { get; set; }
    public virtual DbSet<MachineVideo>                       MachineVideos                       { get; set; }
    public virtual DbSet<GpuVideo>                           GpuVideos                           { get; set; }
    public virtual DbSet<ProcessorVideo>                     ProcessorVideos                     { get; set; }
    public virtual DbSet<SoundSynthVideo>                    SoundSynthVideos                    { get; set; }
    public virtual DbSet<SoftwareCriticReview>               SoftwareCriticReviews               { get; set; }
    public virtual DbSet<MobyGamesReviewImportState>         MobyGamesReviewImportStates         { get; set; }
    public virtual DbSet<SoftwareUserRating>                  SoftwareUserRatings                 { get; set; }
    public virtual DbSet<SoftwareUserReview>                  SoftwareUserReviews                 { get; set; }
    public virtual DbSet<SoftwareUserReviewVote>              SoftwareUserReviewVotes             { get; set; }
    public virtual DbSet<ReviewReport>                        ReviewReports                       { get; set; }
    public virtual DbSet<Conversation>                        Conversations                       { get; set; }
    public virtual DbSet<ConversationParticipant>             ConversationParticipants            { get; set; }
    public virtual DbSet<Message>                             Messages                            { get; set; }
    public virtual DbSet<MessageState>                        MessageStates                       { get; set; }
    public virtual DbSet<MessageReport>                       MessageReports                      { get; set; }
    public virtual DbSet<SearchEntry>                         SearchEntries                       { get; set; }
    public virtual DbSet<Suggestion>                          Suggestions                         { get; set; }
    public virtual DbSet<OldDosCategory>                      OldDosCategories                    { get; set; }
    public virtual DbSet<OldDosSoftware>                      OldDosSoftwares                     { get; set; }
    public virtual DbSet<OldDosVersion>                       OldDosVersions                      { get; set; }
    public virtual DbSet<OldDosOsPlatformMap>                 OldDosOsPlatformMaps                { get; set; }
    public virtual DbSet<WwpcCategory>                        WwpcCategories                      { get; set; }
    public virtual DbSet<WwpcSoftware>                        WwpcSoftwares                       { get; set; }
    public virtual DbSet<WwpcVersion>                         WwpcVersions                        { get; set; }
    public virtual DbSet<WwpcScreenshot>                      WwpcScreenshots                     { get; set; }
    public virtual DbSet<RankingDefinition>                   RankingDefinitions                  { get; set; }
    public virtual DbSet<RankingEntry>                        RankingEntries                      { get; set; }
    public virtual DbSet<SoftwareScore>                       SoftwareScores                      { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(optionsBuilder.IsConfigured) return;

        IConfigurationBuilder builder       = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        IConfigurationRoot    configuration = builder.Build();

        optionsBuilder
           .UseMySql(configuration.GetConnectionString("DefaultConnection"),
                     new MariaDbServerVersion(new Version(12, 0, 2)),
                     b => b.UseMicrosoftJson()
                             .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
           .UseLazyLoadingProxies()
           .AddMarechaiInterceptors();
    }

    public async Task<int> SaveChangesWithUserAsync(string userId)
    {
        ChangeTracker.DetectChanges();
        List<Audit> audits = [];

        foreach(EntityEntry entry in ChangeTracker.Entries())
        {
            if(entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var audit = new Audit();
            audit.UserId = userId;
            audit.Table  = entry.Metadata.GetTableName();

            Dictionary<string, object> keys    = new();
            Dictionary<string, object> olds    = new();
            Dictionary<string, object> news    = new();
            List<string>               columns = [];

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

            if(keys.Count > 0) audit.Keys = keys;

            if(olds.Count > 0) audit.OldValues = olds;

            if(news.Count > 0) audit.NewValues = news;

            if(columns.Count > 0) audit.AffectedColumns = columns;

            audits.Add(audit);
        }

        await Audit.AddRangeAsync(audits);

        return await SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        MethodInfo naturalSortKeyMethod = typeof(MarechaiContext).GetMethod(nameof(NaturalSortKey),
            BindingFlags.Public | BindingFlags.Static,
            [typeof(string)])!;

        modelBuilder.HasDbFunction(naturalSortKeyMethod).HasName("NaturalSortKey");

        MethodInfo normalizeForDuplicateMethod = typeof(MarechaiContext).GetMethod(nameof(NormalizeForDuplicate),
            BindingFlags.Public | BindingFlags.Static,
            [typeof(string)])!;

        modelBuilder.HasDbFunction(normalizeForDuplicateMethod).HasName("NormalizeForDuplicate");

        MethodInfo normalizeForMatchMethod = typeof(MarechaiContext).GetMethod(nameof(NormalizeForMatch),
            BindingFlags.Public | BindingFlags.Static,
            [typeof(string)])!;

        modelBuilder.HasDbFunction(normalizeForMatchMethod).HasName("NormalizeForMatch");

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(e => e.Title);

            entity.HasIndex(e => e.NativeTitle);

            entity.HasIndex(e => e.Published);

            entity.HasIndex(e => e.CountryId);

            entity.HasIndex(e => e.Isbn);

            entity.HasIndex(e => e.Pages);

            entity.HasIndex(e => e.Edition);

            entity.HasOne(d => d.Previous)
                  .WithOne(d => d.Next)
                  .HasForeignKey<Book>(d => d.PreviousId)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Source)
                  .WithMany(d => d.Derivates)
                  .HasForeignKey(d => d.SourceId)
                  .OnDelete(DeleteBehavior.ClientSetNull);

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

        modelBuilder.Entity<BookSynopsis>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.BookId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_book_synopses_book_language");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_book_synopses_language");

            entity.HasOne(e => e.Book)
                  .WithMany(b => b.Synopses)
                  .HasForeignKey(e => e.BookId);
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

            entity.Property(e => e.Agif).HasColumnName("agif").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Browser)
                  .IsRequired()
                  .HasColumnName("browser")
                  .HasColumnType("varchar(64)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Colors).HasColumnName("colors").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Flash).HasColumnName("flash").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Frames).HasColumnName("frames").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Gif87).HasColumnName("gif87").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Gif89).HasColumnName("gif89").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Jpeg).HasColumnName("jpeg").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Js).HasColumnName("js").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Os)
                  .IsRequired()
                  .HasColumnName("os")
                  .HasColumnType("varchar(32)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Platform)
                  .IsRequired()
                  .HasColumnName("platform")
                  .HasColumnType("varchar(8)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Png).HasColumnName("png").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Pngt).HasColumnName("pngt").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Table).HasColumnName("table").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.UserAgent)
                  .IsRequired()
                  .HasColumnName("user_agent")
                  .HasColumnType("varchar(128)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Version)
                  .IsRequired()
                  .HasColumnName("version")
                  .HasColumnType("varchar(16)")
                  .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<MarechaiDb>(entity =>
        {
            entity.ToTable("marechai_db");

            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

            entity.Property(e => e.Updated)
                  .HasColumnName("updated")
                  .HasColumnType("datetime")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

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

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasColumnName("name")
                  .HasColumnType("varchar(128)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasColumnType("varchar(25)");

            entity.Property(e => e.Province).HasColumnName("province").HasColumnType("varchar(80)");

            entity.Property(e => e.Sold).HasColumnName("sold").HasColumnType("datetime");

            entity.Property(e => e.SoldToId).HasColumnName("sold_to").HasColumnType("int(11)");

            entity.Property(e => e.Status).HasColumnName("status").HasColumnType("int(11)");

            entity.Property(e => e.Twitter).HasColumnName("twitter").HasColumnType("varchar(45)");

            entity.Property(e => e.Website).HasColumnName("website").HasColumnType("varchar(255)");

            entity.HasOne(d => d.Country)
                  .WithMany(p => p.Companies)
                  .HasForeignKey(d => d.CountryId)
                  .HasConstraintName("fk_companies_country");

            entity.HasOne(d => d.SoldTo)
                  .WithMany(p => p.InverseSoldToNavigation)
                  .HasForeignKey(d => d.SoldToId)
                  .HasConstraintName("fk_companies_sold_to");

        });

        modelBuilder.Entity<CompanyDescription>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.CompanyId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_company_descriptions_company_language");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_company_descriptions_language");
        });

        modelBuilder.Entity<SoftwareGenreTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.GenreId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_software_genre_translations_genre_language");

            entity.HasOne(e => e.Genre)
                  .WithMany()
                  .HasForeignKey(e => e.GenreId)
                  .HasConstraintName("fk_software_genre_translations_genre")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_software_genre_translations_language");
        });

        modelBuilder.Entity<SoftwareAttributeString>(entity =>
        {
            entity.HasIndex(e => e.Text)
                  .IsUnique()
                  .HasDatabaseName("idx_software_attribute_strings_text");
        });

        modelBuilder.Entity<SoftwareAttributeStringTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.StringId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_software_attribute_string_translations_string_language");

            entity.HasOne(e => e.String)
                  .WithMany()
                  .HasForeignKey(e => e.StringId)
                  .HasConstraintName("fk_software_attribute_string_translations_string")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_software_attribute_string_translations_language");
        });

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

            entity.HasOne(d => d.Company)
                  .WithMany(p => p.Logos)
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("fk_company_logos_company1");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(e => e.Title);

            entity.HasIndex(e => e.NativeTitle);

            entity.HasIndex(e => e.Published);

            entity.HasIndex(e => e.CountryId);

            entity.HasOne(d => d.Country).WithMany(p => p.Documents).HasForeignKey(d => d.CountryId);
        });

        modelBuilder.Entity<DocumentSynopsis>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.DocumentId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_document_synopses_document_language");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_document_synopses_language");

            entity.HasOne(e => e.Document)
                  .WithMany(d => d.Synopses)
                  .HasForeignKey(e => e.DocumentId);
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

            entity.Property(e => e.Browser)
                  .IsRequired()
                  .HasColumnName("browser")
                  .HasColumnType("char(128)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Date)
                  .IsRequired()
                  .HasColumnName("date")
                  .HasColumnType("char(20)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Ip)
                  .IsRequired()
                  .HasColumnName("ip")
                  .HasColumnType("char(16)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Referer)
                  .IsRequired()
                  .HasColumnName("referer")
                  .HasColumnType("char(255)")
                  .HasDefaultValueSql("''");
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

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasColumnName("name")
                  .HasColumnType("char(128)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Package).HasColumnName("package").HasColumnType("varchar(45)");

            entity.Property(e => e.Process).HasColumnName("process").HasColumnType("varchar(45)");

            entity.Property(e => e.ProcessNm).HasColumnName("process_nm");

            entity.Property(e => e.Transistors).HasColumnName("transistors").HasColumnType("bigint(20)");

            entity.HasOne(d => d.Company)
                  .WithMany(p => p.Gpus)
                  .HasForeignKey(d => d.CompanyId)
                  .HasConstraintName("fk_gpus_company");
        });

        modelBuilder.Entity<GpusByMachine>(entity =>
        {
            entity.ToTable("gpus_by_machine");

            entity.HasIndex(e => e.GpuId).HasDatabaseName("idx_gpus_by_machine_gpus");

            entity.HasIndex(e => e.MachineId).HasDatabaseName("idx_gpus_by_machine_machine");

            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("bigint(20)");

            entity.Property(e => e.GpuId).HasColumnName("gpu").HasColumnType("int(11)");

            entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

            entity.HasOne(d => d.Gpu)
                  .WithMany(p => p.GpusByMachine)
                  .HasForeignKey(d => d.GpuId)
                  .HasConstraintName("fk_gpus_by_machine_gpu");

            entity.HasOne(d => d.Machine)
                  .WithMany(p => p.Gpus)
                  .HasForeignKey(d => d.MachineId)
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

            entity.HasOne(d => d.Extension)
                  .WithMany(p => p.InstructionSetExtensionsByProcessor)
                  .HasForeignKey(d => d.ExtensionId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("fk_extension_extension_id");

            entity.HasOne(d => d.Processor)
                  .WithMany(p => p.InstructionSetExtensions)
                  .HasForeignKey(d => d.ProcessorId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
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

            entity.Property(e => e.Browser)
                  .IsRequired()
                  .HasColumnName("browser")
                  .HasColumnType("char(128)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Date)
                  .IsRequired()
                  .HasColumnName("date")
                  .HasColumnType("char(20)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Ip)
                  .IsRequired()
                  .HasColumnName("ip")
                  .HasColumnType("char(16)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Referer)
                  .IsRequired()
                  .HasColumnName("referer")
                  .HasColumnType("char(255)")
                  .HasDefaultValueSql("''");
        });

        modelBuilder.Entity<MachineFamily>(entity =>
        {
            entity.ToTable("machine_families");

            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_machine_families_company");

            entity.HasIndex(e => e.Name).HasDatabaseName("idx_machine_families_name");

            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

            entity.Property(e => e.CompanyId).HasColumnName("company").HasColumnType("int(11)");

            entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(255)");

            entity.HasOne(d => d.Company)
                  .WithMany(p => p.MachineFamilies)
                  .HasForeignKey(d => d.CompanyId)
                  .HasConstraintName("fk_machine_families_company");
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

            // Type+Introduced composite covers the device controllers' year-bucketed
            // queries (`/computers/by-year/{N}`, `/consoles/by-year/{N}`, etc.). The
            // existing standalone (Type) and (Introduced) indexes each serve one
            // axis; MariaDB can index-merge but a composite avoids the merge cost
            // entirely and lets the YEAR(Introduced) filter degrade to a seek.
            entity.HasIndex(e => new { e.Type, e.Introduced })
                  .HasDatabaseName("idx_machines_type_introduced");

            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

            entity.Property(e => e.CompanyId)
                  .HasColumnName("company")
                  .HasColumnType("int(11)")
                  .HasDefaultValueSql("'0'");

            entity.Property(e => e.FamilyId).HasColumnName("family").HasColumnType("int(11)");

            entity.Property(e => e.Introduced).HasColumnName("introduced").HasColumnType("datetime");

            entity.Property(e => e.Model).HasColumnName("model").HasColumnType("varchar(50)");

            entity.Property(e => e.Name).IsRequired().HasColumnName("name").HasColumnType("varchar(255)");

            entity.Property(e => e.Type)
                  .HasColumnName("type")
                  .HasColumnType("int(11)")
                  .HasDefaultValue(MachineType.Unknown);

            entity.HasOne(d => d.Company)
                  .WithMany(p => p.Machines)
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("fk_machines_company");

            entity.HasOne(d => d.Family)
                  .WithMany(p => p.Machines)
                  .HasForeignKey(d => d.FamilyId)
                  .HasConstraintName("fk_machines_family");
        });

        modelBuilder.Entity<MachineDescription>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.MachineId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_machine_descriptions_machine_language");

            entity.Property(e => e.LanguageCode).UseCollation("utf8mb4_general_ci");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_machine_descriptions_language");
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

        modelBuilder.Entity<GpuPhoto>(entity =>
        {
            // Foreign-key + sort columns used by the per-GPU photos endpoint
            // (`/gpus/{id}/photos`) and by the consolidated /gpus/{id}/full
            // endpoint. Without these indexes the query degenerates to a full
            // table scan + filesort over the entire gpu_photos table because
            // the historical EXIF indexes do not cover the (GpuId, sort)
            // access pattern. Mirrors the MachinePhoto retrofit (see below).
            entity.HasIndex(e => e.GpuId).HasDatabaseName("idx_gpu_photos_gpu");

            entity.HasIndex(e => new { e.GpuId, e.CreatedOn, e.Id })
                  .HasDatabaseName("idx_gpu_photos_gpu_created");

            entity.HasIndex(e => e.Aperture);

            entity.HasIndex(e => e.Author);

            entity.HasIndex(e => e.CameraManufacturer);

            entity.HasIndex(e => e.CameraModel);

            entity.HasIndex(e => e.ColorSpace);

            // `Comments` is intentionally NOT indexed: the EXIF UserComment payload
            // can exceed varchar(255) (the implicit type Pomelo picks for indexed
            // strings) and no query in the codebase filters or sorts by it. Leaving
            // it un-indexed lets EF emit `longtext` (Pomelo default for unconstrained
            // strings) so values up to the EXIF 2.x spec limit of 1023 bytes — and
            // beyond — fit without truncation.
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

            entity.HasOne(d => d.Gpu).WithMany(p => p.Photos).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User).WithMany().OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.License).WithMany().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MachinePhoto>(entity =>
        {
            // Foreign-key + sort columns used by the per-machine photos endpoint
            // (`/machines/{id}/photos`). Without these indexes the query degenerates
            // to a full table scan + filesort over the entire machine_photos table
            // because the historical EXIF indexes do not cover the (MachineId, sort)
            // access pattern.
            entity.HasIndex(e => e.MachineId).HasDatabaseName("idx_machine_photos_machine");

            entity.HasIndex(e => new { e.MachineId, e.CreatedOn, e.Id })
                  .HasDatabaseName("idx_machine_photos_machine_created");

            entity.HasIndex(e => e.Aperture);

            entity.HasIndex(e => e.Author);

            entity.HasIndex(e => e.CameraManufacturer);

            entity.HasIndex(e => e.CameraModel);

            entity.HasIndex(e => e.ColorSpace);

            // `Comments` intentionally un-indexed — see GpuPhoto block above.
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

        modelBuilder.Entity<ProcessorPhoto>(entity =>
        {
            // Foreign-key + sort indexes for the per-processor photo lookup hit by the
            // public processor view (`/processors/{id}/photos`) and by the consolidated
            // /processors/{id}/full endpoint. Without these the query degenerates to a
            // full table scan + filesort because the historical EXIF indexes do not
            // cover the (ProcessorId, sort) access pattern. Mirrors the GpuPhoto and
            // MachinePhoto retrofits.
            entity.HasIndex(e => e.ProcessorId).HasDatabaseName("idx_processor_photos_processor");

            entity.HasIndex(e => new { e.ProcessorId, e.CreatedOn, e.Id })
                  .HasDatabaseName("idx_processor_photos_processor_created");

            entity.HasIndex(e => e.Aperture);

            entity.HasIndex(e => e.Author);

            entity.HasIndex(e => e.CameraManufacturer);

            entity.HasIndex(e => e.CameraModel);

            entity.HasIndex(e => e.ColorSpace);

            // `Comments` intentionally un-indexed — see GpuPhoto block above.
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

            entity.HasOne(d => d.Processor).WithMany(p => p.Photos).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User).WithMany().OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.License).WithMany().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SoundSynthPhoto>(entity =>
        {
            entity.HasIndex(e => e.Aperture);

            entity.HasIndex(e => e.Author);

            entity.HasIndex(e => e.CameraManufacturer);

            entity.HasIndex(e => e.CameraModel);

            entity.HasIndex(e => e.ColorSpace);

            // `Comments` intentionally un-indexed — see GpuPhoto block above.
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

            // FK index for /sound-synths/{id}/photos lookups (matches the
            // MachinePhoto/GpuPhoto/ProcessorPhoto pattern: this FK was missing
            // among the 30+ EXIF indexes). Composite (SoundSynthId, CreatedOn,
            // Id) backs the OrderBy(CreatedOn).ThenBy(Id) sort.
            entity.HasIndex(e => e.SoundSynthId).HasDatabaseName("idx_sound_synth_photos_sound_synth");

            entity.HasIndex(e => new { e.SoundSynthId, e.CreatedOn, e.Id })
                  .HasDatabaseName("idx_sound_synth_photos_sound_synth_created");

            entity.HasOne(d => d.SoundSynth).WithMany(p => p.Photos).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User).WithMany().OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.License).WithMany().OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Magazine>(entity =>
        {
            entity.HasIndex(e => e.Title);

            entity.HasIndex(e => e.NativeTitle);

            entity.HasIndex(e => e.Published);

            entity.HasIndex(e => e.CountryId);

            entity.HasIndex(e => e.Issn);

            entity.HasIndex(e => e.FirstPublication);

            entity.HasOne(d => d.Country).WithMany(p => p.Magazines).HasForeignKey(d => d.CountryId);
        });

        modelBuilder.Entity<MagazineSynopsis>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.MagazineId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_magazine_synopses_magazine_language");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_magazine_synopses_language");

            entity.HasOne(e => e.Magazine)
                  .WithMany(m => m.Synopses)
                  .HasForeignKey(e => e.MagazineId);
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

        modelBuilder.Entity<MagazinesBySoftware>(entity =>
        {
            entity.HasIndex(e => e.MagazineId);

            entity.HasIndex(e => e.SoftwareId);

            entity.HasOne(d => d.Magazine).WithMany(p => p.Software).HasForeignKey(d => d.MagazineId);

            entity.HasOne(d => d.Software).WithMany(p => p.Magazines).HasForeignKey(d => d.SoftwareId);
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

            entity.Property(e => e.Type)
                  .HasColumnName("type")
                  .HasColumnType("int(11)")
                  .HasDefaultValue(MemoryType.Unknown);

            entity.Property(e => e.Usage)
                  .HasColumnName("usage")
                  .HasColumnType("int(11)")
                  .HasDefaultValue(MemoryUsage.Unknown);

            entity.HasOne(d => d.Machine)
                  .WithMany(p => p.Memory)
                  .HasForeignKey(d => d.MachineId)
                  .HasConstraintName("fk_memory_by_machine_machine");
        });

        modelBuilder.Entity<MoneyDonation>(entity =>
        {
            entity.ToTable("money_donations");

            entity.HasIndex(e => e.Donator).HasDatabaseName("idx_money_donations_donator");

            entity.HasIndex(e => e.Quantity).HasDatabaseName("idx_money_donations_quantity");

            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

            entity.Property(e => e.Donator)
                  .IsRequired()
                  .HasColumnName("donator")
                  .HasColumnType("char(128)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Quantity)
                  .HasColumnName("quantity")
                  .HasColumnType("decimal(11,2)")
                  .HasDefaultValueSql("'0.00'");
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.ToTable("news");

            entity.HasIndex(e => e.AddedId).HasDatabaseName("idx_news_ip");

            entity.HasIndex(e => e.Date).HasDatabaseName("idx_news_date");

            entity.HasIndex(e => e.Type).HasDatabaseName("idx_news_type");

            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

            entity.Property(e => e.AddedId)
                  .HasColumnName("added_id")
                  .HasColumnType("bigint(20)")
                  .HasDefaultValueSql("'0'");

            entity.Property(e => e.Date).IsRequired().HasColumnName("date").HasColumnType("datetime");

            entity.Property(e => e.Type)
                  .HasColumnName("type")
                  .HasColumnType("int(11)")
                  .HasDefaultValue(NewsType.NewComputerInDb);

            entity.Property(e => e.Name)
                  .HasColumnName("name")
                  .HasColumnType("longtext")
                  .IsRequired(false);
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
            entity.ToTable("PeopleByCompany");

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
        });

        modelBuilder.Entity<PersonDescription>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.PersonId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_person_descriptions_person_language");

            entity.Property(e => e.LanguageCode).UseCollation("utf8mb4_general_ci");

            entity.HasOne(e => e.Person)
                  .WithMany(p => p.Descriptions)
                  .HasForeignKey(e => e.PersonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_person_descriptions_language");
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

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasColumnName("name")
                  .HasColumnType("char(50)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.Package).HasColumnName("package").HasColumnType("varchar(45)");

            entity.Property(e => e.Process).HasColumnName("process").HasColumnType("varchar(45)");

            entity.Property(e => e.ProcessNm).HasColumnName("process_nm");

            entity.Property(e => e.SimdRegisters).HasColumnName("SIMD_registers").HasColumnType("int(11)");

            entity.Property(e => e.SimdSize).HasColumnName("SIMD_size").HasColumnType("int(11)");

            entity.Property(e => e.Speed).HasColumnName("speed");

            entity.Property(e => e.ThreadsPerCore).HasColumnName("threads_per_core").HasColumnType("int(11)");

            entity.Property(e => e.Transistors).HasColumnName("transistors").HasColumnType("bigint(20)");

            entity.HasOne(d => d.Company)
                  .WithMany(p => p.Processors)
                  .HasForeignKey(d => d.CompanyId)
                  .HasConstraintName("fk_processors_company");

            entity.HasOne(d => d.InstructionSet)
                  .WithMany(p => p.Processors)
                  .HasForeignKey(d => d.InstructionSetId)
                  .HasConstraintName("fk_processors_instruction_set");
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

            entity.HasOne(d => d.Machine)
                  .WithMany(p => p.Processors)
                  .HasForeignKey(d => d.MachineId)
                  .HasConstraintName("fk_processors_by_machine_machine");

            entity.HasOne(d => d.Processor)
                  .WithMany(p => p.ProcessorsByMachine)
                  .HasForeignKey(d => d.ProcessorId)
                  .HasConstraintName("fk_processors_by_machine_processor");
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
                   })
                  .HasDatabaseName("idx_resolutions_resolution");

            entity.HasIndex(e => new
                   {
                       e.Width,
                       e.Height,
                       e.Colors
                   })
                  .HasDatabaseName("idx_resolutions_resolution_with_color");

            entity.HasIndex(e => new
                   {
                       e.Width,
                       e.Height,
                       e.Colors,
                       e.Palette
                   })
                  .HasDatabaseName("idx_resolutions_resolution_with_color_and_palette");

            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("int(11)");

            entity.Property(e => e.Chars).HasColumnName("chars").HasColumnType("tinyint(1)").HasDefaultValue(false);

            entity.Property(e => e.Colors).HasColumnName("colors").HasColumnType("bigint(20)");

            entity.Property(e => e.Height).HasColumnName("height").HasColumnType("int(11)").HasDefaultValueSql("'0'");

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

            entity.HasOne(d => d.Gpu)
                  .WithMany(p => p.ResolutionsByGpu)
                  .HasForeignKey(d => d.GpuId)
                  .HasConstraintName("fk_resolutions_by_gpu_gpu");

            entity.HasOne(d => d.Resolution)
                  .WithMany(p => p.ResolutionsByGpu)
                  .HasForeignKey(d => d.ResolutionId)
                  .HasConstraintName("fk_resolutions_by_gpu_resolution");
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

            entity.HasOne(d => d.Machine)
                  .WithMany(p => p.Sound)
                  .HasForeignKey(d => d.MachineId)
                  .HasConstraintName("fk_sound_by_machine_machine");

            entity.HasOne(d => d.SoundSynth)
                  .WithMany(p => p.SoundByMachine)
                  .HasForeignKey(d => d.SoundSynthId)
                  .HasConstraintName("fk_sound_by_machine_sound_synth");
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

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasColumnName("name")
                  .HasColumnType("char(50)")
                  .HasDefaultValueSql("''");

            entity.Property(e => e.SquareWave).HasColumnName("square_wave").HasColumnType("int(11)");

            entity.Property(e => e.Type).HasColumnName("type").HasColumnType("int(11)");

            entity.Property(e => e.Voices).HasColumnName("voices").HasColumnType("int(11)");

            entity.Property(e => e.WhiteNoise).HasColumnName("white_noise").HasColumnType("int(11)");

            entity.HasOne(d => d.Company)
                  .WithMany(p => p.SoundSynths)
                  .HasForeignKey(d => d.CompanyId)
                  .HasConstraintName("fk_sound_synths_company");
        });

        modelBuilder.Entity<SoundSynthDescription>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.SoundSynthId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_sound_synth_descriptions_sound_synth_language");

            entity.Property(e => e.LanguageCode).UseCollation("utf8mb4_general_ci");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_sound_synth_descriptions_language");
        });

        modelBuilder.Entity<ProcessorDescription>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.ProcessorId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_processor_descriptions_processor_language");

            entity.Property(e => e.LanguageCode).UseCollation("utf8mb4_general_ci");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_processor_descriptions_language");
        });

        modelBuilder.Entity<GpuDescription>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.GpuId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_gpu_descriptions_gpu_language");

            entity.Property(e => e.LanguageCode).UseCollation("utf8mb4_general_ci");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_gpu_descriptions_language");
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

            entity.Property(e => e.Interface)
                  .HasColumnName("interface")
                  .HasColumnType("int(11)")
                  .HasDefaultValue(StorageInterface.Unknown);

            entity.Property(e => e.MachineId).HasColumnName("machine").HasColumnType("int(11)");

            entity.Property(e => e.Type)
                  .HasColumnName("type")
                  .HasColumnType("int(11)")
                  .HasDefaultValue(StorageType.Unknown);

            entity.HasOne(d => d.Machine)
                  .WithMany(p => p.Storage)
                  .HasForeignKey(d => d.MachineId)
                  .HasConstraintName("fk_storage_by_machine_machine");
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

        modelBuilder.Entity<CurrencyInflation>(entity => { entity.HasIndex(d => d.Year); });

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
            entity.Property(e => e.Md5).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha1).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha256).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha3).HasConversion(_hexToBytesConverter);

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
            entity.Property(e => e.Md5).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha1).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha256).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha3).HasConversion(_hexToBytesConverter);

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
            entity.Property(e => e.Md5).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha1).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha256).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha3).HasConversion(_hexToBytesConverter);

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
            entity.Property(e => e.Md5).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha1).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha256).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha3).HasConversion(_hexToBytesConverter);

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
            entity.Property(e => e.Md5).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha1).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha256).HasConversion(_hexToBytesConverter);
            entity.Property(e => e.Sha3).HasConversion(_hexToBytesConverter);

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
            entity.HasOne(d => d.MediaFile)
                  .WithMany(p => p.DataStreams)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FilesByFilesystem>(entity =>
        {
            entity.HasOne(d => d.Filesystem)
                  .WithMany(p => p.Files)
                  .OnDelete(DeleteBehavior.Cascade);
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

        modelBuilder.Entity<FileDataStreamsByStandaloneFile>(entity =>
        {
            entity.HasOne(d => d.StandaloneFile)
                  .WithMany(p => p.DataStreams)
                  .OnDelete(DeleteBehavior.Cascade);
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

        modelBuilder.Entity<SoftwareFamily>(entity =>
        {
            entity.HasIndex(x => x.Name);

            entity.HasOne(x => x.Parent)
                  .WithMany(x => x.Children)
                  .HasForeignKey(x => x.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Software>(entity =>
        {
            entity.HasIndex(x => x.Name);

            entity.HasOne(x => x.Family)
                  .WithMany(x => x.Softwares)
                  .HasForeignKey(x => x.FamilyId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.PredecessorId);

            entity.HasOne(x => x.Predecessor)
                  .WithMany(x => x.Successors)
                  .HasForeignKey(x => x.PredecessorId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.BaseSoftwareId);

            entity.HasOne(x => x.BaseSoftware)
                  .WithMany(x => x.Addons)
                  .HasForeignKey(x => x.BaseSoftwareId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SoftwareCompilation>(entity =>
        {
            entity.HasIndex(x => x.Name);

            entity.HasOne(x => x.Software)
                  .WithMany()
                  .HasForeignKey(x => x.SoftwareId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Machine)
                  .WithMany()
                  .HasForeignKey(x => x.MachineId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.PredecessorId);

            entity.HasOne(x => x.Predecessor)
                  .WithMany(x => x.Successors)
                  .HasForeignKey(x => x.PredecessorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SoftwareBySoftwareCompilation>(entity =>
        {
            entity.HasKey(x => new
            {
                x.SoftwareCompilationId,
                x.SoftwareId
            });

            entity.HasOne(x => x.SoftwareCompilation)
                  .WithMany(x => x.IncludedSoftware)
                  .HasForeignKey(x => x.SoftwareCompilationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Software)
                  .WithMany(x => x.CompilationMemberships)
                  .HasForeignKey(x => x.SoftwareId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwareVersionBySoftwareCompilation>(entity =>
        {
            entity.HasKey(x => new
            {
                x.SoftwareCompilationId,
                x.SoftwareVersionId
            });

            entity.HasOne(x => x.SoftwareCompilation)
                  .WithMany(x => x.IncludedVersions)
                  .HasForeignKey(x => x.SoftwareCompilationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SoftwareVersion)
                  .WithMany(x => x.CompilationMemberships)
                  .HasForeignKey(x => x.SoftwareVersionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwareCompilationBySoftwareCompilation>(entity =>
        {
            entity.HasKey(x => new
            {
                x.ParentCompilationId,
                x.ChildCompilationId
            });

            entity.HasOne(x => x.ParentCompilation)
                  .WithMany(x => x.IncludedCompilations)
                  .HasForeignKey(x => x.ParentCompilationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ChildCompilation)
                  .WithMany(x => x.ContainingCompilations)
                  .HasForeignKey(x => x.ChildCompilationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwareDescription>(entity =>
        {
            entity.HasIndex(e => e.Text).IsFullText();

            entity.HasIndex(e => new { e.SoftwareId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_software_descriptions_software_language");

            entity.Property(e => e.LanguageCode).UseCollation("utf8mb4_general_ci");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_software_descriptions_language");
        });

        modelBuilder.Entity<SoftwareAlternativeTitle>(entity =>
        {
            entity.HasIndex(e => e.SoftwareId).HasDatabaseName("idx_software_alternative_titles_software");

            entity.HasOne(e => e.Software)
                  .WithMany(s => s.AlternativeTitles)
                  .HasForeignKey(e => e.SoftwareId)
                  .HasConstraintName("fk_software_alternative_titles_software")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExternalSite>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("idx_external_sites_name");
        });

        modelBuilder.Entity<SoftwareExternalId>(entity =>
        {
            entity.HasIndex(e => new { e.ExternalSiteId, e.ExternalId })
                  .IsUnique()
                  .HasDatabaseName("idx_software_external_ids_site_external_id");

            entity.HasIndex(e => e.SoftwareId).HasDatabaseName("idx_software_external_ids_software");

            entity.HasOne(e => e.Software)
                  .WithMany(s => s.ExternalIds)
                  .HasForeignKey(e => e.SoftwareId)
                  .HasConstraintName("fk_software_external_ids_software")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ExternalSite)
                  .WithMany(s => s.SoftwareExternalIds)
                  .HasForeignKey(e => e.ExternalSiteId)
                  .HasConstraintName("fk_software_external_ids_external_site")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SoftwareSimilarTo>(entity =>
        {
            entity.HasKey(x => new { x.SoftwareId, x.SimilarSoftwareId });

            entity.HasIndex(e => e.SimilarSoftwareId).HasDatabaseName("idx_software_similar_to_similar_software");

            entity.HasOne(x => x.Software)
                  .WithMany(s => s.SimilarToLeft)
                  .HasForeignKey(x => x.SoftwareId)
                  .HasConstraintName("fk_software_similar_to_software")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SimilarSoftware)
                  .WithMany(s => s.SimilarToRight)
                  .HasForeignKey(x => x.SimilarSoftwareId)
                  .HasConstraintName("fk_software_similar_to_similar_software")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwareAlternativeTitleCommentTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.CommentText, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_software_alternative_title_comment_translations_text_language");

            entity.HasIndex(e => e.CommentText)
                  .HasDatabaseName("idx_software_alternative_title_comment_translations_text");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_software_alternative_title_comment_translations_language");
        });

        modelBuilder.Entity<SoftwareVersion>(entity =>
        {
            entity.HasIndex(x => x.VersionString);

            entity.HasOne(x => x.Software)
                  .WithMany(x => x.Versions)
                  .HasForeignKey(x => x.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ParentVersion)
                  .WithMany(x => x.Children)
                  .HasForeignKey(x => x.ParentVersionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwarePlatform>(entity => { entity.HasIndex(x => x.Name).IsUnique(); });

        modelBuilder.Entity<SoftwarePlatformsByMachine>(entity =>
        {
            entity.ToTable("software_platforms_by_machine");

            entity.HasIndex(e => e.SoftwarePlatformId)
                  .HasDatabaseName("idx_software_platforms_by_machine_software_platform");

            entity.HasIndex(e => e.MachineId)
                  .HasDatabaseName("idx_software_platforms_by_machine_machine");

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasColumnType("bigint(20)");

            entity.Property(e => e.SoftwarePlatformId)
                  .HasColumnName("software_platform")
                  .HasColumnType("bigint(20) unsigned");

            entity.Property(e => e.MachineId)
                  .HasColumnName("machine")
                  .HasColumnType("int(11)");

            entity.HasOne(d => d.SoftwarePlatform)
                  .WithMany(p => p.Machines)
                  .HasForeignKey(d => d.SoftwarePlatformId)
                  .HasConstraintName("fk_software_platforms_by_machine_software_platform");

            entity.HasOne(d => d.Machine)
                  .WithMany(p => p.SoftwarePlatforms)
                  .HasForeignKey(d => d.MachineId)
                  .HasConstraintName("fk_software_platforms_by_machine_machine");
        });

        modelBuilder.Entity<SoftwareRelease>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.SoftwareVersionId,
                x.PlatformId
            });

            entity.HasOne(x => x.SoftwareVersion)
                  .WithMany(x => x.Releases)
                  .HasForeignKey(x => x.SoftwareVersionId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Software)
                  .WithMany(x => x.DirectReleases)
                  .HasForeignKey(x => x.SoftwareId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SoftwareCompilation)
                  .WithMany(x => x.Releases)
                  .HasForeignKey(x => x.SoftwareCompilationId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Platform)
                  .WithMany(x => x.SoftwareReleases)
                  .HasForeignKey(x => x.PlatformId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Publisher)
                  .WithMany(x => x.SoftwareReleases)
                  .HasForeignKey(x => x.PublisherId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwareCover>(entity =>
        {
            entity.HasIndex(x => x.SoftwareReleaseId);
            entity.HasIndex(x => x.SoftwareId);
            entity.HasIndex(x => x.SoftwareCompilationId);
            entity.HasIndex(x => x.GroupId);

            // Type-leading composite for the FrontCoverId backfill query
            // (`PopulateFrontCoverIdsAsync` in SoftwareController), which filters
            // `Type == Front` first and then joins to SoftwareReleases on
            // SoftwareReleaseId. Putting Type first lets MariaDB skip the ~half
            // of rows that are back covers / spine / disc / etc.
            entity.HasIndex(x => new { x.Type, x.SoftwareReleaseId })
                  .HasDatabaseName("idx_software_covers_type_release");

            entity.HasOne(x => x.Software)
                  .WithMany(x => x.Covers)
                  .HasForeignKey(x => x.SoftwareId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Release)
                  .WithMany(x => x.Covers)
                  .HasForeignKey(x => x.SoftwareReleaseId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.SoftwareCompilation)
                  .WithMany(x => x.Covers)
                  .HasForeignKey(x => x.SoftwareCompilationId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UnM49>(entity =>
        {
            entity.HasIndex(x => x.ParentId);
            entity.HasIndex(x => x.Type);

            entity.HasOne(x => x.Parent)
                  .WithMany(x => x.Children)
                  .HasForeignKey(x => x.ParentId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UnM49BySoftwareRelease>(entity =>
        {
            entity.HasKey(x => new { x.SoftwareReleaseId, x.UnM49Id });

            entity.HasOne(x => x.SoftwareRelease)
                  .WithMany(x => x.Regions)
                  .HasForeignKey(x => x.SoftwareReleaseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UnM49)
                  .WithMany(x => x.SoftwareReleases)
                  .HasForeignKey(x => x.UnM49Id)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LanguageBySoftwareRelease>(entity =>
        {
            entity.HasKey(x => new { x.SoftwareReleaseId, x.LanguageCode });

            entity.Property(x => x.LanguageCode).UseCollation("utf8mb4_general_ci");

            entity.HasOne(x => x.SoftwareRelease)
                  .WithMany(x => x.Languages)
                  .HasForeignKey(x => x.SoftwareReleaseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Language)
                  .WithMany()
                  .HasForeignKey(x => x.LanguageCode)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwareBarcode>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();

            entity.HasOne(x => x.Release).WithMany(x => x.Barcodes).HasForeignKey(x => x.ReleaseId);
        });

        modelBuilder.Entity<SoftwareProductCode>(entity =>
        {
            entity.HasIndex(x => new
                   {
                       x.Issuer,
                       x.Code
                   })
                  .IsUnique();

            entity.HasOne(x => x.Release).WithMany(x => x.ProductCodes).HasForeignKey(x => x.ReleaseId);
        });

        modelBuilder.Entity<SoftwareRequirement>(entity =>
        {
            entity.HasKey(x => new
            {
                x.SoftwareVersionId,
                x.RequiredSoftwareVersionId,
                x.RequirementType
            });

            entity.HasOne(x => x.SoftwareVersion)
                  .WithMany(x => x.Requirements)
                  .HasForeignKey(x => x.SoftwareVersionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RequiredSoftwareVersion)
                  .WithMany()
                  .HasForeignKey(x => x.RequiredSoftwareVersionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwareOSCompatibility>(entity =>
        {
            entity.HasKey(x => new
            {
                x.SoftwareVersionId,
                x.OSVersionId
            });

            entity.HasOne(x => x.SoftwareVersion)
                  .WithMany(x => x.OSCompatibility)
                  .HasForeignKey(x => x.SoftwareVersionId);

            entity.HasOne(x => x.OSVersion)
                  .WithMany()
                  .HasForeignKey(x => x.OSVersionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SoftwareRole>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Enabled);
            entity.Property(p => p.Enabled).HasDefaultValue(true);
        });

        modelBuilder.Entity<SoftwareCompanyRole>(entity =>
        {
            entity.HasKey(x => new
            {
                x.SoftwareId,
                x.CompanyId,
                x.RoleId
            });

            entity.HasOne(x => x.Software).WithMany(x => x.CompanyRoles).HasForeignKey(x => x.SoftwareId);

            entity.HasOne(x => x.Company).WithMany(x => x.SoftwareRoles).HasForeignKey(x => x.CompanyId);

            entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<CompanyBySoftwareVersion>(entity =>
        {
            entity.HasIndex(e => e.SoftwareVersionId);
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.RoleId);
            entity.HasOne(d => d.SoftwareVersion).WithMany(p => p.Companies).HasForeignKey(d => d.SoftwareVersionId);
            entity.HasOne(d => d.Company).WithMany(p => p.SoftwareVersions).HasForeignKey(d => d.CompanyId);
            entity.HasOne(d => d.Role).WithMany().HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<CompanyBySoftwareFamily>(entity =>
        {
            entity.HasIndex(e => e.SoftwareFamilyId);
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.RoleId);
            entity.HasOne(d => d.SoftwareFamily).WithMany(p => p.Companies).HasForeignKey(d => d.SoftwareFamilyId);
            entity.HasOne(d => d.Company).WithMany(p => p.SoftwareFamilies2).HasForeignKey(d => d.CompanyId);
            entity.HasOne(d => d.Role).WithMany().HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<MinimumGpuBySoftwareRelease>(entity =>
        {
            entity.HasKey(x => new
            {
                x.ReleaseId,
                x.GpuId
            });

            entity.HasOne(x => x.Release).WithMany(x => x.MinimumGpus).HasForeignKey(x => x.ReleaseId);

            entity.HasOne(x => x.Gpu).WithMany(x => x.MinimumForSoftwareReleases).HasForeignKey(x => x.GpuId);
        });

        modelBuilder.Entity<RecommendedGpuBySoftwareRelease>(entity =>
        {
            entity.HasKey(x => new
            {
                x.ReleaseId,
                x.GpuId
            });

            entity.HasOne(x => x.Release).WithMany(x => x.RecommendedGpus).HasForeignKey(x => x.ReleaseId);

            entity.HasOne(x => x.Gpu).WithMany(x => x.RecommendedForSoftwareReleases).HasForeignKey(x => x.GpuId);
        });

        modelBuilder.Entity<SoundSynthBySoftwareRelease>(entity =>
        {
            entity.HasKey(x => new
            {
                x.ReleaseId,
                x.SoundSynthId
            });

            entity.HasOne(x => x.Release).WithMany(x => x.SupportedSoundSynths).HasForeignKey(x => x.ReleaseId);

            entity.HasOne(x => x.SoundSynth)
                  .WithMany(x => x.SupportedBySoftwareReleases)
                  .HasForeignKey(x => x.SoundSynthId);
        });


        modelBuilder.Entity<CollectedBook>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.BookId });

            entity.HasIndex(e => e.BookId);

            entity.HasOne(e => e.User)
                  .WithMany(p => p.CollectedBooks)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Book)
                  .WithMany(p => p.CollectedBy)
                  .HasForeignKey(e => e.BookId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CollectedDocument>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.DocumentId });

            entity.HasIndex(e => e.DocumentId);

            entity.HasOne(e => e.User)
                  .WithMany(p => p.CollectedDocuments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Document)
                  .WithMany(p => p.CollectedBy)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CollectedSoftwareRelease>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SoftwareReleaseId });

            entity.HasIndex(e => e.SoftwareReleaseId);

            entity.HasOne(e => e.User)
                  .WithMany(p => p.CollectedSoftwareReleases)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SoftwareRelease)
                  .WithMany(p => p.CollectedBy)
                  .HasForeignKey(e => e.SoftwareReleaseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CollectedMagazineIssue>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.MagazineIssueId });

            entity.HasIndex(e => e.MagazineIssueId);

            entity.HasOne(e => e.User)
                  .WithMany(p => p.CollectedMagazineIssues)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MagazineIssue)
                  .WithMany(p => p.CollectedBy)
                  .HasForeignKey(e => e.MagazineIssueId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SoftwareGenre>(entity =>
        {
            entity.HasIndex(e => new { e.Name, e.Type }).IsUnique();
        });

        modelBuilder.Entity<GenreBySoftware>(entity =>
        {
            entity.HasKey(e => new { e.SoftwareId, e.GenreId });

            // Reverse-lookup: "which software titles share this genre". Without
            // a standalone GenreId index, the composite PK only accelerates the
            // SoftwareId-leading path; a query keyed by GenreId alone (e.g. the
            // genre landing page) degenerates to a full table scan.
            entity.HasIndex(e => e.GenreId).HasDatabaseName("idx_genres_by_software_genre");

            entity.HasOne(e => e.Software)
                  .WithMany(p => p.Genres)
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Genre)
                  .WithMany(p => p.Softwares)
                  .HasForeignKey(e => e.GenreId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PeopleBySoftware>(entity =>
        {
            entity.HasIndex(e => new { e.SoftwareId, e.PersonId, e.Role }).IsUnique();

            // Reverse-lookup: "what did this person work on". Without an index on
            // PersonId alone, the unique composite cannot serve PersonId-leading
            // queries and the credit endpoint falls back to a full scan.
            entity.HasIndex(e => e.PersonId).HasDatabaseName("idx_people_by_software_person");

            // Role-filtered scan (e.g. "show all programmers"). Optional FK; the
            // SetNull cascade means RoleId can be null which is fine for an index.
            entity.HasIndex(e => e.RoleId).HasDatabaseName("idx_people_by_software_role");

            entity.HasOne(e => e.Software)
                  .WithMany(p => p.Credits)
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Person)
                  .WithMany()
                  .HasForeignKey(e => e.PersonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.DocumentRole)
                  .WithMany()
                  .HasForeignKey(e => e.RoleId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SoftwareAttribute>(entity =>
        {
            entity.HasIndex(e => new { e.SoftwareReleaseId, e.Category, e.Key });

            entity.HasOne(e => e.SoftwareRelease)
                  .WithMany(p => p.Attributes)
                  .HasForeignKey(e => e.SoftwareReleaseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MobyGamesImportState>(entity =>
        {
            entity.HasIndex(e => e.MobyGameId).IsUnique();
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.SoftwareCompilation)
                  .WithMany()
                  .HasForeignKey(e => e.SoftwareCompilationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IgdbPlatform>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.MatchStatus);
            entity.HasIndex(e => e.SoftwarePlatformId);
        });

        modelBuilder.Entity<IgdbCompany>(entity =>
        {
            entity.HasIndex(e => e.IgdbId).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.MatchStatus);
            entity.HasIndex(e => e.CompanyId);
        });

        modelBuilder.Entity<IgdbGame>(entity =>
        {
            entity.HasIndex(e => e.IgdbId).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.MatchStatus);
            entity.HasIndex(e => e.SoftwareId);
            entity.HasIndex(e => e.ParentGameId);
            entity.HasIndex(e => e.VersionParentId);
            entity.HasIndex(e => e.GameTypeId);
        });

        modelBuilder.Entity<IgdbGameType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<IgdbInvolvedCompany>(entity =>
        {
            entity.HasIndex(e => e.IgdbId).IsUnique();
            entity.HasIndex(e => e.GameIgdbId);
            entity.HasIndex(e => e.CompanyIgdbId);
        });

        modelBuilder.Entity<IgdbAlternativeName>(entity =>
        {
            entity.HasIndex(e => e.IgdbId).IsUnique();
            entity.HasIndex(e => e.GameIgdbId);
        });

        modelBuilder.Entity<OldDosCategory>(entity =>
        {
            entity.HasIndex(e => e.ParentId);

            entity.HasOne(e => e.Parent)
                  .WithMany(p => p.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OldDosSoftware>(entity =>
        {
            entity.HasIndex(e => e.SourceUrl).IsUnique();
            entity.HasIndex(e => e.SourceId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OldDosCategoryId);

            entity.HasOne(e => e.OldDosCategory)
                  .WithMany(p => p.Softwares)
                  .HasForeignKey(e => e.OldDosCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OldDosVersion>(entity =>
        {
            entity.HasIndex(e => e.OldDosSoftwareId);

            entity.HasOne(e => e.OldDosSoftware)
                  .WithMany(p => p.Versions)
                  .HasForeignKey(e => e.OldDosSoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OldDosOsPlatformMap>(entity =>
        {
            entity.HasOne(e => e.SoftwarePlatform)
                  .WithMany()
                  .HasForeignKey(e => e.SoftwarePlatformId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WwpcCategory>(entity =>
        {
            entity.HasIndex(e => new { e.ProductType, e.Name }).IsUnique();
        });

        modelBuilder.Entity<WwpcSoftware>(entity =>
        {
            entity.HasIndex(e => e.SourceUrl).IsUnique();
            entity.HasIndex(e => e.Slug);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ProductType);
            entity.HasIndex(e => e.WwpcCategoryId);
            entity.HasIndex(e => e.SuggestedVendorCompanyId);

            entity.HasOne(e => e.WwpcCategory)
                  .WithMany(p => p.Softwares)
                  .HasForeignKey(e => e.WwpcCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WwpcVersion>(entity =>
        {
            entity.HasIndex(e => e.WwpcSoftwareId);
            entity.HasIndex(e => new { e.WwpcSoftwareId, e.MajorRelease, e.VersionString });

            entity.HasOne(e => e.WwpcSoftware)
                  .WithMany(p => p.Versions)
                  .HasForeignKey(e => e.WwpcSoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WwpcScreenshot>(entity =>
        {
            entity.HasIndex(e => e.WwpcSoftwareId);
            entity.HasIndex(e => e.SourceUrl).IsUnique();

            entity.HasOne(e => e.WwpcSoftware)
                  .WithMany(p => p.Screenshots)
                  .HasForeignKey(e => e.WwpcSoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MobyGamesDiscoveredGame>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.NumericId);
            entity.HasIndex(e => e.ReleaseYear);
            entity.HasIndex(e => e.RawFetchedAt);
        });

        modelBuilder.Entity<SoftwarePromoArtGroup>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<SoftwarePromoArt>(entity =>
        {
            entity.HasIndex(e => e.SoftwareId);
            entity.HasIndex(e => e.GroupId);

            entity.HasOne(e => e.Software)
                  .WithMany(p => p.PromoArt)
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Group)
                  .WithMany(p => p.PromoArt)
                  .HasForeignKey(e => e.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SoftwarePromoArtGroupTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.GroupId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_software_promo_art_group_translations_group_language");

            entity.HasOne(e => e.Group)
                  .WithMany()
                  .HasForeignKey(e => e.GroupId)
                  .HasConstraintName("fk_software_promo_art_group_translations_group")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_software_promo_art_group_translations_language");
        });

        modelBuilder.Entity<SoftwareCoverCaptionTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.CaptionText, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_software_cover_caption_translations_text_language");

            entity.HasIndex(e => e.CaptionText)
                  .HasDatabaseName("idx_software_cover_caption_translations_text");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_software_cover_caption_translations_language");
        });

        modelBuilder.Entity<PeopleBySoftwareRoleTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.RoleText, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_people_by_software_role_translations_text_language");

            entity.HasIndex(e => e.RoleText)
                  .HasDatabaseName("idx_people_by_software_role_translations_text");

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_people_by_software_role_translations_language");
        });

        modelBuilder.Entity<SoftwareScreenshotCaptionTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.ScreenshotId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_software_screenshot_caption_translations_screenshot_language");

            entity.HasOne(e => e.Screenshot)
                  .WithMany()
                  .HasForeignKey(e => e.ScreenshotId)
                  .HasConstraintName("fk_software_screenshot_caption_translations_screenshot")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_software_screenshot_caption_translations_language");
        });

        modelBuilder.Entity<SoftwareScreenshotGroup>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<SoftwareScreenshot>(entity =>
        {
            entity.HasIndex(e => e.GroupId);

            entity.HasOne(e => e.Group)
                  .WithMany(g => g.Screenshots)
                  .HasForeignKey(e => e.GroupId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SoftwareScreenshotGroupTranslation>(entity =>
        {
            entity.HasIndex(e => new { e.GroupId, e.LanguageCode })
                  .IsUnique()
                  .HasDatabaseName("idx_software_screenshot_group_translations_group_language");

            entity.HasOne(e => e.Group)
                  .WithMany()
                  .HasForeignKey(e => e.GroupId)
                  .HasConstraintName("fk_software_screenshot_group_translations_group")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageCode)
                  .HasConstraintName("fk_software_screenshot_group_translations_language");
        });

        modelBuilder.Entity<MobyGamesPromoArtDownloadState>(entity =>
        {
            entity.HasIndex(e => e.PromoPageUrl).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.MobyGameId);
        });

        modelBuilder.Entity<MobyGamesScreenshotDownloadState>(entity =>
        {
            entity.HasIndex(e => e.ScreenshotPageUrl).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.MobyGameId);
        });

        modelBuilder.Entity<SoftwareVideo>(entity =>
        {
            entity.HasIndex(e => e.SoftwareId);
            entity.HasIndex(e => new { e.SoftwareId, e.Provider, e.VideoId }).IsUnique();

            entity.HasOne(e => e.Software)
                  .WithMany(p => p.Videos)
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MachineVideo>(entity =>
        {
            entity.HasIndex(e => e.MachineId);
            entity.HasIndex(e => new { e.MachineId, e.Provider, e.VideoId }).IsUnique();

            entity.HasOne(e => e.Machine)
                  .WithMany(p => p.Videos)
                  .HasForeignKey(e => e.MachineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GpuVideo>(entity =>
        {
            entity.HasIndex(e => e.GpuId);
            entity.HasIndex(e => new { e.GpuId, e.Provider, e.VideoId }).IsUnique();

            entity.HasOne(e => e.Gpu)
                  .WithMany(p => p.Videos)
                  .HasForeignKey(e => e.GpuId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProcessorVideo>(entity =>
        {
            entity.HasIndex(e => e.ProcessorId);
            entity.HasIndex(e => new { e.ProcessorId, e.Provider, e.VideoId }).IsUnique();

            entity.HasOne(e => e.Processor)
                  .WithMany(p => p.Videos)
                  .HasForeignKey(e => e.ProcessorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SoundSynthVideo>(entity =>
        {
            entity.HasIndex(e => e.SoundSynthId);
            entity.HasIndex(e => new { e.SoundSynthId, e.Provider, e.VideoId }).IsUnique();

            entity.HasOne(e => e.SoundSynth)
                  .WithMany(p => p.Videos)
                  .HasForeignKey(e => e.SoundSynthId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MobyGamesVideoImportState>(entity =>
        {
            entity.HasIndex(e => e.VideoUrl);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.MobyGameId);
            entity.HasIndex(e => e.SoftwareId);
        });

        modelBuilder.Entity<MobyGamesRejection>(entity =>
        {
            entity.HasIndex(e => e.MobyGameId);
            entity.HasIndex(e => e.ReviewAction);
        });

        modelBuilder.Entity<SoftwareCriticReview>(entity =>
        {
            entity.HasIndex(e => new { e.SoftwareId, e.MagazineId, e.PlatformId }).IsUnique();
            entity.HasIndex(e => e.SoftwareId);
            entity.HasIndex(e => e.MagazineId);
            entity.HasIndex(e => e.PlatformId);

            entity.HasOne(e => e.Software)
                  .WithMany(s => s.CriticReviews)
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Magazine)
                  .WithMany()
                  .HasForeignKey(e => e.MagazineId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Platform)
                  .WithMany()
                  .HasForeignKey(e => e.PlatformId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MobyGamesReviewImportState>(entity =>
        {
            entity.HasIndex(e => e.MobyGameId).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<SoftwareUserRating>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SoftwareId });

            entity.HasIndex(e => e.SoftwareId);

            entity.HasOne(e => e.User)
                  .WithMany(p => p.SoftwareRatings)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Software)
                  .WithMany(p => p.UserRatings)
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SoftwareUserReview>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.SoftwareId }).IsUnique();
            entity.HasIndex(e => e.SoftwareId);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                  .WithMany(p => p.SoftwareReviews)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Software)
                  .WithMany(p => p.UserReviews)
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SoftwareUserReviewVote>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ReviewId });

            entity.HasIndex(e => e.ReviewId);

            entity.HasOne(e => e.User)
                  .WithMany(p => p.ReviewVotes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Review)
                  .WithMany(p => p.Votes)
                  .HasForeignKey(e => e.ReviewId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewReport>(entity =>
        {
            entity.HasIndex(e => new { e.ReporterId, e.ReviewId }).IsUnique();
            entity.HasIndex(e => e.ReviewId);
            entity.HasIndex(e => e.IsResolved);

            entity.HasOne(e => e.Reporter)
                  .WithMany(p => p.ReviewReports)
                  .HasForeignKey(e => e.ReporterId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Review)
                  .WithMany(p => p.Reports)
                  .HasForeignKey(e => e.ReviewId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ResolvedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ResolvedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasIndex(e => e.IsSystemThread);
        });

        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ConversationId, e.UserId });

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Conversation)
                  .WithMany(p => p.Participants)
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(e => new { e.ConversationId, e.CreatedOn });
            entity.HasIndex(e => e.SenderId);

            entity.HasOne(e => e.Conversation)
                  .WithMany(p => p.Messages)
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentMessage)
                  .WithMany()
                  .HasForeignKey(e => e.ParentMessageId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MessageState>(entity =>
        {
            entity.HasKey(e => new { e.MessageId, e.UserId });

            entity.HasIndex(e => new { e.UserId, e.IsRead, e.DeletedAt });

            entity.HasOne(e => e.Message)
                  .WithMany(p => p.States)
                  .HasForeignKey(e => e.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MessageReport>(entity =>
        {
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.IsResolved);

            entity.HasOne(e => e.Reporter)
                  .WithMany()
                  .HasForeignKey(e => e.ReporterId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Message)
                  .WithMany()
                  .HasForeignKey(e => e.MessageId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ResolvedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ResolvedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvitationCode>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.Property(e => e.Code)
                  .HasMaxLength(9)
                  .IsRequired();

            entity.Property(e => e.RowVersion).IsConcurrencyToken();

            entity.HasIndex(e => e.UsedById);
            entity.HasIndex(e => e.CreatedOn);

            entity.HasOne(e => e.CreatedBy)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UsedBy)
                  .WithMany()
                  .HasForeignKey(e => e.UsedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SearchEntry>(entity =>
        {
            entity.ToTable("SearchEntries");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EntityType)
                  .HasConversion<byte>()
                  .IsRequired();

            entity.Property(e => e.EntityId).IsRequired();

            entity.Property(e => e.DisplayName)
                  .HasMaxLength(512)
                  .IsRequired();

            entity.Property(e => e.AltName).HasMaxLength(512);

            entity.Property(e => e.NormalizedName)
                  .HasMaxLength(1024)
                  .IsRequired();

            entity.Property(e => e.Soundex).HasMaxLength(10);

            entity.HasIndex(e => new { e.EntityType, e.EntityId }).IsUnique();
            entity.HasIndex(e => new { e.EntityType, e.Year });
            entity.HasIndex(e => new { e.EntityType, e.CountryId });
            entity.HasIndex(e => new { e.EntityType, e.CompanyId });
            entity.HasIndex(e => new { e.EntityType, e.Kind });
            entity.HasIndex(e => e.Soundex);
            // FULLTEXT (with ngram parser) on NormalizedName is created via raw SQL in the migration
            // body because Pomelo's IsFullText() does not expose the parser option.
        });

        modelBuilder.Entity<Suggestion>(entity =>
        {
            entity.ToTable("Suggestions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EntityType).HasConversion<byte>().IsRequired();
            entity.Property(e => e.Status).HasConversion<byte>().IsRequired();

            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.Subkey, e.Status });
            entity.HasIndex(e => e.CreatedById);
            entity.HasIndex(e => new { e.Status, e.CreatedOn });

            entity.HasOne(e => e.CreatedBy)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RankingDefinition>(entity =>
        {
            entity.Property(e => e.Dimension).HasConversion<byte>().IsRequired();

            // One row per (dimension, dimensionId) pair. NULL DimensionId is allowed
            // only for the single Dimension=All row; the unique index treats NULL as a
            // distinct value so a uniqueness violation only ever fires on a duplicate
            // (Genre, genreId) or (Platform, platformId) — defence against worker bugs.
            entity.HasIndex(e => new { e.Dimension, e.DimensionId }).IsUnique();
        });

        modelBuilder.Entity<RankingEntry>(entity =>
        {
            entity.HasKey(e => new { e.RankingDefinitionId, e.SoftwareId });

            // Ordered fetch: "give me ranking N's top 250 by rank ascending".
            entity.HasIndex(e => new { e.RankingDefinitionId, e.Rank })
                  .HasDatabaseName("idx_ranking_entries_definition_rank");

            // Reverse-lookup: "which rankings does this software appear in" — drives the
            // chip row under the Marechai-score banner on the software detail page.
            entity.HasIndex(e => e.SoftwareId).HasDatabaseName("idx_ranking_entries_software");

            entity.HasOne(e => e.RankingDefinition)
                  .WithMany(p => p.Entries)
                  .HasForeignKey(e => e.RankingDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Software)
                  .WithMany()
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SoftwareScore>(entity =>
        {
            entity.HasKey(e => e.SoftwareId);

            // Browsing the eligible set by rank (e.g. "compute the median Marechai score").
            entity.HasIndex(e => e.GlobalRank).HasDatabaseName("idx_software_scores_global_rank");

            entity.HasOne(e => e.Software)
                  .WithMany()
                  .HasForeignKey(e => e.SoftwareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
