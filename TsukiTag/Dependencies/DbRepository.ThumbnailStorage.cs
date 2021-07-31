using Avalonia.Media.Imaging;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Dependencies
{
    public interface IThumbnailStorage
    {
        void AddOrUpdateThumbnail(string md5, Bitmap bitmap);

        Bitmap? FindThumbnail(string md5);
    }

    public partial class DbRepository
    {
        public IThumbnailStorage ThumbnailStorage { get; protected set; }

        private class ThumbnailStorageDb : IThumbnailStorage
        {
            private LiteDatabase currentConnection;

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
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms);
                        ms.Position = 0;

                        var storage = GetConnection().FileStorage;
                        var existing = storage.FindById(md5);

                        if (existing != null)
                        {
                            storage.Delete(md5);
                        }

                        storage.Upload(md5, md5, ms);
                    }
                }
                catch(Exception)
                {

                }                
            }
        }
    }
}
