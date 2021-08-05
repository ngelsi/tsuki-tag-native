using LiteDB;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models.Repository;

namespace TsukiTag.Dependencies
{
    public interface IMetadataGroupDb
    {
        event EventHandler MetadataGroupsChanged;

        List<MetadataGroup> GetAll();

        MetadataGroup Get(Guid id);

        MetadataGroup GetDefault();

        void AddOrUpdate(MetadataGroup MetadataGroup);

        void AddOrUpdate(List<MetadataGroup> MetadataGroups);
    }

    public partial class DbRepository
    {
        public IMetadataGroupDb MetadataGroup { get; protected set; }

        private class MetadataGroupDb : IMetadataGroupDb
        {
            public event EventHandler MetadataGroupsChanged;

            private List<MetadataGroup> metadataGroupCache;

            public MetadataGroup Get(Guid id)
            {
                EnsureMetadataGroupCache();
                return metadataGroupCache.FirstOrDefault(w => w.Id == id);
            }

            public MetadataGroup GetDefault()
            {
                EnsureMetadataGroupCache();
                return metadataGroupCache.FirstOrDefault(w => w.IsDefault == true);
            }

            public void AddOrUpdate(MetadataGroup MetadataGroup)
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<MetadataGroup>();
                    coll.Upsert(MetadataGroup);
                }

                EnsureMetadataGroupCache(true);
                MetadataGroupsChanged?.Invoke(this, EventArgs.Empty);
            }

            public void AddOrUpdate(List<MetadataGroup> MetadataGroups)
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<MetadataGroup>();
                    var allPreviousMetadataGroups = coll.Query().ToList();

                    foreach (var metadataGroup in MetadataGroups)
                    {
                        coll.Upsert(metadataGroup);
                    }

                    var deletedMetadataGroups = allPreviousMetadataGroups.Where(a => !MetadataGroups.Any(l => l.Id == a.Id)).ToList();
                    foreach (var deletedMetadataGroup in deletedMetadataGroups)
                    {
                        coll.Delete(deletedMetadataGroup.Id);

                        var workspaceCollection = db.GetCollection<Workspace>();
                        var workspaces = workspaceCollection.Query().Where(w => w.MetadataGroupId == deletedMetadataGroup.Id).ToList();

                        foreach (var workspace in workspaces)
                        {
                            workspace.MetadataGroupId = null;
                            workspaceCollection.Update(workspace);
                        }
                    }
                }

                EnsureMetadataGroupCache(true);
                MetadataGroupsChanged?.Invoke(this, EventArgs.Empty);
            }

            public List<MetadataGroup> GetAll()
            {
                EnsureMetadataGroupCache();
                return metadataGroupCache;
            }

            private void EnsureMetadataGroupCache(bool reset = false)
            {
                if (reset || metadataGroupCache == null)
                {
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<MetadataGroup>();
                        var MetadataGroups = coll.Query().ToList();

                        metadataGroupCache = MetadataGroups.OrderBy(l => l.Name).ToList();
                    }
                }
            }
        }
    }
}
