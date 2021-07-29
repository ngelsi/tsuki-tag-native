using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {

        }

        public MainWindow(MainWindowViewModel vm)
        {
            this.DataContext = vm;
            this.Initialized += OnInitialized;
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void OnInitialized(object? sender, System.EventArgs e)
        {
            if(this.DataContext is MainWindowViewModel vm)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    vm.Initialized();
                });                
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
