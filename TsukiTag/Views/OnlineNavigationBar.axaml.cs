using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TsukiTag.Views
{
    public partial class OnlineNavigationBar : UserControl
    {
        public OnlineNavigationBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
