using Avalonia.Media.Imaging;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TsukiTag.Dependencies
{
    public interface IThumbnailStorage
    {
        void AddOrUpdateThumbnail(string md5, Bitmap bitmap);

        void CloseConnection();

        Bitmap? FindThumbnail(string md5);
    }

    public partial class DbRepository
    {
        public IThumbnailStorage ThumbnailStorage { get; protected set; }

        private class ThumbnailStorageDb : IThumbnailStorage
        {
            private LiteDatabase currentConnection;
            private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

            private LiteDatabase GetConnection()
            {
                if (currentConnection == null)
                {
                    currentConnection = new LiteDatabase(ThumbnailRepositoryPath);
                }

                return currentConnection;
            }

            ~ThumbnailStorageDb()
            {
                currentConnection?.Dispose();
            }

            public void CloseConnection()
            {
                currentConnection?.Dispose();
            }

            public Bitmap? FindThumbnail(string md5)
            {
                using (var ms = new MemoryStream())
                {
                    var storage = GetConnection().FileStorage;
                    var existing = storage.FindById(md5);

                    if (existing != null)
                    {
                        existing.CopyTo(ms);
                        ms.Position = 0;

                        return new Bitmap(ms);
                    }
                }

                return null;
            }

            public void AddOrUpdateThumbnail(string md5, Bitmap bitmap)
            {
                try
                {
                    semaphoreSlim.Wait();

                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms);
                        ms.Position = 0;

                        var storage = GetConnection().FileStorage;
                        var existing = storage.Find(f => f.Filename == md5 || f.Id == md5).ToList();                                               

                        if (existing != null && existing.Count > 0)
                        {
                            return;
                        }

                        storage.Upload(md5, md5, ms);
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
        }
    }
}
