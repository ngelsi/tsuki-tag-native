using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsukiTag.Views
{
    public partial class TagBar : UserControl
    {
        public TagBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
