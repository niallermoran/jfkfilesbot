using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Text;

namespace jfkfiles.bot
{
    /// <summary>
    /// The top-level natural language dialog for sample.
    /// </summary>
    [Serializable]
    // [LuisModel("luis.ai replace with application ID", "luis.ai publish app and get subscription-key from URL")] 
    // create another partial IntentDialog with this attribute set storing your secrets and add to your gitignore so secrets don;t get committed
    internal sealed partial class IntentDialog : LuisDialog<object>
    {
        private readonly BingSearchService bingSearchService;

        public IntentDialog()
        {
            bingSearchService = new BingSearchService();

        }

        public async Task PostHelp(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
           
        }

        [LuisIntent(JFKFilesBOTStrings.GreetingIntentName)]
        public async Task GreetingIntentHandlerAsync(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Hi");
            builder.AppendLine("---");
            builder.AppendLine("There are a number of things I can help you with, such as:");
            builder.AppendLine("");
            builder.AppendLine("* Find articles on specific topics by asking for information");
            builder.AppendLine("* Upload an image and I will do my best to tell you what I see");
            //builder.AppendLine("* I can even translate text for you");
            await context.PostAsync(builder.ToString());
        }

        [LuisIntent(JFKFilesBOTStrings.GratitudeIntentName)]
        public async Task GratitudeIntentHandlerAsync(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("You are very welcome!");
        }

        [LuisIntent(JFKFilesBOTStrings.ConfidenceIntentName)]
        public async Task ConfidenceIntentHandlerAsync(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            if (context.ConversationData.ContainsKey("confidence"))
            {
                double confidence = context.ConversationData.GetValue<double>("confidence");
                var rounded = Math.Round(confidence * 100);
                await context.PostAsync($"I am " + rounded.ToString() + "% confident");
            }
        }

        [LuisIntent(JFKFilesBOTStrings.SearchIntentName)]
        public async Task FindArticlesIntentHandlerAsync(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            EntityRecommendation entityRecommendation = null;

            // get the entity from the query
            var query = result.TryFindEntity(JFKFilesBOTStrings.ArticlesEntityTopic, out entityRecommendation) ? entityRecommendation.Entity : result.Query;
                        
            // let's do a bing search
            var bingSearch = await this.bingSearchService.FindArticles(query);

            // check for any errors
            if( bingSearch.statusCode != null && bingSearch.statusCode != "200")
            {
                await context.PostAsync( string.Format( "Sorry something went wrong: {0}", bingSearch.message));
            }
            else if ( bingSearch.webPages != null && bingSearch.webPages.value.Length > 0)
            {
                // get the result
                var searchresult = this.PrepareSearchResult(query, bingSearch.webPages.value[0]);

                // build the markdown text
                var summaryText = $"### [{searchresult.Tile}]({searchresult.Url})\n{searchresult.Snippet}\n\n";
                summaryText += $"*{string.Format(Strings.PowerBy, $"[Bing™](https://www.bing.com/search/?q={searchresult.Query} site:wikipedia.org)")}*";

                // post result
                await context.PostAsync(string.Format(Strings.SearchTopicTypeMessage));
                await context.PostAsync(summaryText);

            }
            else
            {
                await context.PostAsync("Sorry something went wrong!");
            }
           


        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task FallbackIntentHandlerAsync(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(string.Format(Strings.FallbackIntentMessage));
            context.Wait(this.MessageReceived);
        }

        private SearchResult PrepareSearchResult(string query, Value page)
        {
            string url;
            var myUri = new Uri(page.url);

            if (myUri.Host == "www.bing.com" && myUri.AbsolutePath == "/cr")
            {
                url = System.Web.HttpUtility.ParseQueryString(myUri.Query).Get("r");
            }
            else
            {
                url = page.url;
            }

            var zummerResult = new SearchResult
            {
                Url = url,
                Query = query,
                Tile = page.name,
                Snippet = page.snippet
            };

            return zummerResult;
        }
    }
}