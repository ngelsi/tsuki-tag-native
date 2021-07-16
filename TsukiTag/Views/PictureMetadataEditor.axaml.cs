using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
