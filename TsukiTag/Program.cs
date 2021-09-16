using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Serilog;
using System;
using System.IO;

namespace TsukiTag
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .AfterSetup(AfterSetupCallback)                     
                .UsePlatformDetect()
                .LogToTrace(Avalonia.Logging.LogEventLevel.Error)                
                .UseReactiveUI();

        private static void AfterSetupCallback(AppBuilder appBuilder)
        {
            SetUpFontAwesome();
            SetUpLogging();
        }

        private static void SetUpFontAwesome()
        {
            IconProvider.Register<FontAwesomeIconProvider>();
        }

        private static void SetUpLogging()
        {
            var logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TsukiTag", "logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            Log.Logger = new LoggerConfiguration()                        
                        .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                        .WriteTo.File(Path.Combine(logPath, "log-.txt"), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: (1024*1024), rollOnFileSizeLimit: true, retainedFileCountLimit: 5, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}{Properties:j}")
                        .CreateLogger();                              
        }
    }
}
