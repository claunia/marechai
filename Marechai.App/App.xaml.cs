using System;
using System.Net.Http;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.ViewModels;
using Marechai.App.Presentation.ViewModels.Admin;
using Marechai.App.Presentation.Views;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.App.Services.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Dispatching;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Multipart;
using Microsoft.UI.Xaml;
using Serilog;
using Serilog.Events;
using Uno.Extensions;
using Uno.Extensions.Authentication;
using Uno.Extensions.Hosting;
using Uno.Extensions.Toolkit;
using Uno.UI;

namespace Marechai.App;

public partial class App : PrismApplication
{
    private Window _mainWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override UIElement CreateShell() => Container.Resolve<MainPage>();

    protected override void ConfigureApp(IApplicationBuilder builder)
    {
        Resources.MergedDictionaries.Add(new AppResources());
    }

    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder
           .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory)
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                      .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: false);
            })
           .UseSerilog((context, config) =>
            {
                config.MinimumLevel.Is(LogEventLevel.Information)
                      .WriteTo.Console();
            });
    }

    protected override void ConfigureWindow(Window window)
    {
        _mainWindow = window;
#if DEBUG
//        window.UseStudio();
#endif
        window.SetWindowIcon();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Logging — create first since other services depend on it
        var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddSerilog(dispose: true);
        });

        containerRegistry.RegisterInstance<ILoggerFactory>(loggerFactory);
        containerRegistry.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));

        // Configuration — build from appsettings files so services can resolve IConfiguration
        System.Diagnostics.Debug.WriteLine($"[App] AppContext.BaseDirectory={AppContext.BaseDirectory}");
        System.Diagnostics.Debug.WriteLine($"[App] appsettings.json exists={System.IO.File.Exists(System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json"))}");
        var configuration = new ConfigurationBuilder()
                           .SetBasePath(AppContext.BaseDirectory)
                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                           .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: false)
                           .Build();

        foreach(var kvp in configuration.AsEnumerable())
            System.Diagnostics.Debug.WriteLine($"[App] Config: {kvp.Key} = {kvp.Value}");

        containerRegistry.RegisterInstance<IConfiguration>(configuration);

        // Localization via Microsoft.Extensions.Localization
        var locServices = new ServiceCollection();
        locServices.AddSingleton<ILoggerFactory>(loggerFactory);
        locServices.AddLocalization(options => options.ResourcesPath = "Resources");

        ServiceProvider locProvider = locServices.BuildServiceProvider();

        var locFactory = locProvider.GetRequiredService<IStringLocalizerFactory>();
        var stringLocalizer = locFactory.Create("Resources", typeof(App).Assembly.GetName().Name!);

        containerRegistry.RegisterInstance<IStringLocalizerFactory>(locFactory);
        containerRegistry.RegisterInstance<IStringLocalizer>(stringLocalizer);

        // Initialize localization helpers
        Presentation.Converters.Loc.Initialize(stringLocalizer);
        Presentation.Converters.LocalizeConverter.Initialize(stringLocalizer);

        // Create LocalizedStrings and add as XAML resource for binding
        var localizedStrings = new LocalizedStrings(stringLocalizer);
        containerRegistry.RegisterInstance(localizedStrings);
        Resources["Strings"] = localizedStrings;

        containerRegistry.RegisterInstance<IOptions<AppConfig>>(Options.Create(new AppConfig()));

        // HTTP client + Kiota ApiClient
        containerRegistry.RegisterSingleton<HttpAuthHandler>();
#if DEBUG
        containerRegistry.RegisterSingleton<DebugHttpHandler>();
