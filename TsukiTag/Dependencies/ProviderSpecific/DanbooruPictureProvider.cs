using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TsukiTag.Models;
using TsukiTag.Models.ProviderSpecific;

namespace TsukiTag.Dependencies.ProviderSpecific
{
    public interface IDanbooruPictureProvider : IPictureProviderElement
    {

    }

    public class DanbooruPictureProvider : OnlinePictureProviderElement, IDanbooruPictureProvider
    {
        private const string BaseUrl = "https://danbooru.donmai.us/posts.json";

        public override string Provider => Models.Provider.Danbooru.Name;

        public override bool IsXml => false;

        public override string ConstructIdentifiedUrl(string id)
        {
            return $"{BaseUrl}?tags=id:{id}";
        }

        public override string ConstructUrl(ProviderFilterElement filter)
        {
            var url = BaseUrl;

            url += $"?page={filter.Page + 1}";

            if (filter.Tags != null && filter.Tags.Count > 0)
            {
                url += $"&tags={HttpUtility.UrlEncode(HarmonizeTagString(filter.TagString))}";
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

            try
            {
                var pictureObjects = JsonConvert.DeserializeObject<List<JObject>>(responseData);

                foreach (var pobj in pictureObjects)
                {
                    var picture = new DanbooruPicture();

                    picture.Id = pobj.GetValue("id")?.ToString();
                    picture.ParentId = pobj.GetValue("parent_id")?.ToString();
                    picture.Rating = pobj.GetValue("rating")?.ToString();
                    picture.Tags = pobj.GetValue("tag_string")?.ToString();
                    picture.Md5 = pobj.GetValue("md5")?.ToString();
                    picture.Score = int.Parse(pobj.GetValue("score")?.ToString() ?? "0");
                    picture.Source = pobj.GetValue("source")?.ToString();
                    picture.Status = pobj.GetValue("status")?.ToString();
                    picture.Url = pobj.GetValue("large_file_url")?.ToString();
                    picture.PreviewUrl = pobj.GetValue("preview_file_url")?.ToString();
                    picture.DownloadUrl = pobj.GetValue("file_url")?.ToString();
                    picture.CreatedAt = pobj.GetValue("created_at")?.ToString();
                    picture.CreatedBy = pobj.GetValue("uploader_id")?.ToString();
                    picture.Author = picture.CreatedBy;

                    if (int.TryParse(pobj.GetValue("image_height")?.ToString(), out int h))
                    {
                        picture.Height = h;
                    }

                    if (int.TryParse(pobj.GetValue("image_width")?.ToString(), out int w))
                    {
                        picture.Width = w;
                    }

                    var heightRatio = 150.0 / picture.Height;
                    var widthRatio = 150.0 / picture.Width;
                    var lowerRatio = heightRatio < widthRatio ? heightRatio : widthRatio;

                    picture.PreviewHeight = (int)(picture.Height * lowerRatio);
                    picture.PreviewWidth = (int)(picture.Width * lowerRatio);

                    if (!string.IsNullOrEmpty(picture.Md5))
                    {
                        pictures.Add(picture);
                    }
                }
            }
            catch
            { }

            return Task.FromResult(pictures);
        }

        protected override void OnNonOkResultReceived(IRestResponse response, ProviderFilterElement filter, ProviderResult result)
        {
            if (filter.Tags.Count > 2)
            {
                result.ErrorCode = "ToastProviderTagLimit2";
            }
        }
    }
}
