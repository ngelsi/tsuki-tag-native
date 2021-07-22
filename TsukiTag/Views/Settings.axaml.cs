using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();

            this.Initialized += OnInitialized;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnInitialized(object? sender, System.EventArgs e)
        {
            if (this.DataContext is SettingsViewModel vm)
            {
                vm.Initialize();
            }
        }
    }
}
