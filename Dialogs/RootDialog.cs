namespace jfkfiles.bot
{
    using System.Text;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Configuration;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public static IEnumerable<string> options = new List<string> {
            "Find articles on specific topics",
            "Upload an image and I will do my best to tell you what I see" };

        private static string ConnectionName = ConfigurationManager.AppSettings["AuthenticationConnectionName"];

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.ShowWelcomeMessage);            
        }

        public async virtual Task ShowWelcomeMessage(IDialogContext context, IAwaitable<IMessageActivity> activity )
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Hi there");
            builder.AppendLine("There are a number of things I can help you with, such as:");
            foreach (string s in RootDialog.options)
            {
                builder.AppendLine("* " + s);
            }
            await context.PostAsync(builder.ToString());
            context.Wait(this.MessageReceivedAsync);
        }

        public async virtual Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = (await result);

            // create an intent dialog that uses LUIS to understand what the user wants
            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                // check if an image has been uploaded
                await context.Forward(new AnalyseAttachmentDialog(), ResumeAttachmentAfterDialog, message, System.Threading.CancellationToken.None);
            }
            else if (!string.IsNullOrEmpty(message.Text))
            {
                // create a new dialog for the LUIS interaction
                IntentDialog intdlg = new IntentDialog();
                await context.Forward(intdlg, ResumeIntentDialog, message, CancellationToken.None);
            }

            context.Wait(this.MessageReceivedAsync);

        }


      

        private async Task ResumeIntentDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(this.MessageReceivedAsync);
        }


        private async Task ResumeAttachmentAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(this.MessageReceivedAsync);
        }

    }
}
