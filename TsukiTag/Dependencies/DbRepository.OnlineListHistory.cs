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
    public interface IOnlineListHistoryDb
    {
        void AddHistoryItem(OnlineListHistory history);
    }

    public partial class DbRepository
    {
        public IOnlineListHistoryDb OnlineListHistory { get; protected set; }

        private class OnlineListHistoryDb : IOnlineListHistoryDb
        {
            public OnlineListHistoryDb()
            {
                EnsureIndexes();
            }

            public void AddHistoryItem(OnlineListHistory history)
            {
                using (var db = new LiteDatabase(HistoryRepositoryPath))
                {
                    db.GetCollection<OnlineListHistory>().Insert(history);
                }
            }

            private void EnsureIndexes()
            {
                using (var db = new LiteDatabase(HistoryRepositoryPath))
                {
                    db.GetCollection<OnlineListHistory>().EnsureIndex(p => p.PictureMd5);
                    db.GetCollection<OnlineListHistory>().EnsureIndex(p => p.Date);
                }
            }
        }

    }
}
