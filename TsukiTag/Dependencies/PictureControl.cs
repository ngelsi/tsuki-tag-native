using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;

namespace TsukiTag.Dependencies
{
    public interface IPictureControl
    {
        event EventHandler<Picture> PictureAdded;

        event EventHandler<Picture> PictureRemoved;

        event EventHandler PicturesReset;

        void AddPicture(Picture picture);

        void RemovePicture(Picture picture);

        void ResetPictures();

        Task<TagCollection> GetTags();
    }

    public class PictureControl : IPictureControl
    {
        private static readonly AsyncLock asyncLock = new AsyncLock();

        private List<Picture> currentPictureSet;

        private readonly IPictureDownloader pictureDownloadControl;

        public event EventHandler<Picture> PictureAdded;

        public event EventHandler<Picture> PictureRemoved;

        public event EventHandler PicturesReset;

        public PictureControl(
            IPictureDownloader pictureDownloadControl
        )
        {
            this.pictureDownloadControl = pictureDownloadControl;

            currentPictureSet = new List<Picture>();
        }

        public async void AddPicture(Picture picture)
        {
            asyncLock.Wait(async () =>
            {
                try
                {
                    await Task.Delay(50);

                    if (picture.PreviewImage == null && !string.IsNullOrEmpty(picture.PreviewUrl))
                    {
                        picture.PreviewImage = await pictureDownloadControl.DownloadBitmap(picture.PreviewUrl);
                    }

                    currentPictureSet.Add(picture);
                    PictureAdded?.Invoke(this, picture);
                }
                catch { }
            });
        }

        public async void RemovePicture(Picture picture)
        {
            await Task.Run(() =>
            {
                currentPictureSet.Remove(picture);
                PictureRemoved?.Invoke(this, picture);
            });
        }

        public async void ResetPictures()
        {
            await Task.Run(() =>
            {
                currentPictureSet = new List<Picture>();

                PicturesReset?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task<TagCollection> GetTags()
        {
            return await Task.FromResult(TagCollection.GetTags(this.currentPictureSet.ToList()));
        }
    }
}
