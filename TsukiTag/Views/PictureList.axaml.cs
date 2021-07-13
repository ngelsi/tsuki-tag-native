using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsukiTag.Views
{
    public partial class PictureList : UserControl
    {
        public PictureList()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
