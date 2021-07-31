using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
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

        void SelectAllPictures();

        void DeselectPicture(Picture picture);

        void OpenPicture(Picture picture);

        void OpenPictureInBackground(Picture picture);

        void ClosePicture(Picture picture);

        Task<TagCollection> GetTags();

        Task<int> GetSelectedPictureCount();

        Task<bool> PictureExists(string md5);
    }

    public class PictureControl : IPictureControl
    {
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private List<Picture> currentPictureSet;

        private List<Picture> selectedPictures;

        private List<Picture> openedPictures;

        private HashSet<string> seenPictures;

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
            seenPictures = new HashSet<string>();
        }

        public async void SelectPicture(Picture picture)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (!selectedPictures.Any(p => p.Md5 == picture.Md5))
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
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async void SelectAllPictures()
        {
            for (var i = this.currentPictureSet.Count - 1; i >= 0; i--)
            {
                var picture = this.currentPictureSet.ElementAtOrDefault(i);
                if (picture != null)
                {
                    SelectPicture(picture);
                }
            }
        }

        public async Task<bool> PictureExists(string md5)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                return await Task.FromResult(seenPictures.Contains(md5) || currentPictureSet.Any(p => p.Md5 == md5));
            }
            catch { }
            finally
            {
                semaphoreSlim.Release();
            }

            return await Task.FromResult(false);
        }

        public async void DeselectPicture(Picture picture)
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                if (picture.SourceImage != null)
                {
                    picture.SourceImage.Dispose();
                    picture.SourceImage = null;
                }

                picture.Selected = false;
                selectedPictures.Remove(picture);

                PictureDeselected?.Invoke(this, picture);
            }
            catch { }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async void OpenPicture(Picture picture)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(picture.FileUrl))
                {
                    picture.SampleImage = await pictureDownloadControl.DownloadLocalBitmap(picture.FileUrl);
                }

                if (picture.SampleImage == null && !string.IsNullOrEmpty(picture.Url))
                {
                    picture.SampleImage = await pictureDownloadControl.DownloadBitmap(picture.Url);
                }

                openedPictures.Add(picture);
                PictureOpened?.Invoke(this, picture);
            }
            catch { }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async void OpenPictureInBackground(Picture picture)
        {
            await semaphoreSlim.WaitAsync();
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
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async void AddPicture(Picture picture)
        {
            try
            {
                if (!(await PictureExists(picture.Md5)))
                {
                    if (picture.PreviewImage == null && !string.IsNullOrEmpty(picture.PreviewUrl))
                    {
                        picture.PreviewImage = await pictureDownloadControl.DownloadBitmap(picture.PreviewUrl, picture.IsLocal ? picture.Md5 : null);
                    }

                    var added = false;
                    try
                    {
                        if (!(await PictureExists(picture.Md5)))
                        {
                            await semaphoreSlim.WaitAsync();

                            picture.Selected = selectedPictures.Contains(picture);

                            currentPictureSet.Add(picture);
                            seenPictures.Add(picture.Md5);

                            added = true;
                            PictureAdded?.Invoke(this, picture);
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        if (added)
                        {
                            semaphoreSlim.Release();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public async void ClosePicture(Picture picture)
        {
            await Task.Run(() =>
            {
                if (picture.SourceImage != null)
                {
                    picture.SourceImage.Dispose();
                    picture.SourceImage = null;
                }

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
                seenPictures = new HashSet<string>();

                PicturesReset?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task<TagCollection> GetTags()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                return await Task.FromResult(
                    TagCollection.GetTags(
                        this.currentPictureSet.ToList()
                ));
            }
            catch { }
            finally
            {
                semaphoreSlim.Release();
            }

            return await Task.FromResult(new TagCollection());
        }

        public async Task<int> GetSelectedPictureCount()
        {
            return await Task.FromResult(selectedPictures.Count);
        }
    }
}
