﻿using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;

namespace TsukiTag.Dependencies
{
    public interface IPictureProviderElement
    {
        string Provider { get; }

        Task<ProviderResult> GetPictures(ProviderFilterElement filter);

        Task<Picture> GetPicture(string id);
    }

    public abstract class OnlinePictureProviderElement : IPictureProviderElement
    {
        public abstract string Provider { get; }

        public abstract bool IsXml { get; }

        public abstract string ConstructUrl(ProviderFilterElement filter);

        public abstract string ConstructIdentifiedUrl(string id);

        public abstract Task<List<Picture>> TransformRawData(ProviderFilterElement? filter, string responseData);

        public virtual string TagSortKeyword => "order";

        public virtual async Task<Picture> GetPicture(string id)
        {
            try
            {
                var url = ConstructIdentifiedUrl(id);
                var client = new RestClient();
                var request = new RestRequest(url, Method.Get) { RequestFormat = IsXml ? DataFormat.Xml : DataFormat.Json };
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                {
                    var pictures = await TransformRawData(null, response.Content);
                    return pictures?.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error occured while getting picture from online provider {Provider} with ID {id}");
            }

            return null;
        }

        public virtual async Task<ProviderResult> GetPictures(ProviderFilterElement filter)
        {
            var result = new ProviderResult()
            {
                Provider = Provider
            };

            try
            {
                var url = ConstructUrl(filter);
                var client = new RestClient();
                var request = new RestRequest(url, Method.Get) { RequestFormat = IsXml ? DataFormat.Xml : DataFormat.Json };
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                {
                    var pictures = await TransformRawData(filter, response.Content);

                    result.Succeeded = true;
                    result.Pictures = pictures.DistinctBy(p => p.Id).DistinctBy(p => p.Md5)?.ToList();
                    result.ProviderEnd = result.Pictures.Count == 0;

                    OnResultProcessed(response, filter, result);
                }
                else
                {
                    result.Succeeded = false;
                    result.ProviderEnd = true;
                    result.ErrorCode = "ToastProviderError";

                    Log.Warning<ProviderFilterElement>($"Non-OK status code {response?.StatusCode} received from provider {Provider} to filter.", filter);

                    OnNonOkResultReceived(response, filter, result);
                }
            }
            catch (Exception ex)
            {
                Log.Error<ProviderFilterElement>(ex, $"Exception occurred during picture retrieval from provider {Provider} to filter.", filter);

                result.Succeeded = false;
                result.ErrorCode = "ToastProviderError";
            }

            return result;
        }

        protected virtual void OnResultProcessed(RestResponse response, ProviderFilterElement filter, ProviderResult result)
        {

        }

        protected virtual void OnNonOkResultReceived(RestResponse response, ProviderFilterElement filter, ProviderResult result)
        {

        }

        protected virtual string HarmonizeTagString(string tagString)
        {
            return tagString?.Replace("sort:", TagSortKeyword + ":").Replace("order:", TagSortKeyword + ":");
        }
    }
}
