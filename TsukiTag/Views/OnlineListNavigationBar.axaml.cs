using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsukiTag.Views
{
    public partial class OnlineListNavigationBar : UserControl
    {
        public OnlineListNavigationBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
