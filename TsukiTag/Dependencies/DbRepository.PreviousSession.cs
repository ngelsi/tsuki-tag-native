using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models.Repository;

namespace TsukiTag.Dependencies
{
    public interface IPreviousSessionDb
    {
        event EventHandler PreviousSessionsChanged;

        List<PreviousSession> GetAll();

        void Clear();

        void AddOrUpdate(List<PreviousSession> PreviousSessions);
    }

    public partial class DbRepository
    {
        public IPreviousSessionDb PreviousSession { get; protected set; }

        private class PreviousSessionDb : IPreviousSessionDb
        {
            private const int MaximumPreviousSessions = 20;
            private List<PreviousSession> previousSessionCache;

            public event EventHandler PreviousSessionsChanged;

            public List<PreviousSession> GetAll()
            {
                EnsureSessionCache();
                return previousSessionCache;
            }

            public void AddOrUpdate(List<PreviousSession> PreviousSessions)
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<PreviousSession>();
                    var allPreviousPreviousSessions = coll.Query().ToList();

                    foreach (var PreviousSession in PreviousSessions)
                    {
                        coll.Upsert(PreviousSession);
                    }
                }

                EnsureSessionCache(true);
                PreviousSessionsChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Clear()
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<PreviousSession>();
                    coll.DeleteAll();
                }

                EnsureSessionCache(true);
                PreviousSessionsChanged?.Invoke(this, EventArgs.Empty);
            }

            private void EnsureSessionCache(bool reset = false)
            {
                if (reset || previousSessionCache == null)
                {
                    List<PreviousSession> previousSessions = null;

                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<PreviousSession>();
                        previousSessions = coll.Query().ToList().OrderByDescending(s => s.Added).ToList();

                        if (previousSessions.Count > MaximumPreviousSessions)
                        {
                            foreach (var session in previousSessions.Skip(MaximumPreviousSessions).ToList())
                            {
                                if (session != null)
                                {
                                    coll.Delete(session.Id);
                                }
                            }

                            previousSessionCache = previousSessions.Take(MaximumPreviousSessions).ToList();
                        }
                        else
                        {
                            previousSessionCache = previousSessions;
                        }
                    }
                }
            }
        }
    }
}
