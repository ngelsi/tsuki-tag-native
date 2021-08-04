using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class TagCollection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Dictionary<string, int> tags;
        private List<TagCollectionElement> tagElements;

        public List<TagCollectionElement> Tags
        {
            get { return tagElements; }
            set
            {
                tagElements = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagCount)));
            }
        }

        public int TagCount
        {
            get
            {
                return Tags.Count;
            }
        }

        public TagCollection()
        {
            tags = new Dictionary<string, int>();
            Tags = new List<TagCollectionElement>();
        }

        ~TagCollection()
        {
            if (PropertyChanged != null)
            {
                foreach (var p in PropertyChanged.GetInvocationList())
                {
                    PropertyChanged -= (p as PropertyChangedEventHandler);
                }
            }
        }

        public void AddPictureTags(Picture picture)
        {
            if (picture != null)
            {
                foreach (var pictureTag in picture.TagList)
                {
                    if (tags.ContainsKey(pictureTag))
                    {
                        tags[pictureTag] += 1;
                    }
                    else
                    {
                        tags.Add(pictureTag, 1);
                    }
                }

                Tags = tags.OrderByDescending(d => d.Value).Select(t => new TagCollectionElement() { Tag = t.Key, Count = t.Value }).ToList();
            }
        }

        public static TagCollection GetTags(List<Picture> pictures)
        {
            var tags = new TagCollection();
            var dict = new Dictionary<string, int>();

            foreach (var picture in pictures)
            {
                if (picture != null)
                {
                    var pictureTags = picture.TagList;
                    foreach (var pictureTag in pictureTags)
                    {
                        if (dict.ContainsKey(pictureTag))
                        {
                            dict[pictureTag] += 1;
                        }
                        else
                        {
                            dict.Add(pictureTag, 1);
                        }
                    }
                }
            }

            tags.Tags = dict.OrderBy(d => d.Value).Select(t => new TagCollectionElement() { Tag = t.Key, Count = t.Value }).ToList();
            return tags;
        }
    }

    public class TagCollectionElement
    {
        public string Tag { get; set; }

        public int Count { get; set; }
    }
}
