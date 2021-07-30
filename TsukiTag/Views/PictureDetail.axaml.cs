using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Reactive.Concurrency;
using TsukiTag.Models;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class PictureDetail : UserControl
    {
        public PictureDetail()
        {
            InitializeComponent();
        }

        public PictureDetail(Picture picture)
        {
            InitializeComponent();

            DataContext = new PictureDetailViewModel(
                picture,
                Ioc.SimpleIoc.PictureControl,
                Ioc.SimpleIoc.DbRepository,
                Ioc.SimpleIoc.NotificationControl
            );
        }

        ~PictureDetail()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                (DataContext as PictureDetailViewModel)?.OnInternalClose();
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
