using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;
using TsukiTag.Models.Repository;


namespace TsukiTag.Dependencies
{

    public interface IOnlineListPictureDb
    {
        bool AddToList(Guid listId, Picture picture);

        bool AddToAllLists(Picture picture);

        bool AddToAllEligible(Picture picture);

        bool RemoveFromList(Guid listId, Picture picture);

        bool RemoveFromAllLists(Picture picture);

        List<OnlineListPicture> GetAllForPicture(string md5);
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

            public List<OnlineListPicture> GetAllForPicture(string md5)
            {
                try
                {
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineListPicture>();
                        var allItems = coll.Find(p => p.Md5 == md5).ToList();                        

                        return allItems;
                    }
                }
                catch (Exception)
                {
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

            public bool AddToList(Guid listId, Picture picture)
            {
                try
                {
                    var list = parent.OnlineList.Get(listId);
                    var newid = Guid.NewGuid();
                    OnlineListPicture newItem = null;

                    if (list != null)
                    {
                        using (var db = new LiteDatabase(MetadataRepositoryPath))
                        {
                            var coll = db.GetCollection<OnlineListPicture>();

                            var existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ListId == listId);
                            if (existing != null)
                            {
                                existing.Picture = list.ProcessPicture(picture.MetadatawiseClone());
                                coll.Update(existing);
                            }
                            else
                            {
                                newItem = new OnlineListPicture()
                                {
                                    Id = newid,
                                    ListId = listId,
                                    Md5 = picture.Md5,
                                    Picture = list.ProcessPicture(picture.MetadatawiseClone())
                                };

                                coll.Insert(newItem);
                            }
                        }

                        if(newItem != null)
                        {
                            parent.OnlineListHistory.AddHistoryItem(Models.Repository.OnlineListHistory.PictureAdded(newItem));
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
                    List<OnlineListPicture> allItems;
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineListPicture>();
                        allItems = coll.Find(p => p.Md5 == picture.Md5).ToList();

                        foreach(var item in allItems)
                        {
                            coll.Delete(item.Id);
                        }
                    }

                    if (allItems?.Count > 0)
                    {
                        parent.OnlineListHistory.AddHistoryItem(Models.Repository.OnlineListHistory.PictureRemoved(picture.Md5, allItems));
                    }

                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            }

            public bool RemoveFromList(Guid listId, Picture picture)
            {
                try
                {
                    OnlineListPicture existing = null;
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineListPicture>();
                        existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ListId == listId);

                        if (existing != null)
                        {
                            coll.Delete(existing.Id);
                        }
                    }

                    if(existing != null)
                    {
                        parent.OnlineListHistory.AddHistoryItem(Models.Repository.OnlineListHistory.PictureRemoved(existing));
                    }

                    return true;
                }
                catch (Exception)
                {
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
                            var existing = coll.FindOne(p => p.Md5 == picture.Md5 && p.ListId == list.Id);
                            if (existing != null)
                            {
                                existing.Picture = list.ProcessPicture(picture.MetadatawiseClone());

                                coll.Update(existing);
                                pictures.Add(existing);
                            }
                            else
                            {
                                var newItem = new OnlineListPicture()
                                {
                                    Id = Guid.NewGuid(),
                                    ListId = list.Id,
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
                        parent.OnlineListHistory.AddHistoryItem(Models.Repository.OnlineListHistory.PictureAdded(picture.Md5, pictures));
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
                    db.GetCollection<OnlineListPicture>().EnsureIndex(p => p.Md5);
                    db.GetCollection<OnlineListPicture>().EnsureIndex(p => p.ListId);
                }
            }
        }
    }
}
