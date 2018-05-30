using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Net.Http.Headers;
using System.Configuration;

namespace jfkfiles.bot
{
    [BotAuthentication( )]
    public class MessagesController : ApiController
    {
        private static string ConnectionName = ConfigurationManager.AppSettings["AuthenticationConnectionName"];

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity.Type == ActivityTypes.Message || activity.Type == ActivityTypes.ConversationUpdate )
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
            else if (message.Type == ActivityTypes.Event)
            {
            }
            return null;
        }
    }
}