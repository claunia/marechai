using System.Net.Http;
using Marechai.App.Presentation.ViewModels;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml;
using Uno.Extensions;
using Uno.Extensions.Configuration;
using Uno.Extensions.Hosting;
using Uno.Extensions.Http;
using Uno.Extensions.Localization;
using Uno.Extensions.Navigation;
using Uno.UI;
using CompanyDetailViewModel = Marechai.App.Presentation.ViewModels.CompanyDetailViewModel;
using ComputersListViewModel = Marechai.App.Presentation.ViewModels.ComputersListViewModel;
using ComputersViewModel = Marechai.App.Presentation.ViewModels.ComputersViewModel;
using GpuDetailViewModel = Marechai.App.Presentation.ViewModels.GpuDetailViewModel;
using GpuListViewModel = Marechai.App.Presentation.ViewModels.GpusListViewModel;
using MachineViewViewModel = Marechai.App.Presentation.ViewModels.MachineViewViewModel;
using MainViewModel = Marechai.App.Presentation.ViewModels.MainViewModel;
using NewsViewModel = Marechai.App.Presentation.ViewModels.NewsViewModel;
using PhotoDetailViewModel = Marechai.App.Presentation.ViewModels.PhotoDetailViewModel;
using ProcessorDetailViewModel = Marechai.App.Presentation.ViewModels.ProcessorDetailViewModel;
using ProcessorsListViewModel = Marechai.App.Presentation.ViewModels.ProcessorsListViewModel;
using SettingsViewModel = Marechai.App.Presentation.ViewModels.SettingsViewModel;
using SoundSynthDetailViewModel = Marechai.App.Presentation.ViewModels.SoundSynthDetailViewModel;
using SoundSynthsListViewModel = Marechai.App.Presentation.ViewModels.SoundSynthsListViewModel;

namespace Marechai.App;

public partial class App : Application
{
    /// <summary>
    ///     Initializes the singleton application object. This is the first line of authored code
    ///     executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    public    IHost?  Host       { get; private set; }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        IApplicationBuilder builder = this.CreateBuilder(args)

                                           // Add navigation support for toolkit controls such as TabBar and NavigationView
                                          .UseToolkitNavigation()
                                          .Configure(host => host
#if DEBUG

                                                             // Switch to Development environment when running in DEBUG
                                                            .UseEnvironment(Environments.Development)
#endif
                                                            .UseLogging((context, logBuilder) =>
                                                                        {
                                                                            // Configure log levels for different categories of logging
                                                                            logBuilder
                                                                               .SetMinimumLevel(context
                                                                                   .HostingEnvironment
                                                                                   .IsDevelopment()
                                                                                    ? LogLevel.Information
                                                                                    : LogLevel.Warning)

                                                                                // Default filters for core Uno Platform namespaces
                                                                               .CoreLogLevel(LogLevel.Warning);

                                                                            // Uno Platform namespace filter groups
                                                                            // Uncomment individual methods to see more detailed logging
                                                                            //// Generic Xaml events
                                                                            //logBuilder.XamlLogLevel(LogLevel.Debug);
                                                                            //// Layout specific messages
                                                                            //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                                                                            //// Storage messages
                                                                            //logBuilder.StorageLogLevel(LogLevel.Debug);
                                                                            //// Binding related messages
                                                                            //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                                                                            //// Binder memory references tracking
                                                                            //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                                                                            //// DevServer and HotReload related
                                                                            //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                                                                            //// Debug JS interop
                                                                            //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);
                                                                        },
                                                                        true)
                                                            .UseSerilog(true, true)
                                                            .UseConfiguration(configure: configBuilder =>
                                                                                  configBuilder.EmbeddedSource<App>()
                                                                                     .Section<AppConfig>())

                                                             // Enable localization (see appsettings.json for supported languages)
                                                            .UseLocalization()
                                                            .UseHttp((context, services) =>
                                                             {
#if DEBUG

                                                                 // DelegatingHandler will be automatically injected
                                                                 services
                                                                    .AddTransient<DelegatingHandler,
                                                                         DebugHttpHandler>();
#endif
                                                                 services.AddKiotaClientV2<ApiClient>(context,
                                                                     new EndpointOptions
                                                                     {
                                                                         Url = context.Configuration
                                                                                  .GetSection("ApiClient:Url")
                                                                                  .Value ??

                                                                               // Fallback to a default URL if not configured
                                                                               "https://localhost:5023"
                                                                     });
                                                             })
                                                            .ConfigureServices((context, services) =>
                                                             {
                                                                 // Register application services
                                                                 services
                                                                    .AddSingleton<IColorThemeService,
                                                                         ColorThemeService>();

                                                                 services.AddSingleton<FlagCache>();
                                                                 services.AddSingleton<CompanyLogoCache>();
                                                                 services.AddSingleton<MachinePhotoCache>();
                                                                 services.AddSingleton<NewsService>();
                                                                 services.AddSingleton<NewsViewModel>();
                                                                 services.AddSingleton<ComputersService>();
                                                                 services.AddSingleton<ComputersViewModel>();
                                                                 services.AddSingleton<ConsolesService>();
                                                                 services.AddSingleton<ConsolesViewModel>();
                                                                 services.AddSingleton<CompaniesService>();
                                                                 services.AddSingleton<CompaniesViewModel>();
                                                                 services.AddSingleton<CompanyDetailService>();
                                                                 services.AddSingleton<CompanyDetailViewModel>();
                                                                 services.AddSingleton<MachineViewViewModel>();
                                                                 services.AddSingleton<GpusService>();
                                                                 services.AddSingleton<ProcessorsService>();
                                                                 services.AddSingleton<SoundSynthsService>();
                                                                 services.AddTransient<PhotoDetailViewModel>();

                                                                 services
                                                                    .AddSingleton<IComputersListFilterContext,
                                                                         ComputersListFilterContext>();

                                                                 services
                                                                    .AddSingleton<IConsolesListFilterContext,
                                                                         ConsolesListFilterContext>();

                                                                 services.AddTransient<ComputersListViewModel>();
                                                                 services.AddTransient<ConsolesListViewModel>();
                                                                 services.AddTransient<GpuListViewModel>();
                                                                 services.AddTransient<GpuDetailViewModel>();
                                                                 services.AddTransient<ProcessorsListViewModel>();
                                                                 services.AddTransient<ProcessorDetailViewModel>();
                                                                 services.AddTransient<SoundSynthsListViewModel>();
                                                                 services.AddTransient<SoundSynthDetailViewModel>();
                                                                 services.AddTransient<SettingsViewModel>();
                                                             })
                                                            .UseNavigation(RegisterRoutes));

        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(new ViewMap(ViewModel: typeof(ShellViewModel)),
                       new ViewMap<MainPage, MainViewModel>(),
                       new ViewMap<NewsPage, NewsViewModel>(),
                       new ViewMap<ComputersPage, ComputersViewModel>(),
                       new ViewMap<ComputersListPage, ComputersListViewModel>(),
                       new ViewMap<ConsolesPage, ConsolesViewModel>(),
                       new ViewMap<ConsolesListPage, ConsolesListViewModel>(),
                       new ViewMap<CompaniesPage, CompaniesViewModel>(),
                       new ViewMap<CompanyDetailPage, CompanyDetailViewModel>(),
                       new ViewMap<MachineViewPage, MachineViewViewModel>(),
                       new ViewMap<PhotoDetailPage, PhotoDetailViewModel>(),
                       new ViewMap<GpuListPage, GpuListViewModel>(),
                       new ViewMap<GpuDetailPage, GpuDetailViewModel>(),
                       new ViewMap<ProcessorListPage, ProcessorsListViewModel>(),
                       new ViewMap<ProcessorDetailPage, ProcessorDetailViewModel>(),
                       new ViewMap<SoundSynthListPage, SoundSynthsListViewModel>(),
                       new ViewMap<SoundSynthDetailPage, SoundSynthDetailViewModel>(),
                       new ViewMap<SettingsPage, SettingsViewModel>(),
                       new DataViewMap<SecondPage, SecondViewModel, Entity>());

