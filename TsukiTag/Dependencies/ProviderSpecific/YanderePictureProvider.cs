using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public interface IYanderePictureProvider : IPictureProviderElement
    {

    }

    public class YanderePictureProvider : OnlinePictureProviderElement, IYanderePictureProvider
    {
        private const string BaseUrl = "https://yande.re/post.json";

        public override string Provider => Models.Provider.Yandere.Name;

        public override bool IsXml => false;

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

        public override Task<List<Picture>> TransformRawData(ProviderFilterElement filter, string responseData)
        {
            var pictures = new List<Picture>();

            try
            {
                var pictureObjects = JsonConvert.DeserializeObject<List<JObject>>(responseData);

                foreach (var pobj in pictureObjects)
                {
                    var picture = new YanderePicture();

                    picture.Id = pobj.GetValue("id")?.ToString();
                    picture.ParentId = pobj.GetValue("parent_id")?.ToString();
                    picture.Rating = pobj.GetValue("rating")?.ToString();
                    picture.Tags = pobj.GetValue("tags")?.ToString();
                    picture.Md5 = pobj.GetValue("md5")?.ToString();
                    picture.Source = pobj.GetValue("source")?.ToString();
                    picture.Status = pobj.GetValue("status")?.ToString();
                    picture.Url = pobj.GetValue("sample_url")?.ToString();
                    picture.PreviewUrl = pobj.GetValue("preview_url")?.ToString();
                    picture.DownloadUrl = pobj.GetValue("file_url")?.ToString();
                    picture.CreatedAt = pobj.GetValue("created_at")?.ToString();
                    picture.Author = pobj.GetValue("creator_id")?.ToString();

                    if (int.TryParse(pobj.GetValue("height")?.ToString(), out int h))
                    {
                        picture.Height = h;
                    }

                    if (int.TryParse(pobj.GetValue("width")?.ToString(), out int w))
                    {
                        picture.Width = w;
                    }

                    if (int.TryParse(pobj.GetValue("preview_height")?.ToString(), out int ph))
                    {
                        picture.PreviewHeight = ph;
                    }

                    if (int.TryParse(pobj.GetValue("preview_width")?.ToString(), out int pw))
                    {
                        picture.PreviewWidth = pw;
                    }

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
    }
}
