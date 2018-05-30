    using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using System;

namespace jfkfiles.bot
{
    /// <summary>
    /// Responsible for calling Bing Web Search API
    /// </summary>
    [Serializable]
    internal sealed class BingSearchService : ISearchService
    {
        private const string BingSearchEndpoint = "https://api.cognitive.microsoft.com/bing/v7.0/search/";

        private static readonly Dictionary<string, string> Headers = new Dictionary<string, string>
        {
            { "Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["BingSearchKey"].ToString() }
        };

        private readonly ApiHandler apiHandler;

        public BingSearchService()
        {
            apiHandler = new ApiHandler();
        }

        public async Task<BingSearch> FindArticles(string query)
        {
            var requestParameters = new Dictionary<string, string>
            {
                { "q", $"{query} site:wikipedia.org" },
                { "form", "BTCSWR" }
            };

            return await this.apiHandler.GetJsonAsync<BingSearch>(BingSearchEndpoint, requestParameters, Headers);
        }
    }
}