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
    public partial class MetadataOverview : UserControl
    {
        public MetadataOverview()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void TagGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = (sender as TextBlock)?.Text?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as MetadataOverviewViewModel)?.OnTagClicked(tag);
            }
        }

        private void OpenPressReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Middle)
            {
                RxApp.MainThreadScheduler.Schedule(async () =>
                {
                    (DataContext as MetadataOverviewViewModel)?.OnOpenPictureInBackground();
                });
            }
        }
    }
}
