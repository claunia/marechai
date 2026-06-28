using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Marechai.App.Services;

namespace Marechai.App.Droid;

[Activity(
    MainLauncher = true,
    ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
)]
public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        global::AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);

        base.OnCreate(savedInstanceState);
        ProcessIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        if(intent is not null) ProcessIntent(intent);
    }

    static void ProcessIntent(Intent? intent)
    {
        if(intent?.Action != "marechai.messages.open") return;

        long conversationId = intent.GetLongExtra("conversationId", 0);
        if(conversationId > 0)
            ToastActivationService.Current?.ReportActivation(conversationId);
    }
}