        routes.Register(new RouteMap("",
                                     views.FindByViewModel<ShellViewModel>(),
                                     Nested:
                                     [
                                         new RouteMap("Main",
                                                      views.FindByViewModel<MainViewModel>(),
                                                      true,
                                                      Nested:
                                                      [
                                                          new RouteMap("News",
                                                                       views.FindByViewModel<NewsViewModel>(),
                                                                       true),
                                                          new RouteMap("computers",
                                                                       views.FindByViewModel<ComputersViewModel>(),
                                                                       Nested
                                                                       :
                                                                       [
                                                                           new RouteMap("list-computers",
                                                                               views.FindByViewModel<
                                                                                   ComputersListViewModel>()),
                                                                           new RouteMap("view",
                                                                               views.FindByViewModel<
                                                                                   MachineViewViewModel>())
                                                                       ]),
                                                          new RouteMap("consoles",
                                                                       views.FindByViewModel<ConsolesViewModel>(),
                                                                       Nested
                                                                       :
                                                                       [
                                                                           new RouteMap("list-consoles",
                                                                               views.FindByViewModel<
                                                                                   ConsolesListViewModel>())
                                                                       ]),
                                                          new RouteMap("companies",
                                                                       views.FindByViewModel<CompaniesViewModel>(),
                                                                       Nested
                                                                       :
                                                                       [
                                                                           new RouteMap("company-details",
                                                                               views.FindByViewModel<
                                                                                   CompanyDetailViewModel>())
                                                                       ]),
                                                          new RouteMap("gpus",
                                                                       views.FindByViewModel<GpuListViewModel>(),
                                                                       Nested:
                                                                       [
                                                                           new RouteMap("gpu-details",
                                                                               views.FindByViewModel<
                                                                                   GpuDetailViewModel>())
                                                                       ]),
                                                          new RouteMap("processors",
                                                                       views.FindByViewModel<ProcessorsListViewModel>(),
                                                                       Nested:
                                                                       [
                                                                           new RouteMap("processor-details",
                                                                               views.FindByViewModel<
                                                                                   ProcessorDetailViewModel>())
                                                                       ]),
                                                          new RouteMap("sound-synths",
                                                                       views.FindByViewModel<
                                                                           SoundSynthsListViewModel>(),
                                                                       Nested:
                                                                       [
                                                                           new RouteMap("sound-synth-details",
                                                                               views.FindByViewModel<
                                                                                   SoundSynthDetailViewModel>()),
                                                                           new RouteMap("machine-view",
                                                                               views.FindByViewModel<
                                                                                   MachineViewViewModel>())
                                                                       ]),
                                                          new RouteMap("settings",
                                                                       views.FindByViewModel<SettingsViewModel>()),
                                                          new RouteMap("Second",
                                                                       views.FindByViewModel<SecondViewModel>())
                                                      ])
                                     ]));
    }
}