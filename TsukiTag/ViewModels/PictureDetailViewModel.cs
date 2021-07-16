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
    public class PictureDetailViewModel
    {
        private Picture picture;
        private readonly IPictureControl pictureControl;

        public Picture Picture
        {
            get { return picture; }
            set { picture = value; }
        }

        public ReactiveCommand<Unit, Unit> ClosePictureCommand { get; set; }

        public PictureDetailViewModel(
            Picture picture,
            IPictureControl pictureControl
        )
        {
            this.ClosePictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnClosePicture();
            });

            this.pictureControl = pictureControl;
            this.picture = picture;
        }

        ~PictureDetailViewModel()
        {

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
