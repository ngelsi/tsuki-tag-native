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

        event EventHandler<Picture> PictureSelected;
        
        event EventHandler<Picture> PictureDeselected;

        event EventHandler<Picture> PictureOpened;

        event EventHandler<Picture> PictureClosed;
        
        event EventHandler<Picture> PictureOpenedInBackground;

        event EventHandler PicturesReset;

        void AddPicture(Picture picture);

        void RemovePicture(Picture picture);

        void ResetPictures();

        void SelectPicture(Picture picture);

        void DeselectPicture(Picture picture);

        void OpenPicture(Picture picture);

        void OpenPictureInBackground(Picture picture);

        void ClosePicture(Picture picture);

        Task<TagCollection> GetTags();

        Task<int> GetSelectedPictureCount();
    }

    public class PictureControl : IPictureControl
    {
        private static readonly AsyncLock asyncLock = new AsyncLock();

        private List<Picture> currentPictureSet;

        private List<Picture> selectedPictures;

        private List<Picture> openedPictures;

        private readonly IPictureDownloader pictureDownloadControl;

        public event EventHandler<Picture> PictureSelected;

        public event EventHandler<Picture> PictureDeselected;

        public event EventHandler<Picture> PictureAdded;

        public event EventHandler<Picture> PictureRemoved;
        
        public event EventHandler<Picture> PictureOpened;

        public event EventHandler<Picture> PictureOpenedInBackground;
        
        public event EventHandler<Picture> PictureClosed;

        public event EventHandler PicturesReset;

        public PictureControl(
            IPictureDownloader pictureDownloadControl
        )
        {
            this.pictureDownloadControl = pictureDownloadControl;

            currentPictureSet = new List<Picture>();
            selectedPictures = new List<Picture>();
            openedPictures = new List<Picture>();
        }

        public async void SelectPicture(Picture picture)
        {
            asyncLock.Wait(async () =>
            {
                try
                {
                    if(!selectedPictures.Any(p => p.Md5 == picture.Md5))
                    {
                        picture.Selected = true;
                        selectedPictures.Add(picture);

                        PictureSelected?.Invoke(this, picture);
                    }                    
                    else
                    {
                        PictureSelected?.Invoke(this, picture);
                    }
                }
                catch { }
            });
        }

        public async void DeselectPicture(Picture picture)
        {
            asyncLock.Wait(async () =>
            {
                try
                {                    
                    picture.Selected = false;
                    selectedPictures.Remove(picture);

                    PictureDeselected?.Invoke(this, picture);
                }
                catch { }
            });
        }

        public async void OpenPicture(Picture picture)
        {
            asyncLock.Wait(async () =>
            {
                try
                {
                    if (picture.SampleImage == null && !string.IsNullOrEmpty(picture.Url))
                    {
                        picture.SampleImage = await pictureDownloadControl.DownloadBitmap(picture.Url);
                    }                    

                    openedPictures.Add(picture);
                    PictureOpened?.Invoke(this, picture);
                }
                catch { }
            });
        }

        public async void OpenPictureInBackground(Picture picture)
        {
            asyncLock.Wait(async () =>
            {
                try
                {
                    if (picture.SampleImage == null && !string.IsNullOrEmpty(picture.Url))
                    {
                        picture.SampleImage = await pictureDownloadControl.DownloadBitmap(picture.Url);
                    }

                    openedPictures.Add(picture);
                    PictureOpenedInBackground?.Invoke(this, picture);
                }
                catch { }
            });
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

                    picture.Selected = selectedPictures.Contains(picture);

                    currentPictureSet.Add(picture);
                    PictureAdded?.Invoke(this, picture);
                }
                catch { }
            });
        }

        public async void ClosePicture(Picture picture)
        {
            await Task.Run(() =>
            {
                openedPictures.Remove(picture);
                PictureClosed?.Invoke(this, picture);
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

        public async Task<int> GetSelectedPictureCount()
        {
            return await Task.FromResult(selectedPictures.Count);
        }
    }
}
