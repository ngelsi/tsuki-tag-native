using Avalonia;
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
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(
                        Ioc.SimpleIoc.NavigationControl                        
                    )
                };
            }

            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(Console.WriteLine);
            base.OnFrameworkInitializationCompleted();
        }
    }
}
