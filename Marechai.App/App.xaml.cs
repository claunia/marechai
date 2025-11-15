using System.Net.Http;
using Marechai.App.Services;
using Microsoft.UI.Xaml;
using Uno.Extensions;
using Uno.Extensions.Configuration;
using Uno.Extensions.Hosting;
using Uno.Extensions.Http;
using Uno.Extensions.Localization;
using Uno.Extensions.Navigation;
using Uno.UI;

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
    protected IHost?  Host       { get; private set; }

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
                                                                 services.AddSingleton<NewsService>();
                                                                 services.AddSingleton<NewsViewModel>();
                                                                 services.AddSingleton<ComputersService>();
                                                                 services.AddSingleton<ComputersViewModel>();
                                                                 services.AddSingleton<MachineViewViewModel>();

                                                                 services
                                                                    .AddSingleton<IComputersListFilterContext,
                                                                         ComputersListFilterContext>();

                                                                 services.AddTransient<ComputersListViewModel>();
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
                       new ViewMap<MachineViewPage, MachineViewViewModel>(),
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
                                                                       Nested:
                                                                       [
                                                                           new RouteMap("list",
                                                                               views.FindByViewModel<
                                                                                   ComputersListViewModel>()),
                                                                           new RouteMap("view",
                                                                               views.FindByViewModel<
                                                                                   MachineViewViewModel>())
                                                                       ]),
                                                          new RouteMap("Second",
                                                                       views.FindByViewModel<SecondViewModel>())
                                                      ])
                                     ]));
    }
}