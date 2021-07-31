using Avalonia.Media.Imaging;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Extensions;
using TsukiTag.Models;
using TsukiTag.Models.Repository;


namespace TsukiTag.Dependencies
{

    public interface IWorkspacePictureDb
    {
        bool AddToWorkspace(Guid resourceListId, Picture picture, string filePath);

        bool RemoveFromWorkspace(Guid resourceListId, Picture picture);

        List<WorkspacePicture> GetAllForPicture(string md5);

        List<WorkspacePicture> GetAllForFilter(ProviderFilter filter);
    }

    public partial class DbRepository
    {
        public IWorkspacePictureDb WorkspacePicture { get; protected set; }

        private class WorkspacePictureDb : IWorkspacePictureDb
        {
            private readonly IDbRepository parent;

            public WorkspacePictureDb(DbRepository parent)
            {
                this.parent = parent;
                EnsureIndexes();
            }

            public List<WorkspacePicture> GetAllForFilter(ProviderFilter filter)
            {
                try
                {
                    var allLists = parent.Workspace.GetAll();
                    var filterLists = allLists.Where(l => filter.Providers.Contains(l.Name)).Select(s => s.Id).ToList();

                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<WorkspacePicture>();
                        var query = coll.Query();

                        query = query.Where(l => filterLists.Contains(l.ResourceListId));

                        foreach (var tag in filter.Tags)
                        {
                            query = query.Where("COUNT(FILTER($.Picture.TagList => @ = '" + tag + "')) > 0");
                        }

                        var completeQuery = query.Skip(filter.Page * filter.Limit).Limit(filter.Limit);
                        var items = completeQuery.ToList();

                        return items;
                    }
                }
                catch (Exception)
                {
                    return new List<WorkspacePicture>();
                }
            }

            public List<WorkspacePicture> GetAllForPicture(string md5)
            {
                try
                {
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<WorkspacePicture>();
                        var allItems = coll.Find(p => p.Md5 == md5).ToList();

                        return allItems;
                    }
                }
                catch (Exception)
                {
                    return new List<WorkspacePicture>();
                }
            }

            public bool AddToWorkspace(Guid resourceListId, Picture picture, string filePath)
            {
                try
                {
                    var workspace = parent.Workspace.Get(resourceListId);
                    var newid = Guid.NewGuid();
                    WorkspacePicture newItem = null;

                    if (workspace != null)
                    {
                        using (var db = new LiteDatabase(MetadataRepositoryPath))
                        {
                            var coll = db.GetCollection<WorkspacePicture>();

                            var existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ResourceListId == resourceListId);
                            if (existing != null)
                            {
                                existing.Picture = workspace.ProcessPicture(picture.MetadatawiseClone());
                                existing.FilePath = filePath;

                                coll.Update(existing);
                            }
                            else
                            {
                                newItem = new WorkspacePicture()
                                {
                                    Id = newid,
                                    ResourceListId = resourceListId,
                                    Md5 = picture.Md5,
                                    Picture = workspace.ProcessPicture(picture.MetadatawiseClone()),
                                    FilePath = filePath
                                };

                                coll.Insert(newItem);
                            }
                        }

                        if (newItem != null)
                        {
                            parent.WorkspaceHistory.AddHistoryItem(Models.Repository.WorkspaceHistory.PictureAdded<WorkspaceHistory>(newItem));
                        }

                        parent.ThumbnailStorage.AddOrUpdateThumbnail(picture.Md5, picture.PreviewImage);

                        return true;
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool RemoveFromWorkspace(Guid resourceListId, Picture picture)
            {
                try
                {
                    WorkspacePicture existing = null;
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<WorkspacePicture>();
                        existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ResourceListId == resourceListId);

                        if (existing != null)
                        {
                            coll.Delete(existing.Id);
                        }
                    }

                    if (existing != null)
                    {
                        parent.WorkspaceHistory.AddHistoryItem(Models.Repository.WorkspaceHistory.PictureRemoved<WorkspaceHistory>(existing));
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            private void EnsureIndexes()
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    db.GetCollection<WorkspacePicture>().EnsureIndex(p => p.Md5);
                    db.GetCollection<WorkspacePicture>().EnsureIndex(p => p.ResourceListId);
                }
            }
        }
    }
}