#endif
        containerRegistry.RegisterSingleton<Client>(() =>
        {
            var tokenService = Container.Resolve<ITokenService>();

            var authHandler = new HttpAuthHandler(tokenService);

#if DEBUG
            var debugLogger  = loggerFactory.CreateLogger<DebugHttpHandler>();
            var debugHandler = new DebugHttpHandler(debugLogger, new HttpClientHandler());
            authHandler.InnerHandler = debugHandler;
#else
            authHandler.InnerHandler = new HttpClientHandler();
#endif

            var httpClient = new HttpClient(authHandler)
            {
                BaseAddress = new Uri("http://localhost:5023")
            };

            var authProvider              = new AnonymousAuthenticationProvider();
            var parseNodeFactory          = new JsonParseNodeFactory();
            var serializationWriterFactory = new CompositeSerializationWriterFactory();
            serializationWriterFactory.AddFactory(new JsonSerializationWriterFactory());
            serializationWriterFactory.AddFactory(new MultipartSerializationWriterFactory());

            var requestAdapter = new HttpClientRequestAdapter(authProvider, parseNodeFactory,
                                                              serializationWriterFactory, httpClient);

            return new Client(requestAdapter);
        });

        // Application services
        containerRegistry.RegisterSingleton<IColorThemeService, ColorThemeService>();
        containerRegistry.RegisterSingleton<IAuthenticationService, AuthService>();
        containerRegistry.RegisterSingleton<ITokenService, TokenService>();
        containerRegistry.RegisterSingleton<IJwtService, JwtService>();
        containerRegistry.RegisterSingleton<TwoFactorService>();
        containerRegistry.RegisterSingleton<FlagCache>();
        containerRegistry.RegisterSingleton<CompanyLogoCache>();
        containerRegistry.RegisterSingleton<MachinePhotoCache>();
        containerRegistry.RegisterSingleton<MachinePromoArtCache>();
        containerRegistry.RegisterSingleton<GpuPhotoCache>();
        containerRegistry.RegisterSingleton<ProcessorPhotoCache>();
        containerRegistry.RegisterSingleton<SoundSynthPhotoCache>();
        containerRegistry.RegisterSingleton<BookCoverCache>();
        containerRegistry.RegisterSingleton<MagazineIssueCoverCache>();
        containerRegistry.RegisterSingleton<SoftwareScreenshotCache>();
        containerRegistry.RegisterSingleton<SoftwareCoverCache>();
        containerRegistry.RegisterSingleton<SoftwarePromoArtCache>();
        containerRegistry.RegisterSingleton<ImageSourceFactory>(
            () => new ImageSourceFactory(DispatcherQueue.GetForCurrentThread()));
        containerRegistry.RegisterSingleton<NewsService>();
        containerRegistry.RegisterSingleton<SearchService>();
        containerRegistry.RegisterSingleton<ComputersService>();
        containerRegistry.RegisterSingleton<ConsolesService>();
        containerRegistry.RegisterSingleton<SmartphonesService>();
        containerRegistry.RegisterSingleton<PdasService>();
        containerRegistry.RegisterSingleton<TabletsService>();
        containerRegistry.RegisterSingleton<CompaniesService>();
        containerRegistry.RegisterSingleton<CompanyDetailService>();
        containerRegistry.RegisterSingleton<MachineFamiliesService>();
        containerRegistry.RegisterSingleton<CompanyLogosService>();
        containerRegistry.RegisterSingleton<MachinePhotosService>();
        containerRegistry.RegisterSingleton<MachinePromoArtService>();
        containerRegistry.RegisterSingleton<SoftwarePromoArtService>();
        containerRegistry.RegisterSingleton<LicensesService>();
        containerRegistry.RegisterSingleton<GpusService>();
        containerRegistry.RegisterSingleton<ProcessorsService>();
        containerRegistry.RegisterSingleton<SoundSynthsService>();
        containerRegistry.RegisterSingleton<PeopleService>();
        containerRegistry.RegisterSingleton<BooksService>();
        containerRegistry.RegisterSingleton<DocumentsService>();
        containerRegistry.RegisterSingleton<MagazinesService>();
        containerRegistry.RegisterSingleton<SoftwareService>();
        containerRegistry.RegisterSingleton<SoftwareFamiliesService>();
        containerRegistry.RegisterSingleton<SoftwarePlatformsService>();
        containerRegistry.RegisterSingleton<ExternalSitesService>();
        containerRegistry.RegisterSingleton<SoftwareVersionsService>();
        containerRegistry.RegisterSingleton<SoftwareReleasesService>();
        containerRegistry.RegisterSingleton<SoftwareCompilationsService>();
        containerRegistry.RegisterSingleton<SoftwareBrowsingService>();
        containerRegistry.RegisterSingleton<ProfileService>();
        containerRegistry.RegisterSingleton<MessagingService>();
        containerRegistry.RegisterSingleton<InvitationCodesService>();
        containerRegistry.RegisterSingleton<ReviewReportsService>();
        containerRegistry.RegisterSingleton<SuggestionsService>();
        containerRegistry.RegisterSingleton<ToastActivationService>();
        containerRegistry.RegisterSingleton<INativeToastService, NativeToastService>();
        containerRegistry.RegisterSingleton<MessageNotificationStateService>();
        containerRegistry.RegisterSingleton<WwpcImportsService>();
        containerRegistry.RegisterSingleton<IComputersListFilterContext, ComputersListFilterContext>();
        containerRegistry.RegisterSingleton<IConsolesListFilterContext, ConsolesListFilterContext>();
        containerRegistry.RegisterSingleton<ISmartphonesListFilterContext, SmartphonesListFilterContext>();
        containerRegistry.RegisterSingleton<IPdasListFilterContext, PdasListFilterContext>();
        containerRegistry.RegisterSingleton<ITabletsListFilterContext, TabletsListFilterContext>();
        containerRegistry.RegisterSingleton<IBooksListFilterContext, BooksListFilterContext>();
        containerRegistry.RegisterSingleton<IDocumentsListFilterContext, DocumentsListFilterContext>();
        containerRegistry.RegisterSingleton<IMagazinesListFilterContext, MagazinesListFilterContext>();
        containerRegistry.RegisterSingleton<IPeopleListFilterContext, PeopleListFilterContext>();
        containerRegistry.RegisterSingleton<ISoftwareListFilterContext, SoftwareListFilterContext>();

        // Register ViewModels explicitly — RegisterForNavigation creates conditional
        // (keyed) registrations that the ViewModelLocator can't resolve by type alone
        containerRegistry.RegisterSingleton<NewsViewModel>();
        containerRegistry.Register<MainViewModel>();
        containerRegistry.Register<LoginViewModel>();
        containerRegistry.Register<ForgotPasswordViewModel>();
        containerRegistry.Register<RegisterViewModel>();
        containerRegistry.Register<ResendConfirmationViewModel>();
        containerRegistry.Register<DeleteAccountViewModel>();
        containerRegistry.Register<DeletionPendingViewModel>();
        containerRegistry.Register<ComputersViewModel>();
        containerRegistry.Register<ComputersListViewModel>();
        containerRegistry.Register<ConsolesViewModel>();
        containerRegistry.Register<ConsolesListViewModel>();
        containerRegistry.Register<SmartphonesViewModel>();
        containerRegistry.Register<SmartphonesListViewModel>();
        containerRegistry.Register<PdasViewModel>();
        containerRegistry.Register<PdasListViewModel>();
        containerRegistry.Register<TabletsViewModel>();
        containerRegistry.Register<TabletsListViewModel>();
        containerRegistry.Register<CompaniesViewModel>();
        containerRegistry.Register<CompanyDetailViewModel>();
        containerRegistry.Register<MachineFamilyViewViewModel>();
        containerRegistry.Register<BooksViewModel>();
        containerRegistry.Register<BooksListViewModel>();
        containerRegistry.Register<BookViewViewModel>();
        containerRegistry.Register<DocumentsViewModel>();
        containerRegistry.Register<DocumentsListViewModel>();
        containerRegistry.Register<DocumentViewViewModel>();
        containerRegistry.Register<MagazinesViewModel>();
        containerRegistry.Register<MagazinesListViewModel>();
        containerRegistry.Register<MagazineViewViewModel>();
        containerRegistry.Register<MagazineIssueViewViewModel>();
        containerRegistry.Register<MagazineIssuesByYearViewModel>();
        containerRegistry.Register<PeopleViewModel>();
        containerRegistry.Register<PeopleListViewModel>();
        containerRegistry.Register<PersonViewViewModel>();
        containerRegistry.Register<MachineViewViewModel>();
        containerRegistry.Register<PhotoDetailViewModel>();
        containerRegistry.Register<MachinePromoArtDetailViewModel>();
        containerRegistry.Register<SoftwarePromoArtDetailViewModel>();
        containerRegistry.Register<GpusListViewModel>();
        containerRegistry.Register<GpuDetailViewModel>();
        containerRegistry.Register<GpuPhotoDetailViewModel>();
        containerRegistry.Register<ProcessorsListViewModel>();
        containerRegistry.Register<ProcessorDetailViewModel>();
        containerRegistry.Register<ProcessorPhotoDetailViewModel>();
        containerRegistry.Register<SoundSynthsListViewModel>();
        containerRegistry.Register<SoundSynthDetailViewModel>();
        containerRegistry.Register<SoundSynthPhotoDetailViewModel>();
        containerRegistry.Register<SettingsViewModel>();
        containerRegistry.Register<MessagesViewModel>();
        containerRegistry.Register<MessageThreadViewModel>();
        containerRegistry.Register<ComposeMessageViewModel>();
        containerRegistry.Register<UsersViewModel>();
        containerRegistry.Register<AdminCompaniesViewModel>();
        containerRegistry.Register<AdminGpusViewModel>();
        containerRegistry.Register<AdminProcessorsViewModel>();
        containerRegistry.Register<AdminInstructionSetsViewModel>();
        containerRegistry.Register<AdminInstructionSetExtensionsViewModel>();
        containerRegistry.Register<AdminResolutionsViewModel>();
        containerRegistry.Register<AdminSoundSynthsViewModel>();
        containerRegistry.Register<AdminScreensViewModel>();
        containerRegistry.Register<AdminMachinesViewModel>();
        containerRegistry.Register<AdminMachineFamiliesViewModel>();
        containerRegistry.Register<AdminCompanyLogosViewModel>();
        containerRegistry.Register<AdminMachinePhotosViewModel>();
        containerRegistry.Register<AdminPeopleViewModel>();
        containerRegistry.Register<AdminBooksViewModel>();
        containerRegistry.Register<AdminDocumentsViewModel>();
        containerRegistry.Register<AdminMagazinesViewModel>();
        containerRegistry.Register<SoftwareViewModel>();
        containerRegistry.Register<SoftwareListViewModel>();
        containerRegistry.Register<SoftwareViewViewModel>();
        containerRegistry.Register<SoftwareReleaseViewViewModel>();
        containerRegistry.Register<SoftwareCompilationViewViewModel>();
        containerRegistry.Register<SoftwareRankingsViewModel>();
        containerRegistry.Register<SoftwareRankingDetailViewModel>();
        containerRegistry.Register<ProfileViewModel>();
        containerRegistry.Register<AdminSoftwareViewModel>();
        containerRegistry.Register<AdminSoftwareFamiliesViewModel>();
        containerRegistry.Register<AdminSoftwarePlatformsViewModel>();
        containerRegistry.Register<AdminExternalSitesViewModel>();
        containerRegistry.Register<AdminSoftwareVersionsViewModel>();
        containerRegistry.Register<AdminSoftwareReleasesViewModel>();
        containerRegistry.Register<AdminSoftwareScreenshotsViewModel>();
        containerRegistry.Register<AdminWwpcImportsViewModel>();
        containerRegistry.Register<AdminWwpcImportReviewViewModel>();
        containerRegistry.Register<AdminMessageReportsViewModel>();
        containerRegistry.Register<AdminInvitationCodesViewModel>();
        containerRegistry.Register<AdminReviewReportsViewModel>();
        containerRegistry.Register<AdminSuggestionsViewModel>();
        containerRegistry.Register<AdminSuggestionReviewViewModel>();
        containerRegistry.Register<ScreenshotDetailViewModel>();
        containerRegistry.Register<CoverDetailViewModel>();

        // Register views for navigation
        containerRegistry.RegisterForNavigation<MainPage, MainViewModel>();
        containerRegistry.RegisterForNavigation<LoginPage, LoginViewModel>();
        containerRegistry.RegisterForNavigation<ForgotPasswordPage, ForgotPasswordViewModel>();
        containerRegistry.RegisterForNavigation<RegisterPage, RegisterViewModel>();
        containerRegistry.RegisterForNavigation<ResendConfirmationPage, ResendConfirmationViewModel>();
        containerRegistry.RegisterForNavigation<DeleteAccountPage, DeleteAccountViewModel>();
        containerRegistry.RegisterForNavigation<ChangePasswordPage, ChangePasswordViewModel>();
        containerRegistry.RegisterForNavigation<DeletionPendingPage, DeletionPendingViewModel>();
        containerRegistry.RegisterForNavigation<NewsPage, NewsViewModel>();
        containerRegistry.RegisterForNavigation<AboutPage, AboutViewModel>();
        containerRegistry.RegisterForNavigation<ContactPage, ContactViewModel>();
        containerRegistry.RegisterForNavigation<AdvancedSearchPage, AdvancedSearchViewModel>();
        containerRegistry.RegisterForNavigation<ComputersPage, ComputersViewModel>();
        containerRegistry.RegisterForNavigation<ComputersListPage, ComputersListViewModel>();
        containerRegistry.RegisterForNavigation<ConsolesPage, ConsolesViewModel>();
        containerRegistry.RegisterForNavigation<ConsolesListPage, ConsolesListViewModel>();
        containerRegistry.RegisterForNavigation<SmartphonesPage, SmartphonesViewModel>();
        containerRegistry.RegisterForNavigation<SmartphonesListPage, SmartphonesListViewModel>();
        containerRegistry.RegisterForNavigation<PdasPage, PdasViewModel>();
        containerRegistry.RegisterForNavigation<PdasListPage, PdasListViewModel>();
        containerRegistry.RegisterForNavigation<TabletsPage, TabletsViewModel>();
        containerRegistry.RegisterForNavigation<TabletsListPage, TabletsListViewModel>();
        containerRegistry.RegisterForNavigation<CompaniesPage, CompaniesViewModel>();
        containerRegistry.RegisterForNavigation<CompanyDetailPage, CompanyDetailViewModel>();
        containerRegistry.RegisterForNavigation<MachineFamilyViewPage, MachineFamilyViewViewModel>();
        containerRegistry.RegisterForNavigation<BooksPage, BooksViewModel>();
        containerRegistry.RegisterForNavigation<BooksListPage, BooksListViewModel>();
        containerRegistry.RegisterForNavigation<BookViewPage, BookViewViewModel>();
        containerRegistry.RegisterForNavigation<DocumentsPage, DocumentsViewModel>();
        containerRegistry.RegisterForNavigation<DocumentsListPage, DocumentsListViewModel>();
        containerRegistry.RegisterForNavigation<DocumentViewPage, DocumentViewViewModel>();
        containerRegistry.RegisterForNavigation<MagazinesPage, MagazinesViewModel>();
        containerRegistry.RegisterForNavigation<MagazinesListPage, MagazinesListViewModel>();
        containerRegistry.RegisterForNavigation<MagazineViewPage, MagazineViewViewModel>();
        containerRegistry.RegisterForNavigation<MagazineIssueViewPage, MagazineIssueViewViewModel>();
        containerRegistry.RegisterForNavigation<MagazineIssuesByYearPage, MagazineIssuesByYearViewModel>();
        containerRegistry.RegisterForNavigation<PeoplePage, PeopleViewModel>();
        containerRegistry.RegisterForNavigation<PeopleListPage, PeopleListViewModel>();
        containerRegistry.RegisterForNavigation<PersonViewPage, PersonViewViewModel>();
        containerRegistry.RegisterForNavigation<MachineViewPage, MachineViewViewModel>();
        containerRegistry.RegisterForNavigation<PhotoDetailPage, PhotoDetailViewModel>();
        containerRegistry.RegisterForNavigation<MachinePromoArtDetailPage, MachinePromoArtDetailViewModel>();
        containerRegistry.RegisterForNavigation<GpuListPage, GpusListViewModel>();
        containerRegistry.RegisterForNavigation<GpuDetailPage, GpuDetailViewModel>();
        containerRegistry.RegisterForNavigation<GpuPhotoDetailPage, GpuPhotoDetailViewModel>();
        containerRegistry.RegisterForNavigation<ProcessorListPage, ProcessorsListViewModel>();
        containerRegistry.RegisterForNavigation<ProcessorDetailPage, ProcessorDetailViewModel>();
        containerRegistry.RegisterForNavigation<ProcessorPhotoDetailPage, ProcessorPhotoDetailViewModel>();
        containerRegistry.RegisterForNavigation<SoundSynthListPage, SoundSynthsListViewModel>();
        containerRegistry.RegisterForNavigation<SoundSynthDetailPage, SoundSynthDetailViewModel>();
        containerRegistry.RegisterForNavigation<SoundSynthPhotoDetailPage, SoundSynthPhotoDetailViewModel>();
        containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>();
        containerRegistry.RegisterForNavigation<MessagesPage, MessagesViewModel>();
        containerRegistry.RegisterForNavigation<MessageThreadPage, MessageThreadViewModel>();
        containerRegistry.RegisterForNavigation<ComposeMessagePage, ComposeMessageViewModel>();
        containerRegistry.RegisterForNavigation<UsersPage, UsersViewModel>();
        containerRegistry.RegisterForNavigation<AdminCompaniesPage, AdminCompaniesViewModel>();
        containerRegistry.RegisterForNavigation<AdminGpusPage, AdminGpusViewModel>();
        containerRegistry.RegisterForNavigation<AdminProcessorsPage, AdminProcessorsViewModel>();
        containerRegistry.RegisterForNavigation<AdminInstructionSetsPage, AdminInstructionSetsViewModel>();
        containerRegistry.RegisterForNavigation<AdminInstructionSetExtensionsPage, AdminInstructionSetExtensionsViewModel>();
        containerRegistry.RegisterForNavigation<AdminResolutionsPage, AdminResolutionsViewModel>();
        containerRegistry.RegisterForNavigation<AdminSoundSynthsPage, AdminSoundSynthsViewModel>();
        containerRegistry.RegisterForNavigation<AdminScreensPage, AdminScreensViewModel>();
        containerRegistry.RegisterForNavigation<AdminMachinesPage, AdminMachinesViewModel>();
        containerRegistry.RegisterForNavigation<AdminMachineFamiliesPage, AdminMachineFamiliesViewModel>();
        containerRegistry.RegisterForNavigation<AdminCompanyLogosPage, AdminCompanyLogosViewModel>();
        containerRegistry.RegisterForNavigation<AdminMachinePhotosPage, AdminMachinePhotosViewModel>();
        containerRegistry.RegisterForNavigation<AdminPeoplePage, AdminPeopleViewModel>();
        containerRegistry.RegisterForNavigation<AdminBooksPage, AdminBooksViewModel>();
        containerRegistry.RegisterForNavigation<AdminDocumentsPage, AdminDocumentsViewModel>();
        containerRegistry.RegisterForNavigation<AdminMagazinesPage, AdminMagazinesViewModel>();
        containerRegistry.RegisterForNavigation<SoftwarePage, SoftwareViewModel>();
        containerRegistry.RegisterForNavigation<SoftwareListPage, SoftwareListViewModel>();
        containerRegistry.RegisterForNavigation<SoftwareViewPage, SoftwareViewViewModel>();
        containerRegistry.RegisterForNavigation<SoftwareReleaseViewPage, SoftwareReleaseViewViewModel>();
        containerRegistry.RegisterForNavigation<SoftwareCompilationViewPage, SoftwareCompilationViewViewModel>();
        containerRegistry.RegisterForNavigation<SoftwareRankingsPage, SoftwareRankingsViewModel>();
        containerRegistry.RegisterForNavigation<SoftwareRankingDetailPage, SoftwareRankingDetailViewModel>();
        containerRegistry.RegisterForNavigation<ProfilePage, ProfileViewModel>();
        containerRegistry.RegisterForNavigation<AdminSoftwarePage, AdminSoftwareViewModel>();
        containerRegistry.RegisterForNavigation<AdminSoftwareFamiliesPage, AdminSoftwareFamiliesViewModel>();
        containerRegistry.RegisterForNavigation<AdminSoftwarePlatformsPage, AdminSoftwarePlatformsViewModel>();
        containerRegistry.RegisterForNavigation<AdminExternalSitesPage, AdminExternalSitesViewModel>();
        containerRegistry.RegisterForNavigation<AdminSoftwareVersionsPage, AdminSoftwareVersionsViewModel>();
        containerRegistry.RegisterForNavigation<AdminSoftwareReleasesPage, AdminSoftwareReleasesViewModel>();
        containerRegistry.RegisterForNavigation<AdminSoftwareScreenshotsPage, AdminSoftwareScreenshotsViewModel>();
        containerRegistry.RegisterForNavigation<AdminWwpcImportsPage, AdminWwpcImportsViewModel>();
        containerRegistry.RegisterForNavigation<AdminWwpcImportReviewPage, AdminWwpcImportReviewViewModel>();
        containerRegistry.RegisterForNavigation<AdminMessageReportsPage, AdminMessageReportsViewModel>();
        containerRegistry.RegisterForNavigation<AdminInvitationCodesPage, AdminInvitationCodesViewModel>();
        containerRegistry.RegisterForNavigation<AdminReviewReportsPage, AdminReviewReportsViewModel>();
        containerRegistry.RegisterForNavigation<AdminSuggestionsPage, AdminSuggestionsViewModel>();
        containerRegistry.RegisterForNavigation<AdminSuggestionReviewPage, AdminSuggestionReviewViewModel>();
        containerRegistry.RegisterForNavigation<ScreenshotDetailPage, ScreenshotDetailViewModel>();
        containerRegistry.RegisterForNavigation<CoverDetailPage, CoverDetailViewModel>();
        containerRegistry.RegisterForNavigation<SoftwarePromoArtDetailPage, SoftwarePromoArtDetailViewModel>();
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();

        // Navigate to NewsPage as the default content view
        RegionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

        // Initialize theming directly — window and content are ready
        try
        {
            var window = _mainWindow;

            if(window != null)
            {
                var themeService = Uno.Extensions.WindowExtensions.GetThemeService(window);

                if(themeService != null)
                {
                    await themeService.InitializeAsync();

                    var colorThemeService = Container.Resolve<IColorThemeService>();
                    colorThemeService.SetThemeService(themeService);
                }
            }
        }
        catch
        {
            // Theme initialization is optional
        }
    }
}
