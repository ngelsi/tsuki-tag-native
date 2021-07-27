using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.Repository
{
    public class OnlineListHistory
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public string PictureMd5 { get; set; }

        public OnlineListHistoryItem[] HistoryItems { get; set; }

        public static OnlineListHistory PictureAdded(OnlineListPicture picture)
        {
            return new OnlineListHistory()
            {
                Date = DateTime.Now,
                PictureMd5 = picture.Md5,
                HistoryItems = new[]
                {
                    new OnlineListHistoryItem()
                    {
                        IsAdded = true,
                        ListId = picture.ListId,
                        PictureId = picture.Id
                    }
                }
            };
        }

        public static OnlineListHistory PictureAdded(string md5, List<OnlineListPicture> pictures)
        {
            return new OnlineListHistory()
            {
                Date = DateTime.Now,
                PictureMd5 = md5,
                HistoryItems = pictures.Select(s => new OnlineListHistoryItem()
                {
                    IsAdded = true,
                    ListId = s.ListId,
                    PictureId = s.Id
                }).ToArray()
            };
        }


        public static OnlineListHistory PictureRemoved(OnlineListPicture picture)
        {
            return new OnlineListHistory()
            {
                Date = DateTime.Now,
                PictureMd5 = picture.Md5,
                HistoryItems = new[]
                {
                    new OnlineListHistoryItem()
                    {
                        IsAdded = false,
                        ListId = picture.ListId,
                        PictureId = picture.Id
                    }
                }
            };
        }

        public static OnlineListHistory PictureRemoved(string md5, List<OnlineListPicture> pictures)
        {
            return new OnlineListHistory()
            {
                Date = DateTime.Now,
                PictureMd5 = md5,
                HistoryItems = pictures.Select(s => new OnlineListHistoryItem()
                {
                    IsAdded = false,
                    ListId = s.ListId,
                    PictureId = s.Id
                }).ToArray()
            };
        }
    }

    public class OnlineListHistoryItem
    {
        public Guid PictureId { get; set; }

        public Guid ListId { get; set; }

        public bool IsAdded { get; set; }
    }
}
