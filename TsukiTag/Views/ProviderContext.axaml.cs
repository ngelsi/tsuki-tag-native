using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Reactive.Concurrency;
using TsukiTag.Models;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class ProviderContext : UserControl
    {
        public ProviderContext()
        {
            InitializeComponent();

            this.Initialized += OnInitialized;
        }

        private void OnInitialized(object? sender, System.EventArgs e)
        {
            if(this.DataContext is ProviderContextViewModel vm)
            {
                vm.Initialize();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void TabGotPointerRelease(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Middle)
            {
                var picture = (((sender as TextBlock)?.DataContext as ProviderTabModel)?.Context as Picture);
                if(picture != null)
                {
                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        (DataContext as ProviderContextViewModel)?.OnTabPictureClosed(picture);
                    });
                }
            }
        }        
    }
}
