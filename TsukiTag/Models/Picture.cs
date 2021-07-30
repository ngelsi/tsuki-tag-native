using Avalonia.Media.Imaging;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class Picture : INotifyPropertyChanged
    {
        private string id;
        private string provider;
        private string parentId;
        private int score;
        private string rating;
        private string md5;
        private string createdAt;
        private string createdBy;
        private int width;
        private int height;
        private int previewWidth;
        private int previewHeight;
        private string previewUrl;
        private string url;
        private string downloadUrl;
        private string status;
        private string tags;
        private string userTags;
        private bool hasChildren;
        private bool favorite;
        private bool selected;
        private string extension;
        private string source;
        private Bitmap previewImage;
        private Bitmap sampleImage;
        private bool isLocal;
        private Bitmap sourceImage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Provider
        {
            get { return provider; }
            set { provider = value; }
        }

        public string ParentId
        {
            get { return parentId; }
            set { parentId = value; }
        }

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public string Rating
        {
            get { return rating; }
            set { rating = value; }
        }

        public string Md5
        {
            get { return md5; }
            set { md5 = value; }
        }

        public string CreatedAt
        {
            get { return createdAt; }
            set { createdAt = value; }
        }

        public string CreatedBy
        {
            get { return createdBy; }
            set { createdBy = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public int PreviewWidth
        {
            get { return previewWidth; }
            set { previewWidth = value; }
        }

        public int PreviewHeight
        {
            get { return previewHeight; }
            set { previewHeight = value; }
        }

        public string PreviewUrl
        {
            get { return previewUrl; }
            set { previewUrl = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public string DownloadUrl
        {
            get { return downloadUrl; }
            set { downloadUrl = value; }
        }

        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        public string Tags
        {
            get { return tags?.Trim(); }
            set { tags = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags))); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagList))); }
        }

        public string UserTags
        {
            get { return userTags; }
            set { userTags = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserTags))); }
        }

        public bool HasChildren
        {
            get { return hasChildren; }
            set { hasChildren = value; }
        }

        public bool Favorite
        {
            get { return favorite; }
            set { favorite = value; }
        }

        public bool Selected
        {
            get { return selected; }
            set { selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected))); }
        }

        public string Source
        {
            get { return source; }
            set { source = value; }
        }

        public List<string> TagList
        {
            get
            {
                return this.tags?.Trim().Split(' ').ToList() ?? new List<string>();
            }
        }

        public List<string> UserTagList
        {
            get
            {
                return new string[]
                {
                    $"rating_{this.rating}",
                    $"provider_{this.provider}"
                }.Concat(
                    this.userTags?.Trim().Split(' ').ToList() ?? new List<string>()
                ).ToList();
            }
        }

        public List<string> CompleteTagList
        {
            get
            {
                return TagList.Concat(UserTagList).ToList();
            }
        }

        public string Dimensions
        {
            get
            {
                return $"{width}x{height}";
            }
        }

        public string PreviewDimensions
        {
            get
            {
                return $"{previewWidth}x{previewHeight}";
            }
        }

        [BsonIgnore]
        [JsonIgnore]
        public bool IsLocal
        {
            get { return isLocal; }
            set { isLocal = value; }
        }

        [BsonIgnore]
        [JsonIgnore]
        public Bitmap PreviewImage
        {
            get { return previewImage; }
            set { previewImage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewImage))); }
        }

        [BsonIgnore]
        [JsonIgnore]
        public Bitmap SampleImage
        {
            get { return sampleImage; }
            set { sampleImage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SampleImage))); }
        }

        [BsonIgnore]
        [JsonIgnore]
        public Bitmap SourceImage
        {
            get { return sourceImage; }
            set { sourceImage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceImage))); }
        }

        public string Extension
        {
            get
            {
                if (string.IsNullOrEmpty(extension))
                {
                    var index = this.downloadUrl.LastIndexOf('.');
                    extension = this.downloadUrl.Substring(index + 1, this.downloadUrl.Length - index - 1).ToLower();
                }

                return extension;
            }
        }

        public bool IsMedia
        {
            get { return Extension == "mp4" || Extension == "webm" || Extension == "mov"; }
        }

        public bool IsJpg
        {
            get
            {
                return Extension == "jpg" || Extension == "jpeg";
            }
        }

        public string RatingDisplay
        {
            get
            {
                switch (Rating.ToLower())
                {
                    case "s": return Models.Rating.Safe.DisplayName;
                    case "e": return Models.Rating.Explicit.DisplayName;
                    case "q": return Models.Rating.Questionable.DisplayName;
                }

                return string.Empty;
            }
        }

        public List<string> Metadata
        {
            get
            {
                return new string[]
                {
                    $"PROVIDER: {this.provider}",
                    $"ID: {this.id}",
                    $"MD5: {this.md5}",
                    $"URL: {this.url}",
                    $"SOURCE: {this.source}",
                    $"DOWNLOADURL: {this.downloadUrl}",
                    $"RATING: {this.rating}",
                    $"SCORE: {this.score}"
                }.ToList();
            }
        }

        public void AddUserTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                var list = UserTagList;
                list.Add(tag);

                UserTags = string.Join(' ', list.Distinct());
            }
        }

        public void AddTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                var list = TagList;
                list.Add(tag);

                Tags = string.Join(' ', list.Distinct().OrderBy(s => s));
            }
        }

        public void RemoveUserTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                UserTags = string.Join(' ', UserTagList.Where(t => t != tag).Distinct());
            }
        }

        public void RemoveTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                Tags = string.Join(' ', TagList.Where(t => t != tag).Distinct().OrderBy(s => s));
            }
        }

        public Picture MetadatawiseClone()
        {
            return JsonConvert.DeserializeObject<Picture>(JsonConvert.SerializeObject(this));
        }

        public Picture()
        {
        }

        public override bool Equals(object? obj)
        {
            return (obj as Picture)?.Md5 == Md5;
        }
    }
}
