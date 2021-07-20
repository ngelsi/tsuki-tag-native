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
    }

    public interface IProviderSessionDb
    {
        ProviderSession? Get(string context);

        void AddOrUpdate(ProviderSession session);
    }

    public class DbRepository : IDbRepository
    {
        protected static string BaseRepositoryPath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TsukiTag");
        protected static string MetadataRepositoryPath => System.IO.Path.Combine(BaseRepositoryPath, MetadataFileName);
        protected static string MetadataFileName => "tsukitag.db";

        public IProviderSessionDb ProviderSession { get; }

        public DbRepository()
        {
            ProviderSession = new ProviderSessionDb();

            EnsureRepositoryPath();
            EnsureIndexes();
        }

        private void EnsureRepositoryPath()
        {
            if (!Directory.Exists(BaseRepositoryPath))
            {
                Directory.CreateDirectory(BaseRepositoryPath);
            }
        }

        private void EnsureIndexes()
        {
            using (var db = new LiteDatabase(MetadataRepositoryPath))
            {
                db.GetCollection<ProviderSession>().EnsureIndex(p => p.Context);
            }
        }

        private class ProviderSessionDb : IProviderSessionDb
        {
            public ProviderSession? Get(string context)
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var session = db.GetCollection<ProviderSession>()
                                    .Query()
                                    .Where(s => s.Context == context)
                                    .FirstOrDefault();

                    return session;
                }
            }

            public void AddOrUpdate(ProviderSession session)
            {
                if (session != null && !string.IsNullOrEmpty(session.Context))
                {
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var collection = db.GetCollection<ProviderSession>();
                        var dbSession = collection
                                        .Query()
                                        .Where(s => s.Context == session.Context)
                                        .FirstOrDefault();

                        if (dbSession == null)
                        {
                            dbSession = session;
                            collection.Insert(dbSession);
                        }
                        else
                        {
                            collection.Update(session);
                        }
                    }
                }
            }
        }
    }
}
