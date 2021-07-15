using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class PictureListViewModel : ViewModelBase
    {
        private readonly IPictureControl pictureControl;

        public ObservableCollection<Picture> Pictures { get; set; }

        public PictureListViewModel(
            IPictureControl pictureControl
        )
        {
            this.Pictures = new ObservableCollection<Picture>();
            this.pictureControl = pictureControl;

            this.pictureControl.PictureAdded += OnPictureAdded;
            this.pictureControl.PictureRemoved += OnPictureRemoved;
            this.pictureControl.PicturesReset += OnPicturesReset;
            this.pictureControl.PictureSelected += OnImageControlPictureSelected;
            this.pictureControl.PictureDeselected += OnImageControlPictureDeselected;
        }

        public async void OnPictureOpened(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {

            });
        }


        public async void OnPictureSelected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = Pictures.FirstOrDefault(p => p.Md5 == e.Md5);
                if (picture != null)
                {
                    picture.Selected = !picture.Selected;

                    if (picture.Selected)
                    {
                        this.pictureControl.SelectPicture(picture);
                    }
                    else
                    {
                        this.pictureControl.DeselectPicture(picture);
                    }
                }

                this.RaisePropertyChanged(nameof(Pictures));
                this.RaisePropertyChanged(nameof(Picture.Selected));
            });
        }

        private void OnImageControlPictureDeselected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = Pictures.FirstOrDefault(p => p.Md5 == e.Md5);
                if (picture != null)
                {
                    picture.Selected = false;
                }

                this.RaisePropertyChanged(nameof(Pictures));
                this.RaisePropertyChanged(nameof(Picture.Selected));
            });
        }

        private void OnImageControlPictureSelected(object? sender, Picture e)
        {
        }

        private async void OnPicturesReset(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Pictures = new ObservableCollection<Picture>();
                this.RaisePropertyChanged(nameof(Pictures));
            });
        }

        private async void OnPictureRemoved(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Pictures.Remove(e);
                this.RaisePropertyChanged(nameof(Pictures));
            });
        }

        private async void OnPictureAdded(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Pictures.Add(e);
                this.RaisePropertyChanged(nameof(Pictures));
            });
        }

        ~PictureListViewModel()
        {
            this.pictureControl.PictureAdded -= OnPictureAdded;
            this.pictureControl.PictureRemoved -= OnPictureRemoved;
            this.pictureControl.PicturesReset -= OnPicturesReset;
            this.pictureControl.PictureSelected -= OnImageControlPictureSelected;
            this.pictureControl.PictureDeselected -= OnImageControlPictureDeselected;
        }
    }
}
