using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Serilog;
using System;
using System.Reactive;
using TsukiTag.ViewModels;
using TsukiTag.Views;

namespace TsukiTag
{
    public class App : Application
    {
        public static Window MainWindow;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow(new MainWindowViewModel(
                        Ioc.SimpleIoc.NavigationControl,
                        Ioc.SimpleIoc.DbRepository,
                        Ioc.SimpleIoc.PictureWorker,
                        Ioc.SimpleIoc.NotificationControl
                ));

                MainWindow = desktop.MainWindow;
            }

            RxApp.DefaultExceptionHandler = Observer.Create<Exception>((ex) =>
            {
                Log.Error(ex, "General unhandled exception catched");
            });

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            base.OnFrameworkInitializationCompleted();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.ExceptionObject as Exception, "General unhandled exception catched");            
        }
    }
}
