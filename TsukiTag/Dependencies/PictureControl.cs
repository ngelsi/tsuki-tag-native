using Serilog;
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

        event EventHandler<PictureOpenedEventArgs> PictureOpened;

        event EventHandler<Picture> PictureClosed;

        event EventHandler<PictureOpenedEventArgs> PictureOpenedInBackground;

        event EventHandler PicturesReset;

        Task AddPicture(Picture picture);

        void RemovePicture(Picture picture);

        Task ResetPictures();

        void SelectPicture(Picture picture);

        void SelectAllPictures();

        void DeselectPicture(Picture picture);

        Task OpenPicture(Picture picture);

        Task OpenPictureInBackground(Picture picture);

        Task ClosePicture(Picture picture);

        Task<TagCollection> GetTags();

        Task<int> GetSelectedPictureCount();

        Task<bool> PictureExists(string md5);

        Task<Guid> GetPictureContext();

        Task SwitchPictureContext();

        Task<bool> PictureInContext(Picture picture);
    }

    public class PictureControl : IPictureControl
    {
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private List<Picture> currentPictureSet;

        private TagCollection currentTagCollection;

        private List<Picture> selectedPictures;

        private List<Picture> openedPictures;

        private HashSet<string> seenPictures;

        private Guid pictureContext;

        private readonly IPictureDownloader pictureDownloadControl;
        private readonly IDbRepository dbRepository;

        public event EventHandler<Picture> PictureSelected;

        public event EventHandler<Picture> PictureDeselected;

        public event EventHandler<Picture> PictureAdded;

        public event EventHandler<Picture> PictureRemoved;

        public event EventHandler<PictureOpenedEventArgs> PictureOpened;

        public event EventHandler<PictureOpenedEventArgs> PictureOpenedInBackground;

        public event EventHandler<Picture> PictureClosed;

        public event EventHandler PicturesReset;

        public PictureControl(
            IPictureDownloader pictureDownloadControl,
            IDbRepository dbRepository
        )
        {
            this.pictureDownloadControl = pictureDownloadControl;
            this.dbRepository = dbRepository;

            pictureContext = Guid.NewGuid();
            currentTagCollection = new TagCollection();
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
                if (!selectedPictures.Any(p => p.Equals(picture)))
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
            catch (Exception ex)
            {
                Log.Error<Picture>(ex, $"Error occurred while selecting picture", picture);
            }
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
                return await Task.FromResult(
                    this.dbRepository.ApplicationSettings.Get().AllowDuplicateImages ? false :
                    seenPictures.Contains(md5) || currentPictureSet.Any(p => p.Md5 == md5)
                );
            }
            catch { }
            finally
            {
                semaphoreSlim.Release();
            }

            return await Task.FromResult(false);
        }

        public async Task<bool> PictureInContext(Picture picture)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                return await Task.FromResult(picture.PictureContext == pictureContext);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async void DeselectPicture(Picture picture)
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                picture.Selected = false;

                selectedPictures.Remove(picture);

                PictureDeselected?.Invoke(this, picture);
            }
            catch (Exception ex)
            {
                Log.Error<Picture>(ex, $"Error occurred while de-selecting picture", picture);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task OpenPicture(Picture picture)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var bitmap = await this.pictureDownloadControl.DownloadBitmap(picture);
                if (bitmap != null)
                {
                    openedPictures.Add(picture);
                    PictureOpened?.Invoke(this, new PictureOpenedEventArgs(picture, bitmap));
                }
            }
            catch (Exception ex)
            {
                Log.Error<Picture>(ex, $"Error occurred while opening picture", picture);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task OpenPictureInBackground(Picture picture)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var bitmap = await this.pictureDownloadControl.DownloadBitmap(picture);
                if (bitmap != null)
                {
                    openedPictures.Add(picture);
                    PictureOpenedInBackground?.Invoke(this, new PictureOpenedEventArgs(picture, bitmap));
                }
            }
            catch (Exception ex)
            {
                Log.Error<Picture>(ex, $"Error occurred while opening picture in background", picture);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task AddPicture(Picture picture)
        {
            try
            {
                if (!(await PictureExists(picture.Md5)) && (await PictureInContext(picture)))
                {
                    if (picture.PreviewImage == null && !string.IsNullOrEmpty(picture.PreviewUrl))
                    {
                        picture.PreviewImage = await pictureDownloadControl.DownloadBitmap(picture.PreviewUrl, picture.IsLocal ? picture.Md5 : null);
                    }
                    else if (picture.PreviewUrl == null && (picture.IsLocal || picture.IsLocallyImported))
                    {
                        picture.PreviewImage = await pictureDownloadControl.DownloadBitmap(null, picture.Md5);
                    }

                    var added = false;
                    try
                    {
                        if (!(await PictureExists(picture.Md5)) && (await PictureInContext(picture)))
                        {
                            await semaphoreSlim.WaitAsync();

                            picture.Selected = selectedPictures.Contains(picture);

                            currentPictureSet.Add(picture);
                            currentTagCollection.AddPictureTags(picture);
                            seenPictures.Add(picture.Md5);

                            added = true;
                            PictureAdded?.Invoke(this, picture);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error<Picture>(ex, $"Error occurred while downloading thumbnail and adding picture to current collection", picture);
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
            catch (Exception ex)
            {
                Log.Error<Picture>(ex, $"Error occurred while checking picture existence before downloading and adding to current collection", picture);
            }
        }

        public async Task ClosePicture(Picture picture)
        {
            await Task.Run(() =>
            {
                picture.RemovePictureBitmaps();

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

        public async Task ResetPictures()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                currentTagCollection = new TagCollection();
                currentPictureSet = new List<Picture>();
                seenPictures = new HashSet<string>();
            }
            finally
            {
                semaphoreSlim.Release();
            }

            PicturesReset?.Invoke(this, EventArgs.Empty);
        }

        public async Task SwitchPictureContext()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                pictureContext = Guid.NewGuid();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task<Guid> GetPictureContext()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                return await Task.FromResult(pictureContext);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return await Task.FromResult(pictureContext);
        }

        public async Task<TagCollection> GetTags()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                return await Task.FromResult(currentTagCollection);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception occurred while getting current tag collection");
            }
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
