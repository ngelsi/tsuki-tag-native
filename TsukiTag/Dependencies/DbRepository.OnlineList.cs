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
    public interface IOnlineListDb
    {
        event EventHandler OnlineListsChanged;

        List<OnlineList> GetAll();

        OnlineList Get(Guid id);

        OnlineList GetDefault();

        void AddOrUpdate(List<OnlineList> lists);
    }

    public partial class DbRepository
    {
        public IOnlineListDb OnlineList { get; protected set; }

        private class OnlineListDb : IOnlineListDb
        {
            public event EventHandler OnlineListsChanged;

            private List<OnlineList> onlineListCache;

            public OnlineList Get(Guid id)
            {
                EnsureOnlineListCache();
                return onlineListCache.FirstOrDefault(l => l.Id == id);
            }

            public OnlineList GetDefault()
            {
                EnsureOnlineListCache();
                return onlineListCache.FirstOrDefault(l => l.IsDefault == true);
            }

            public void AddOrUpdate(List<OnlineList> lists)
            {
                using (var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<OnlineList>();
                    var allPreviousLists = coll.Query().ToList();

                    foreach (var list in lists)
                    {
                        var dbList = coll.FindOne(l => l.Id == list.Id);
                        if (dbList == null)
                        {
                            coll.Insert(list);
                        }
                        else
                        {
                            dbList.Name = list.Name;
                            dbList.TagsToAdd = list.TagsToAdd;
                            dbList.TagsToRemove = list.TagsToRemove;
                            dbList.OptionalConditionTags = list.OptionalConditionTags;
                            dbList.MandatoryConditionTags = list.MandatoryConditionTags;
                            dbList.IsDefault = list.IsDefault;

                            coll.Update(dbList);
                        }
                    }

                    var deletedLists = allPreviousLists.Where(a => !lists.Any(l => l.Id == a.Id)).ToList();
                    foreach (var deletedList in deletedLists)
                    {
                        coll.Delete(deletedList.Id);
                        db.GetCollection<OnlineListPicture>().DeleteMany(p => p.ResourceListId == deletedList.Id);                        
                    }
                }

                EnsureOnlineListCache(true);
                OnlineListsChanged?.Invoke(this, EventArgs.Empty);
            }

            public List<OnlineList> GetAll()
            {
                EnsureOnlineListCache();
                return onlineListCache;
            }

            private void EnsureOnlineListCache(bool reset = false)
            {
                if(reset || onlineListCache == null)
                {
                    using (var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<OnlineList>();
                        var lists = coll.Query().ToList();

                        if (lists == null || lists.Count == 0)
                        {
                            lists = new List<OnlineList>();

                            var favoriteList = new OnlineList()
                            {
                                Id = Models.Repository.OnlineList.DefaultFavoriteList,
                                IsDefault = true,
                                Name = Models.Language.ListFavorite,
                                TagsToAdd = new string[] { $"list_favorites", $"provider_#provider#", $"rating_#rating#" }
                            };

                            coll.Insert(favoriteList);

                            lists.Add(favoriteList);
                        }

                        onlineListCache = lists.OrderBy(l => l.Name).ToList();
                    }
                }
            }
        }
    }
}
