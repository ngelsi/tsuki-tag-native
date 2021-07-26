﻿using HarfBuzzSharp;
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
    public interface IOnlineListDb
    {
        List<OnlineList> GetAll();

        void AddOrUpdate(List<OnlineList> lists);
    }

    public partial class DbRepository
    {
        public IOnlineListDb OnlineList { get; protected set; }

        private class OnlineListDb : IOnlineListDb
        {
            public void AddOrUpdate(List<OnlineList> lists)
            {
                using(var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<OnlineList>();
                    var allPreviousLists = coll.Query().ToList();                    

                    foreach(var list in lists)
                    {
                        var dbList = coll.FindOne(l => l.Id == list.Id);
                        if(dbList == null)
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
                    }
                }
            }

            public List<OnlineList> GetAll()
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

                    return lists.OrderBy(l => l.Name).ToList();
                }
            }
        }
    }
}
