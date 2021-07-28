using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models.Repository;

namespace TsukiTag.Dependencies
{
    public interface IDbRepository
    {
        IProviderSessionDb ProviderSession { get; }

        IOnlineListDb OnlineList { get; }

        IOnlineListPictureDb OnlineListPicture { get; }

        IOnlineListHistoryDb OnlineListHistory { get; }

        IThumbnailStorage ThumbnailStorage { get; }
    }

    public partial class DbRepository : IDbRepository
    {
        protected static string BaseRepositoryPath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TsukiTag");
        protected static string MetadataRepositoryPath => System.IO.Path.Combine(BaseRepositoryPath, MetadataFileName);
        protected static string ThumbnailRepositoryPath => System.IO.Path.Combine(BaseRepositoryPath, ThumbnailFileName);
        protected static string HistoryRepositoryPath => System.IO.Path.Combine(BaseRepositoryPath, HistoryFileName);

        protected static string MetadataFileName => "tsukitag.db";
        protected static string ThumbnailFileName => "tsukitag.thumbs.db";
        protected static string HistoryFileName => "tsukitag.history.db";


        public DbRepository()
        {
            EnsureRepositoryPath();

            ProviderSession = new ProviderSessionDb(this);
            OnlineList = new OnlineListDb();
            OnlineListPicture = new OnlineListPictureDb(this);
            OnlineListHistory = new OnlineListHistoryDb();
            ThumbnailStorage = new ThumbnailStorageDb();
        }

        private void EnsureRepositoryPath()
        {
            if (!Directory.Exists(BaseRepositoryPath))
            {
                Directory.CreateDirectory(BaseRepositoryPath);
            }
        }
    }
}
