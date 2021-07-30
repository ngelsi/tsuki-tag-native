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
        bool AddToList(Guid resourceListId, Picture picture);

        bool AddToAllLists(Picture picture);

        bool AddToAllEligible(Picture picture);

        bool RemoveFromList(Guid resourceListId, Picture picture);

        bool RemoveFromAllLists(Picture picture);

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

            public bool AddToAllLists(Picture picture)
            {
                return AddToLists(picture, parent.Workspace.GetAll());
            }

            public bool AddToAllEligible(Picture picture)
            {
                return AddToLists(picture, parent.Workspace.GetAll().Where(l => l.IsEligible(picture)).ToList());
            }

            public bool AddToList(Guid resourceListId, Picture picture)
            {
                try
                {
                    var list = parent.Workspace.Get(resourceListId);
                    var newid = Guid.NewGuid();
                    WorkspacePicture newItem = null;

                    if (list != null)
                    {
                        using (var db = new LiteDatabase(MetadataRepositoryPath))
                        {
                            var coll = db.GetCollection<WorkspacePicture>();

                            var existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ResourceListId == resourceListId);
                            if (existing != null)
                            {
                                existing.Picture = list.ProcessPicture(picture.MetadatawiseClone());
                                coll.Update(existing);
                            }
                            else
                            {
                                newItem = new WorkspacePicture()
                                {
                                    Id = newid,
                                    ResourceListId = resourceListId,
                                    Md5 = picture.Md5,
                                    Picture = list.ProcessPicture(picture.MetadatawiseClone())
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

            public bool RemoveFromAllLists(Picture picture)
            {
                try
                {
                    List<WorkspacePicture> allItems;
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<WorkspacePicture>();
                        allItems = coll.Find(p => p.Md5 == picture.Md5).ToList();

                        foreach (var item in allItems)
                        {
                            coll.Delete(item.Id);
                        }
                    }

                    if (allItems?.Count > 0)
                    {
                        parent.WorkspaceHistory.AddHistoryItem(Models.Repository.WorkspaceHistory.PictureRemoved<WorkspaceHistory>(picture.Md5, allItems.Cast<PictureResourcePicture>().ToList()));
                    }

                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool RemoveFromList(Guid resourceListId, Picture picture)
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

            private bool AddToLists(Picture picture, List<Workspace> workspaces)
            {
                try
                {
                    var pictures = new List<WorkspacePicture>();

                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<WorkspacePicture>();

                        foreach (var list in workspaces)
                        {
                            var existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ResourceListId == list.Id);
                            if (existing != null)
                            {
                                existing.Picture = list.ProcessPicture(picture.MetadatawiseClone());

                                coll.Update(existing);
                                pictures.Add(existing);
                            }
                            else
                            {
                                var newItem = new WorkspacePicture()
                                {
                                    Id = Guid.NewGuid(),
                                    ResourceListId = list.Id,
                                    Md5 = picture.Md5,
                                    Picture = list.ProcessPicture(picture.MetadatawiseClone())
                                };

                                coll.Insert(newItem);
                                pictures.Add(newItem);
                            }
                        }

                    }

                    if (pictures.Count > 0)
                    {
                        parent.WorkspaceHistory.AddHistoryItem(Models.Repository.WorkspaceHistory.PictureAdded<WorkspaceHistory>(picture.Md5, pictures.Cast<PictureResourcePicture>().ToList()));
                        parent.ThumbnailStorage.AddOrUpdateThumbnail(picture.Md5, picture.PreviewImage);
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
