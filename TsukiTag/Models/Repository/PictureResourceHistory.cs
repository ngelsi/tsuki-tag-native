using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.Repository
{
    public class PictureResourceHistory
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public string PictureMd5 { get; set; }

        public PictureResourceHistoryItem[] HistoryItems { get; set; }

        public static T PictureAdded<T>(PictureResourcePicture picture) where T: PictureResourceHistory, new()
        {
            return new T()
            {
                Date = DateTime.Now,
                PictureMd5 = picture.Md5,
                HistoryItems = new[]
                {
                    new PictureResourceHistoryItem()
                    {
                        IsAdded = true,
                        ResourceListId = picture.ResourceListId,
                        PictureId = picture.Id
                    }
                }
            };
        }

        public static T PictureAdded<T>(string md5, List<PictureResourcePicture> pictures) where T : PictureResourceHistory, new()
        {
            return new T()
            {
                Date = DateTime.Now,
                PictureMd5 = md5,
                HistoryItems = pictures.Select(s => new PictureResourceHistoryItem()
                {
                    IsAdded = true,
                    ResourceListId = s.ResourceListId,
                    PictureId = s.Id
                }).ToArray()
            };
        }


        public static T PictureRemoved<T>(PictureResourcePicture picture) where T : PictureResourceHistory, new()
        {
            return new T()
            {
                Date = DateTime.Now,
                PictureMd5 = picture.Md5,
                HistoryItems = new[]
                {
                    new PictureResourceHistoryItem()
                    {
                        IsAdded = false,
                        ResourceListId = picture.ResourceListId,
                        PictureId = picture.Id
                    }
                }
            };
        }

        public static T PictureRemoved<T>(string md5, List<PictureResourcePicture> pictures) where T : PictureResourceHistory, new()
        {
            return new T()
            {
                Date = DateTime.Now,
                PictureMd5 = md5,
                HistoryItems = pictures.Select(s => new PictureResourceHistoryItem()
                {
                    IsAdded = false,
                    ResourceListId = s.ResourceListId,
                    PictureId = s.Id
                }).ToArray()
            };
        }
    }

    public class PictureResourceHistoryItem
    {
        public Guid PictureId { get; set; }

        public Guid ResourceListId { get; set; }

        public bool IsAdded { get; set; }
    }
}
