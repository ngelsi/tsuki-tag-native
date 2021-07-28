using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsukiTag.Views
{
    public partial class OnlineListBrowser : UserControl
    {
        public OnlineListBrowser()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
