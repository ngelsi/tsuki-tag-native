﻿using RestSharp;
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
    public interface ISafebooruPictureProvider : IPictureProviderElement
    {

    }

    public class SafebooruPictureProvider : OnlinePictureProviderElement, ISafebooruPictureProvider
    {
        private const string BaseUrl = "https://safebooru.org/index.php?page=dapi&s=post&q=index";

        public override string Provider => TsukiTag.Models.Provider.Safebooru.Name;

        public override string TagSortKeyword => "sort";

        public override bool IsXml => true;

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

        public override Task<List<Picture>> TransformRawData(ProviderFilterElement filter, string responseData)
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

                    var picture = new SafebooruPicture();

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
                    picture.Author = post.creator_id;

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

        protected override void OnResultProcessed(IRestResponse response, ProviderFilterElement filter, ProviderResult result)
        {
            //Sometimes Safebooru responds with an empty response, even when there are no current restrictions present.
            //So we dont mark this specific situation as the provider is finished.

            if(result.ProviderEnd && (filter.Tags == null || filter.Tags.Count == 0))
            {
                result.ProviderEnd = false;
            }            
        }
    }
}
