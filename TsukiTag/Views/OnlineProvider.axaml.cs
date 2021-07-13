using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class OnlineProvider : UserControl
    {
        public OnlineProvider()
        {
            InitializeComponent();

            this.Initialized += OnInitialized;
        }

        private void OnInitialized(object? sender, System.EventArgs e)
        {
            if(this.DataContext is OnlineProviderViewModel vm)
            {
                vm.GetImages();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }        
    }
}
