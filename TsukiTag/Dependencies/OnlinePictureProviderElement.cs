using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Extensions;
using TsukiTag.Models;

namespace TsukiTag.Dependencies
{
    public interface IOnlinePictureProviderElement
    {
        string Provider { get; }

        Task<ProviderResult> GetPictures(ProviderFilterElement filter);
    }

    public abstract class OnlinePictureProviderElement : IOnlinePictureProviderElement
    {
        public abstract string Provider { get; }

        public abstract bool IsXml { get; }

        public abstract string ConstructUrl(ProviderFilterElement filter);

        public abstract Task<List<Picture>> TransformRawData(ProviderFilterElement filter, string responseData);

        public virtual async Task<ProviderResult> GetPictures(ProviderFilterElement filter)
        {
            var result = new ProviderResult()
            {
                Provider = Provider
            };

            try
            {
                var url = ConstructUrl(filter);
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET) { RequestFormat = IsXml ? DataFormat.Xml : DataFormat.Json };
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                {
                    var pictures = await TransformRawData(filter, response.Content);

                    result.Succeeded = true;
                    result.ProviderEnd = pictures.Count == 0;
                    result.Pictures = pictures.DistinctBy(p => p.Id).DistinctBy(p => p.Md5)?.ToList();
                }
            }
            catch (Exception ex)
            {
                result.Succeeded = false;
                result.ErrorCode = "search.httperror";
            }

            return result;
        }
    }
}
