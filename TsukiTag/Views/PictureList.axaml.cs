using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Reactive.Concurrency;
using TsukiTag.Models;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class PictureList : UserControl
    {
        private System.Timers.Timer clickTimer;
        private Picture clickedPicture;
        private bool waitingDoubleClick;

        public PictureList()
        {
            InitializeComponent();

            clickTimer = new System.Timers.Timer(200);
            clickTimer.Elapsed += OnClickElapsed;
        }

        private void OnClickElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if(clickedPicture != null)
                {
                    (DataContext as PictureListViewModel)?.OnPictureSelected(this, clickedPicture);
                }                

                waitingDoubleClick = false;
                clickedPicture = null;
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ImagePressReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Middle)
            {
                var picture = ((sender as Image)?.DataContext as Picture);
                if (picture != null)
                {
                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        (DataContext as PictureListViewModel)?.OnPictureOpenedInBackground(this, picture);
                    });
                }
            }
        }

        private void ImageGotTap(object sender, RoutedEventArgs e)
        {
            var picture = ((sender as Image)?.DataContext as Picture);
            if (picture != null)
            {
                if (waitingDoubleClick)
                {
                    waitingDoubleClick = false;
                    clickTimer.Stop();

                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        if (clickedPicture != null)
                        {
                            (DataContext as PictureListViewModel)?.OnPictureOpened(this, clickedPicture);
                        }

                        clickedPicture = null;
                    });
                }
                else
                {
                    waitingDoubleClick = true;
                    clickedPicture = picture;

                    clickTimer.Stop();
                    clickTimer.Start();
                }

                e.Handled = true;
            }
        }
    }
}
