using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using TsukiTag.Models;
using TsukiTag.Models.ProviderSpecific;

namespace TsukiTag.Dependencies.ProviderSpecific
{
    public interface IGelbooruPictureProvider : IPictureProviderElement
    {

    }

    public class GelbooruPictureProvider : OnlinePictureProviderElement, IGelbooruPictureProvider
    {
        private const string BaseUrl = "https://gelbooru.com/index.php?page=dapi&s=post&q=index";

        public override string Provider => TsukiTag.Models.Provider.Gelbooru.Name;

        public override string TagSortKeyword => "sort";

        public override bool IsXml => true;

        public override string ConstructIdentifiedUrl(string id)
        {
            return $"{BaseUrl}&tags=id:{id}";
        }

        public override string ConstructUrl(ProviderFilterElement filter)
        {
            var url = BaseUrl;

            if (filter.Tags != null && filter.Tags.Count > 0)
            {
                url += $"&tags={HttpUtility.UrlEncode(HarmonizeTagString(filter.TagString))}";
            }

            if (filter.Page > 0)
            {
                url += $"&pid={filter.Page}";
            }

            if (filter.Limit > 0)
            {
                url += $"&limit={filter.Limit}";
            }

            return url;
        }

        public override Task<List<Picture>> TransformRawData(ProviderFilterElement? filter, string responseData)
        {
            var pictures = new List<Picture>();

            dynamic doc = DynamicXml.Parse(responseData);
            IList<dynamic> posts = new List<dynamic>();

            try
            {
                posts = doc.post;
            }
            catch
            {
                dynamic post = doc.post;
                if (post != null)
                {
                    posts.Add(post);
                }
            }

            if (posts != null && posts.Count > 0)
            {
                for (var i = 0; i < posts.Count; i++)
                {
                    var post = posts[i];

                    var picture = new GelbooruPicture();

                    picture.Id = post.id;
                    picture.ParentId = post.parent_id;
                    picture.Rating = post.rating;
                    picture.Tags = post.tags;
                    picture.Md5 = post.md5;                    
                    picture.Source = post.source;
                    picture.Status = post.status;
                    picture.Url = post.sample_url;
                    picture.PreviewUrl = post.preview_url;
                    picture.DownloadUrl = post.file_url;
                    picture.CreatedAt = post.created_at;
                    picture.CreatedBy = post.creator_id;
                    picture.Author = picture.CreatedBy;

                    if (int.TryParse(post.score, out int s))
                    {
                        picture.Score = s;
                    }

                    if (int.TryParse(post.height, out int h))
                    {
                        picture.Height = h;
                    }

                    if (int.TryParse(post.width, out int w))
                    {
                        picture.Width = w;
                    }

                    if (int.TryParse(post.preview_height, out int ph))
                    {
                        picture.PreviewHeight = ph;
                    }

                    if (int.TryParse(post.preview_width, out int pw))
                    {
                        picture.PreviewWidth = pw;
                    }

                    if (!string.IsNullOrEmpty(picture.Md5))
                    {
                        pictures.Add(picture);
                    }
                }
            }

            return Task.FromResult(pictures);
        }
    }
}
