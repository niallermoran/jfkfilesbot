using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System;

namespace jfkfiles.bot
{
    /// <summary>
    /// Responsible for constructing and issuing Http GET requests for certain API
    /// </summary>
    [Serializable]
    internal sealed class ApiHandler : IApiHandler
    {
        

        public async Task<T> GetJsonAsync<T>(string url, IDictionary<string, string> requestParameters, IDictionary<string, string> headers) where T : class
        {
            string rawResponse = await this.SendRequestAsync(url, requestParameters, headers);

            return JsonConvert.DeserializeObject<T>(rawResponse);
        }

        public async Task<string> GetStringAsync(string url, IDictionary<string, string> requestParameters, IDictionary<string, string> headers)
        {
            return await this.SendRequestAsync(url, requestParameters, headers);
        }

        private async Task<string> SendRequestAsync(string url, IDictionary<string, string> requestParameters, IDictionary<string, string> headers)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            
            string fullUrl = url;

            if (requestParameters != null)
            {
                var requestParams = "?";
                bool first = true;

                foreach (var elem in requestParameters)
                {
                    requestParams += (first == false ? "&" : string.Empty) + $"{elem.Key}={HttpUtility.UrlEncode(elem.Value)}";
                    first = false;
                }

                fullUrl += requestParams;
            }


            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            
            var response = await client.GetAsync(fullUrl);
            var rawResponse = await response.Content.ReadAsStringAsync();

            return rawResponse;
        }
    }
}