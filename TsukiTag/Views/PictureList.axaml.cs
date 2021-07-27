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
        private bool doubleClickResult;
        private bool clickResult;
        private bool imageWasNotSelected;

        public PictureList()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ImageLostExamined(object sender, PointerEventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = ((sender as Image)?.DataContext as Picture);
                if (picture != null)
                {
                    if (clickResult)
                    {
                        if (imageWasNotSelected)
                        {
                            (DataContext as PictureListViewModel)?.OnPictureSelected(this, picture);
                        }
                        else
                        {
                            (DataContext as PictureListViewModel)?.OnPictureDeselected(this, picture);
                        }
                    }
                    else
                    {
                        if (imageWasNotSelected)
                        {
                            (DataContext as PictureListViewModel)?.OnPictureDeselected(this, picture);
                        }
                    }

                    (DataContext as PictureListViewModel)?.OnPictureLostExamined(this, picture);
                }
            });
        }

        private void ImageGotExamined(object sender, PointerEventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                clickResult = false;
                doubleClickResult = false;
                imageWasNotSelected = false;

                var picture = ((sender as Image)?.DataContext as Picture);
                if (picture != null)
                {
                    imageWasNotSelected = !picture.Selected;

                    (DataContext as PictureListViewModel)?.OnPictureGotExamined(this, picture);
                    (DataContext as PictureListViewModel)?.OnPictureSelected(this, picture);
                }
            });
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

        private void ImageGotPress(object sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed &&
                !e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                var picture = ((sender as Image)?.DataContext as Picture);
                if (picture != null)
                {
                    if (!clickResult)
                    {
                        clickResult = true;
                    }
                    else
                    {
                        clickResult = false;
                        doubleClickResult = true;

                        RxApp.MainThreadScheduler.Schedule(async () =>
                        {
                            (DataContext as PictureListViewModel)?.OnPictureOpened(this, picture);
                        });
                    }

                    e.Handled = true;
                }
            }
        }
    }
}
