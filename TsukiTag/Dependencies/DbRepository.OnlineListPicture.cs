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

    public interface IOnlineListPictureDb
    {
        bool AddToList(Guid resourceListId, Picture picture);

        bool AddToAllLists(Picture picture);

        bool AddToAllEligible(Picture picture);

        bool RemoveFromList(Guid resourceListId, Picture picture);

        bool RemoveFromAllLists(Picture picture);

        List<OnlineListPicture> GetAllForPicture(string md5);

        List<OnlineListPicture> GetAllForFilter(ProviderFilter filter);
    }

    public partial class DbRepository
    {
        public IOnlineListPictureDb OnlineListPicture { get; protected set; }

        private class OnlineListPictureDb : IOnlineListPictureDb
        {
            private readonly IDbRepository parent;

            public OnlineListPictureDb(DbRepository parent)
            {
                this.parent = parent;
                EnsureIndexes();
            }

            public List<OnlineListPicture> GetAllForFilter(ProviderFilter filter)
            {
                try
                {
                    var allLists = parent.OnlineList.GetAll();
                    var filterLists = allLists.Where(l => filter.Providers.Contains(l.Name)).Select(s => s.Id).ToList();

                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineListPicture>();
                        var query = coll.Query();
                        var items = new List<OnlineListPicture>();

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
                            item.Picture.IsLocal = true;
                            item.Picture.IsOnlineList = true;
                            item.Picture.LocalProviderType = Language.OnlineList;
                            item.Picture.LocalProviderId = item.ResourceListId;
                            item.Picture.LocalProvider = parent.OnlineList.Get(item.ResourceListId)?.Name;
                        });

                        return items;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error<ProviderFilter>(ex, "Error occurred while retrieving online list pictures for filter", filter);
                    return new List<OnlineListPicture>();
                }
            }

            public List<OnlineListPicture> GetAllForPicture(string md5)
            {
                try
                {
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineListPicture>();
                        var items = coll.Find(p => p.Md5 == md5).ToList();

                        items.ForEach((item) =>
                        {
                            item.Picture.IsLocal = true;
                            item.Picture.IsOnlineList = true;
                            item.Picture.LocalProviderType = Language.OnlineList;
                            item.Picture.LocalProviderId = item.ResourceListId;
                            item.Picture.LocalProvider = parent.OnlineList.Get(item.ResourceListId)?.Name;
                        });

                        return items;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error<string>(ex, "Error occurred while retrieving all online list pictures for MD5", md5);
                    return new List<OnlineListPicture>();
                }
            }

            public bool AddToAllLists(Picture picture)
            {
                return AddToLists(picture, parent.OnlineList.GetAll());
            }

            public bool AddToAllEligible(Picture picture)
            {
                return AddToLists(picture, parent.OnlineList.GetAll().Where(l => l.IsEligible(picture)).ToList());
            }

            public bool AddToList(Guid resourceListId, Picture picture)
            {
                try
                {
                    var list = parent.OnlineList.Get(resourceListId);
                    var newid = Guid.NewGuid();
                    OnlineListPicture newItem = null;

                    if (list != null)
                    {
                        using (var db = new LiteDatabase(MetadataRepositoryPath))
                        {
                            var coll = db.GetCollection<OnlineListPicture>();

                            var existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ResourceListId == resourceListId);
                            if (existing != null)
                            {
                                existing.Picture = list.ProcessPicture(picture.MetadatawiseClone());
                                existing.DateModified = DateTime.Now;

                                if (existing.DateAdded == null)
                                {
                                    existing.DateAdded = DateTime.Now;
                                }

                                coll.Update(existing);
                            }
                            else
                            {
                                newItem = new OnlineListPicture()
                                {
                                    Id = newid,
                                    ResourceListId = resourceListId,
                                    Md5 = picture.Md5,
                                    DateAdded = DateTime.Now,
                                    Picture = list.ProcessPicture(picture.MetadatawiseClone())
                                };

                                coll.Insert(newItem);
                            }
                        }

                        if (newItem != null)
                        {
                            parent.OnlineListHistory.AddHistoryItem(Models.Repository.OnlineListHistory.PictureAdded<OnlineListHistory>(newItem));
                        }

                        parent.ThumbnailStorage.AddOrUpdateThumbnail(picture.Md5, picture.PreviewImage);

                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Log.Error<Picture>(ex, $"Could not add picture to online list {resourceListId}", picture);
                    return false;
                }
            }

            public bool RemoveFromAllLists(Picture picture)
            {
                try
                {
                    List<OnlineListPicture> allItems;
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineListPicture>();
                        allItems = coll.Find(p => p.Md5 == picture.Md5).ToList();

                        foreach (var item in allItems)
                        {
                            coll.Delete(item.Id);
                        }
                    }

                    if (allItems?.Count > 0)
                    {
                        parent.OnlineListHistory.AddHistoryItem(Models.Repository.OnlineListHistory.PictureRemoved<OnlineListHistory>(picture.Md5, allItems.Cast<PictureResourcePicture>().ToList()));
                    }

                    return true;

                }
                catch (Exception ex)
                {
                    Log.Error<Picture>(ex, $"Could not remove picture from online lists", picture);
                    return false;
                }
            }

            public bool RemoveFromList(Guid resourceListId, Picture picture)
            {
                try
                {
                    OnlineListPicture existing = null;
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineListPicture>();
                        existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ResourceListId == resourceListId);

                        if (existing != null)
                        {
                            coll.Delete(existing.Id);
                        }
                    }

                    if (existing != null)
                    {
                        parent.OnlineListHistory.AddHistoryItem(Models.Repository.OnlineListHistory.PictureRemoved<OnlineListHistory>(existing));
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error<Picture>(ex, $"Could not remove picture from online list {resourceListId}", picture);
                    return false;
                }
            }

            private bool AddToLists(Picture picture, List<OnlineList> lists)
            {
                try
                {
                    var pictures = new List<OnlineListPicture>();

                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineListPicture>();

                        foreach (var list in lists)
                        {
                            var existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ResourceListId == list.Id);
                            if (existing != null)
                            {
                                existing.Picture = list.ProcessPicture(picture.MetadatawiseClone());
                                existing.DateModified = DateTime.Now;

                                if (existing.DateAdded == null)
                                {
                                    existing.DateAdded = DateTime.Now;
                                }

                                coll.Update(existing);
                                pictures.Add(existing);
                            }
                            else
                            {
                                var newItem = new OnlineListPicture()
                                {
                                    Id = Guid.NewGuid(),
                                    ResourceListId = list.Id,
                                    Md5 = picture.Md5,
                                    DateAdded = DateTime.Now,
                                    Picture = list.ProcessPicture(picture.MetadatawiseClone())
                                };

                                coll.Insert(newItem);
                                pictures.Add(newItem);
                            }
                        }

                    }

                    if (pictures.Count > 0)
                    {
                        parent.OnlineListHistory.AddHistoryItem(Models.Repository.OnlineListHistory.PictureAdded<OnlineListHistory>(picture.Md5, pictures.Cast<PictureResourcePicture>().ToList()));
                        parent.ThumbnailStorage.AddOrUpdateThumbnail(picture.Md5, picture.PreviewImage);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error<Picture>(ex, $"Could not add picture to specific online lists", picture);
                    return false;
                }
            }

            private void EnsureIndexes()
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    db.GetCollection<OnlineListPicture>().EnsureIndex(p => p.Md5);
                    db.GetCollection<OnlineListPicture>().EnsureIndex(p => p.ResourceListId);
                }
            }
        }
    }
}
