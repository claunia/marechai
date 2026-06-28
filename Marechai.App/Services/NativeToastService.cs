#nullable enable

using System;
using System.Diagnostics;
using System.Threading.Tasks;

#if __IOS__
using Foundation;
using UserNotifications;
#endif
#if __ANDROID__
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Application = Android.App.Application;
#endif

namespace Marechai.App.Services;

public sealed class NativeToastService(ToastActivationService activationService) : INativeToastService
{
    const string AndroidChannelId = "messages";
    bool _initialized;

    public Task InitializeAsync()
    {
        if(_initialized) return Task.CompletedTask;
        _initialized = true;

#if __IOS__
        UNUserNotificationCenter.Current.Delegate = new NativeToastNotificationCenterDelegate(activationService);
#endif
#if __ANDROID__
        if(Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(AndroidChannelId, "Messages", NotificationImportance.Default)
            {
                Description = "Marechai message notifications"
            };

            NotificationManager? notificationManager =
                Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.CreateNotificationChannel(channel);
        }
#endif

        return Task.CompletedTask;
    }

    public async Task<bool> EnsurePermissionAsync()
    {
        await InitializeAsync();

#if __IOS__
        (bool approved, _) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Badge);
        return approved;
#endif
#if __ANDROID__
        if(Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu) return true;
        return ContextCompat.CheckSelfPermission(Application.Context, Android.Manifest.Permission.PostNotifications) ==
               Android.Content.PM.Permission.Granted;
#else
        return true;
#endif
    }

    public async Task ShowAsync(ToastMessage toastMessage)
    {
        await InitializeAsync();

#if __IOS__
        var content = new UNMutableNotificationContent
        {
            Title = toastMessage.Title,
            Body  = toastMessage.Body,
            UserInfo = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
                [NSNumber.FromInt64(toastMessage.ConversationId)],
                [new NSString("conversationId")])
        };

        var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);
        string identifier = $"message-{toastMessage.ConversationId}-{toastMessage.MessageId ?? 0}";
        var request = UNNotificationRequest.FromIdentifier(identifier, content, trigger);
        await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
#elif __ANDROID__
        Context context = Application.Context;
        var intent = new Intent(context, typeof(Marechai.App.Droid.MainActivity));
        intent.SetAction("marechai.messages.open");
        intent.PutExtra("conversationId", toastMessage.ConversationId);
        intent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop | ActivityFlags.NewTask);

        PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
        if(Build.VERSION.SdkInt >= BuildVersionCodes.M) flags |= PendingIntentFlags.Immutable;

        PendingIntent pendingIntent =
            PendingIntent.GetActivity(context, (int)(toastMessage.ConversationId % int.MaxValue), intent, flags)!;

        var builder = new NotificationCompat.Builder(context, AndroidChannelId)
                     .SetContentTitle(toastMessage.Title)
                     .SetContentText(toastMessage.Body)
                     .SetStyle(new NotificationCompat.BigTextStyle().BigText(toastMessage.Body))
                     .SetSmallIcon(Resource.Mipmap.icon)
                     .SetAutoCancel(true)
                     .SetContentIntent(pendingIntent)
                     .SetPriority((int)NotificationPriority.Default);

        NotificationManagerCompat.From(context)
                                 .Notify(Math.Abs(HashCode.Combine(toastMessage.ConversationId, toastMessage.MessageId)), builder.Build());
#else
        await ShowDesktopNotificationAsync(toastMessage);
#endif
    }

#if __IOS__
    sealed class NativeToastNotificationCenterDelegate(ToastActivationService activationService)
        : UNUserNotificationCenterDelegate
    {
        public override void DidReceiveNotificationResponse(UNUserNotificationCenter center,
            UNNotificationResponse response, Action completionHandler)
        {
            NSObject? conversationValue = response.Notification.Request.Content.UserInfo?["conversationId"];

            if(conversationValue is NSNumber number)
                activationService.ReportActivation(number.Int64Value);

            completionHandler();
        }
    }
#endif

    static Task ShowDesktopNotificationAsync(ToastMessage toastMessage)
    {
        try
        {
            if(OperatingSystem.IsMacOS())
            {
                // TODO: Replace shell-based notification dispatch with native desktop APIs when the shared
                // desktop head gains a direct macOS notification bridge.
                string title = EscapeShellArg(toastMessage.Title);
                string body  = EscapeShellArg(toastMessage.Body);
                string command = $"osascript -e 'display notification {body} with title {title}'";
                StartShellCommand(command);
            }
            else if(OperatingSystem.IsLinux())
            {
                // TODO: Replace shell-based notification dispatch with direct freedesktop D-Bus calls.
                string title = EscapeShellArg(toastMessage.Title);
                string body  = EscapeShellArg(toastMessage.Body);
                StartShellCommand($"notify-send {title} {body}");
            }
            else if(OperatingSystem.IsWindows())
            {
                // TODO: Replace shell-based notification dispatch with direct Windows app notification APIs
                // from the shared desktop head once the runtime-only bridge is in place here.
                string title = EscapePowerShell(toastMessage.Title);
                string body  = EscapePowerShell(toastMessage.Body);
                string script =
                    "$null=[Windows.UI.Notifications.ToastNotificationManager,Windows.UI.Notifications,ContentType=WindowsRuntime];" +
                    "$null=[Windows.Data.Xml.Dom.XmlDocument,Windows.Data.Xml.Dom.XmlDocument,ContentType=WindowsRuntime];" +
                    "$xml=New-Object Windows.Data.Xml.Dom.XmlDocument;" +
                    $"$xml.LoadXml(\"<toast><visual><binding template='ToastGeneric'><text>{title}</text><text>{body}</text></binding></visual></toast>\");" +
                    "$toast=[Windows.UI.Notifications.ToastNotification]::new($xml);" +
                    "[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('Marechai').Show($toast);";

                Process.Start(new ProcessStartInfo("powershell", $"-NoProfile -Command \"{script}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow  = true
                });
            }
        }
        catch
        {
            // Swallow desktop notification dispatch errors to avoid destabilizing the app.
        }

        return Task.CompletedTask;
    }

    static void StartShellCommand(string command)
    {
        Process.Start(new ProcessStartInfo("bash", $"-lc \"{command}\"")
        {
            UseShellExecute = false,
            CreateNoWindow  = true
        });
    }

    static string EscapeShellArg(string input) => $"'{input.Replace("'", "'\\''")}'";
    static string EscapePowerShell(string input) => input.Replace("`", "``").Replace("\"", "`\"");
}
