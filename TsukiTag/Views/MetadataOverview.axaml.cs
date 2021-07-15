using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

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

        private void PicturePreviousGotPress(object sender, PointerPressedEventArgs e)
        {
            (this.DataContext as TsukiTag.ViewModels.MetadataOverviewViewModel)?.OnPreviousPicture();
        }

        private void PictureNextGotPress(object sender, PointerPressedEventArgs e)
        {
            (this.DataContext as TsukiTag.ViewModels.MetadataOverviewViewModel)?.OnNextPicture();
        }

        private void PictureDeselectGotPress(object sender, PointerPressedEventArgs e)
        {
            (this.DataContext as TsukiTag.ViewModels.MetadataOverviewViewModel)?.OnDeselectPicture();
        }

        private void TagGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = (sender as TextBlock)?.Text?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.MetadataOverviewViewModel)?.OnTagClicked(tag);
            }
        }
    }
}
