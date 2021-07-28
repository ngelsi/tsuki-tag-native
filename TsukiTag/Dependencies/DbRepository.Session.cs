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
    public interface IProviderSessionDb
    {
        ProviderSession? Get(string context);

        void AddOrUpdate(ProviderSession session);
    }

    public partial class DbRepository
    {
        public IProviderSessionDb ProviderSession { get; protected set; }

        private class ProviderSessionDb : IProviderSessionDb
        {
            private readonly IDbRepository parent;

            public ProviderSessionDb(IDbRepository parent)
            {
                this.parent = parent;
                EnsureIndexes();
            }

            public ProviderSession? Get(string context)
            {
                ProviderSession? session = null;
                bool newItem = false;

                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<ProviderSession>();
                    session = coll
                                .Query()
                                .Where(s => s.Context == context)
                                .FirstOrDefault();
                }

                if (session == null && context == Models.Repository.ProviderSession.OnlineProviderSession)
                {
                    session = new ProviderSession()
                    {
                        Context = context,
                        Providers = new string[] {
                                Models.Provider.Safebooru.Name,
                                Models.Provider.Gelbooru.Name,
                                Models.Provider.Konachan.Name,
                                Models.Provider.Danbooru.Name,
                                Models.Provider.Yandere.Name
                            },
                        Ratings = new string[]
                        {
                            Models.Rating.Safe.Name
                        },
                        Limit = 25
                    };

                    newItem = true;
                }
                else if (session == null && context == Models.Repository.ProviderSession.AllOnlineListsSession)
                {
                    session = new ProviderSession()
                    {
                        Context = context,
                        Providers = parent.OnlineList.GetAll().Select(s => s.Name).ToArray(),
                        Ratings = new string[]
                        {
                            Models.Rating.Safe.Name
                        },
                        Limit = 75
                    };

                    newItem = true;
                }
                else if (session == null && Guid.TryParse(context, out Guid id))
                {
                    session = new ProviderSession()
                    {
                        Context = context,
                        Providers = new string [] { parent.OnlineList.Get(id)?.Name },
                        Ratings = new string[]
                        {
                            Models.Rating.Safe.Name
                        },
                        Limit = 75
                    };

                    newItem = true;
                }

                if (newItem)
                {
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<ProviderSession>();
                        coll.Insert(session);
                    }
                }

                return session;

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

            private void EnsureIndexes()
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    db.GetCollection<ProviderSession>().EnsureIndex(p => p.Context);
                }
            }
        }
    }
}
