using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TsukiTag.Models;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class PictureMetadataEditor : UserControl
    {
        public PictureMetadataEditor()
        {
            InitializeComponent();
        }

        public PictureMetadataEditor(Picture picture)
        {
            InitializeComponent();

            DataContext = new PictureMetadataEditorViewModel(
                picture,
                Ioc.SimpleIoc.PictureControl
            );
        }

        private void TagRemoveGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = ((sender as TextBlock)?.DataContext as string);
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.PictureMetadataEditorViewModel)?.OnTagRemoved(tag);
            }
        }

        private void OnAddTagKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (this.DataContext as TsukiTag.ViewModels.PictureMetadataEditorViewModel)?.OnTagAdded();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
