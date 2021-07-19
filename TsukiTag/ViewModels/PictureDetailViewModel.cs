using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;
using System.Reactive.Concurrency;
using System.Reactive;
using ReactiveUI;


namespace TsukiTag.ViewModels
{
    public class PictureDetailViewModel : ViewModelBase
    {
        private Picture picture;
        private readonly IPictureControl pictureControl;
        private bool maximizedView;
        private bool fillView;

        public Picture Picture
        {
            get { return picture; }
            set { picture = value; }
        }

        public bool MaximizedView
        {
            get { return maximizedView; }
            set
            {
                maximizedView = value;
                fillView = !value;

                this.RaisePropertyChanged(nameof(MaximizedView));
                this.RaisePropertyChanged(nameof(FillView));
            }
        }

        public bool FillView
        {
            get { return fillView; }
            set
            {
                fillView = value;
                maximizedView = !value;

                this.RaisePropertyChanged(nameof(MaximizedView));
                this.RaisePropertyChanged(nameof(FillView));
            }
        }

        public ReactiveCommand<Unit, Unit> ClosePictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SwitchDisplayModeCommand { get; set; }

        public PictureDetailViewModel(
            Picture picture,
            IPictureControl pictureControl
        )
        {
            this.ClosePictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnClosePicture();
            });

            this.SwitchDisplayModeCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnSwitchDisplay();
            });

            this.fillView = true;
            this.pictureControl = pictureControl;
            this.picture = picture;
        }

        ~PictureDetailViewModel()
        {

        }

        public async void OnSwitchDisplay()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                MaximizedView = !maximizedView;   
            });
        }

        public async void OnClosePicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (Picture != null)
                {
                    this.pictureControl.ClosePicture(picture);
                }
            });
        }
    }
}
