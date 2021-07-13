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
        private static readonly AsyncLock asyncLock = new AsyncLock();

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
        }
    }
}
