﻿using Avalonia.Media.Imaging;
using LiteDB;
using Serilog;
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
                        var items = new List<WorkspacePicture>();

                        query = query.Where(l => filterLists.Contains(l.ResourceListId));

                        if (filter.Tags.Count == 0)
                        {
                            var completeQuery = query.Skip(filter.Page * filter.Limit).Limit(filter.Limit);
                            items = completeQuery.ToList();
                        }
                        else if (filter.Tags.Count > 0 && filter.RawTags?.Count == filter.Tags.Count)
                        {
                            foreach (var tag in filter.Tags)
                            {
                                query = query.Where("COUNT(FILTER($.Picture.TagList => @ = '" + tag + "')) > 0");
                            }

                            var completeQuery = query.Skip(filter.Page * filter.Limit).Limit(filter.Limit);
                            items = completeQuery.ToList();
                        }
                        else if (filter.Tags.Count > 0)
                        {
                            var allItems = query.ToList();
                            var tagsToSearch = filter.TagsWithoutPragma;
                            var sortingKeyword = filter.SortingKeyword;

                            if (!string.IsNullOrEmpty(sortingKeyword))
                            {
                                allItems = allItems.OrderByKeyword(sortingKeyword);
                            }

                            if (tagsToSearch?.Count > 0)
                            {
                                var i = 0;
                                while (items.Count <= ((filter.Page + 1) * filter.Limit))
                                {
                                    var item = allItems.ElementAtOrDefault(i);
                                    if (item == null)
                                    {
                                        break;
                                    }

                                    if (item != null && item.Picture.TagList.Any(t => tagsToSearch.Any(tt => t.WildcardMatches(tt))))
                                    {
                                        items.Add(item);
                                    }

                                    i++;
                                }
                            }
                            else
                            {
                                items = allItems;
                            }

                            items = items.Skip(filter.Page * filter.Limit).Take(filter.Limit).ToList();
                        }

                        items.ForEach((item) =>
                        {
                            item.Picture.FileUrl = item.FilePath;
                            item.Picture.IsLocal = true;
                            item.Picture.IsWorkspace = true;
                            item.Picture.LocalProviderType = Language.Workspace;
                            item.Picture.LocalProviderId = item.ResourceListId;
                            item.Picture.LocalProvider = parent.Workspace.Get(item.ResourceListId)?.Name;
                        });

                        return items;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error<ProviderFilter>(ex, "Error occurred while getting workspace pictures for filter", filter);
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

                        allItems.ForEach((item) =>
                        {
                            item.Picture.FileUrl = item.FilePath;
                            item.Picture.IsLocal = true;
                            item.Picture.IsWorkspace = true;
                            item.Picture.LocalProviderType = Language.Workspace;
                            item.Picture.LocalProviderId = item.ResourceListId;
                            item.Picture.LocalProvider = parent.Workspace.Get(item.ResourceListId)?.Name;
                        });

                        return allItems;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error occurred while getting all workspace pictures for hash {md5}");
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
                                existing.DateModified = DateTime.Now;
                                existing.FilePath = filePath;

                                if(existing.DateAdded == null)
                                {
                                    existing.DateAdded = DateTime.Now;
                                }

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
                                    DateAdded = DateTime.Now,
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
                catch (Exception ex)
                {
                    Log.Error<Picture>(ex, $"Could not add picture to workspace {resourceListId} with file path {filePath}", picture);
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
                catch (Exception ex)
                {
                    Log.Error<Picture>($"Could not remove picture from workspace {resourceListId}", picture);
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
