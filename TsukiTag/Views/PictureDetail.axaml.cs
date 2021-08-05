using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.Reactive.Concurrency;
using TsukiTag.Models;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class PictureDetail : UserControl
    {
        private bool scrolling;
        private Point? previousPosition;
        private ScrollViewer scrollViewer;

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
                Ioc.SimpleIoc.NotificationControl,
                Ioc.SimpleIoc.PictureWorker,
                Ioc.SimpleIoc.PictureProviderContext
            );

        }

        ~PictureDetail()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                (DataContext as PictureDetailViewModel)?.OnInternalClose();
            });
        }

        private void ImagePointerPressed(object sender, PointerPressedEventArgs e)
        {
            scrolling = true;
        }

        private void ImagePointerReleased(object sender, PointerReleasedEventArgs e)
        {
            scrolling = false;
            previousPosition = null;
        }

        private void ImagePointerMove(object sender, PointerEventArgs e)
        {
            if(scrollViewer == null)
            {
                scrollViewer = this.FindControl<ScrollViewer>("PictureScroller");
            }

            if (scrolling)
            {
                var position = e.GetPosition(e.Source as IVisual);
                if (previousPosition == null)
                {
                    previousPosition = position;
                    return;
                }
                else
                {
                    if (Math.Abs(position.X - previousPosition.Value.X) < 19 && Math.Abs(position.Y - previousPosition.Value.Y) < 19)
                    {
                        return;
                    }
                    
                    if (position.X >= previousPosition.Value.X && Math.Abs(position.X - previousPosition.Value.X) > 19)
                    {
                        scrollViewer.LineLeft();
                    }
                    else if (position.X < previousPosition.Value.X && Math.Abs(position.X - previousPosition.Value.X) > 19)
                    {
                        scrollViewer.LineRight();
                    }

                    if (position.Y >= previousPosition.Value.Y && Math.Abs(position.Y - previousPosition.Value.Y) > 19)
                    {
                        scrollViewer.LineUp();
                    }
                    else if (position.Y < previousPosition.Value.Y && Math.Abs(position.Y - previousPosition.Value.Y) > 19)
                    {
                        scrollViewer.LineDown();
                    }

                    previousPosition = position;
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
