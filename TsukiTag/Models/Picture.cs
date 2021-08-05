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
        private string author;
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
        private bool selected;
        private string extension;
        private string source;
        private Bitmap previewImage;
        private Bitmap sampleImage;
        private bool isLocal;
        private Bitmap sourceImage;
        private string fileUrl;
        private string localProvider;
        private string localProviderType;
        private Guid? localProviderId;
        private bool isLocallyImported;
        private string title;
        private string description;
        private string copyright;
        private string notes;
        private bool isWorkspace;
        private bool isOnline;
        private bool isMetadataClone;
        private string createdBy;
        private bool isOnlineList;

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

        public string CreatedBy
        {
            get { return createdBy; }
            set { createdBy = value; }
        }

        public string CreatedAt
        {
            get { return createdAt; }
            set { createdAt = value; }
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

        public bool HasChildren
        {
            get { return hasChildren; }
            set { hasChildren = value; }
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

        public string Author
        {
            get { return author; }
            set { author = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Author))); }
        }

        public string Title
        {
            get { return title; }
            set { title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title))); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description))); }
        }

        public string Copyright
        {
            get { return copyright; }
            set { copyright = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Copyright))); }
        }

        public string Notes
        {
            get { return notes; }
            set { notes = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes))); }
        }

        public List<string> TagList
        {
            get
            {
                return this.tags?.Trim().Split(' ').ToList() ?? new List<string>();
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

        public bool IsLocallyImported
        {
            get { return isLocallyImported; }
            set { isLocallyImported = value; }
        }

        [BsonIgnore]
        public string FileUrl
        {
            get { return fileUrl; }
            set { fileUrl = value; }
        }

        [BsonIgnore]
        public bool IsLocal
        {
            get { return isLocal; }
            set { isLocal = value; }
        }

        [BsonIgnore]
        public bool IsWorkspace
        {
            get { return isWorkspace; }
            set { isWorkspace = value; }
        }

        [BsonIgnore]
        public bool IsOnlineList
        {
            get { return isOnlineList; }
            set { isOnlineList = value; }
        }

        [BsonIgnore]
        public bool IsOnline
        {
            get { return isOnline; }
            set { isOnline = value; }
        }

        [BsonIgnore]
        public bool IsMetadataClone
        {
            get { return isMetadataClone; }
            set { isMetadataClone = value; }
        }

        [BsonIgnore]
        public string LocalProvider
        {
            get { return localProvider; }
            set { localProvider = value; }
        }

        [BsonIgnore]
        public Guid? LocalProviderId
        {
            get { return localProviderId; }
            set { localProviderId = value; }
        }

        [BsonIgnore]
        public string LocalProviderType
        {
            get { return localProviderType; }
            set { localProviderType = value; }
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
                    var url = (this.downloadUrl ?? this.fileUrl);
                    var index = url.LastIndexOf('.');

                    extension = url.Substring(index + 1, url.Length - index - 1).ToLower();
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
                    case "u": return Models.Rating.Unknown.DisplayName;
                }

                return Models.Rating.Unknown.DisplayName;
            }
        }

        public void OverrideExtension(string extension)
        {
            this.extension = extension;
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

        public void RemoveTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                Tags = string.Join(' ', TagList.Where(t => t != tag).Distinct().OrderBy(s => s));
            }
        }

        public Picture MetadatawiseClone()
        {
            var picture = JsonConvert.DeserializeObject<Picture>(JsonConvert.SerializeObject(this));
            picture.IsMetadataClone = true;

            return picture;
        }

        public Picture()
        {
        }

        ~Picture()
        {
            if (!IsMetadataClone)
            {
                RemovePictureBitmaps(true);
            }

            if (PropertyChanged != null)
            {
                foreach (var p in PropertyChanged.GetInvocationList())
                {
                    PropertyChanged -= (p as PropertyChangedEventHandler);
                }
            }
        }

        public void RemovePictureBitmaps(bool includePreviewImage = false)
        {
            SourceImage?.Dispose();
            SourceImage = null;

            SampleImage?.Dispose();
            SampleImage = null;

            if (includePreviewImage)
            {
                PreviewImage?.Dispose();
                PreviewImage = null;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is Picture picture)
            {
                return picture.Md5 == Md5 && picture.Id == Id && picture.LocalProviderId == LocalProviderId;
            }

            return false;
        }
    }
}
