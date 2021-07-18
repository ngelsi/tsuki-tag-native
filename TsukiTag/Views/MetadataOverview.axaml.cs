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

        private void FilterBoxGotFocus(object sender, GotFocusEventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                (sender as TextBox)?.SelectAll();
            });
        }

        private void FilterBoxGotPress(object sender, PointerPressedEventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                (sender as TextBox)?.SelectAll();
            });
        }

        private void TagLabelGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = (sender as TextBlock)?.Text?.ToString();
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.MetadataOverviewViewModel)?.OnTagClicked(tag);
            }
        }

        private void TagPlusGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = ((sender as TextBlock)?.DataContext as string);
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.MetadataOverviewViewModel)?.OnTagAdded(tag);
            }
        }

        private void TagMinusGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = ((sender as TextBlock)?.DataContext as string);
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.MetadataOverviewViewModel)?.OnTagRemoved(tag);
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
