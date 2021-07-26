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
    }

    public partial class DbRepository : IDbRepository
    {
        protected static string BaseRepositoryPath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TsukiTag");
        protected static string MetadataRepositoryPath => System.IO.Path.Combine(BaseRepositoryPath, MetadataFileName);
        protected static string MetadataFileName => "tsukitag.db";


        public DbRepository()
        {
            EnsureRepositoryPath();

            ProviderSession = new ProviderSessionDb();
            OnlineList = new OnlineListDb();
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
