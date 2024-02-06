using Microsoft.Extensions.Logging;

namespace HidTerminal;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<INavigationService, NavigationService>()
                        .AddSingleton<IAlertService, AlertService>()
                        .AddSingleton<IFileLogService, FileLogService>()
                        .AddSingleton<IHidUsbService, HidUsbService>()

                        .AddTransient<IHidDeviceFactory, HidDeviceFactory>()

                        .AddSingleton<IAppSettingsModel, HidTerminalSettingsModel>()

                        .AddTransient<DeviceWatcherViewModel>()
                        .AddTransient<DeviceWatcherView>();

        return builder.Build();
    }
}
