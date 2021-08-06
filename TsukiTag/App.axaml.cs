using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
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

            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(Console.WriteLine);
            base.OnFrameworkInitializationCompleted();
        }
    }
}
