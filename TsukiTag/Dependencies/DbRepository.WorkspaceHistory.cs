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
    public interface IWorkspaceHistoryDb
    {
        void AddHistoryItem(WorkspaceHistory history);
    }

    public partial class DbRepository
    {
        public IWorkspaceHistoryDb WorkspaceHistory { get; protected set; }

        private class WorkspaceHistoryDb : IWorkspaceHistoryDb
        {
            public WorkspaceHistoryDb()
            {
                EnsureIndexes();
            }

            public void AddHistoryItem(WorkspaceHistory history)
            {
                using (var db = new LiteDatabase(HistoryRepositoryPath))
                {
                    db.GetCollection<WorkspaceHistory>().Insert(history);
                }
            }

            private void EnsureIndexes()
            {
                using (var db = new LiteDatabase(HistoryRepositoryPath))
                {
                    db.GetCollection<WorkspaceHistory>().EnsureIndex(p => p.PictureMd5);
                    db.GetCollection<WorkspaceHistory>().EnsureIndex(p => p.Date);
                }
            }
        }

    }
}
