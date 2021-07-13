using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class TagCollection
    {
        public List<TagCollectionElement> Tags { get; set; }

        public int TagCount
        {
            get
            {
                return Tags.Count;
            }
        }

        public TagCollection()
        {
            Tags = new List<TagCollectionElement>();
        }

        public static TagCollection GetTags(List<Picture> pictures)
        {
            var tags = new TagCollection();
            var dict = new Dictionary<string, int>();

            foreach (var picture in pictures)
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

            tags.Tags = dict.Select(t => new TagCollectionElement() { Tag = t.Key, Count = t.Value }).OrderByDescending(o => o.Count).ThenBy(o => o.Tag).ToList();
            return tags;
        }
    }

    public class TagCollectionElement
    {
        public string Tag { get; set; }

        public int Count { get; set; }
    }
}
