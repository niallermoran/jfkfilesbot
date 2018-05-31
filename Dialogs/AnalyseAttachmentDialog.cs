namespace jfkfiles.bot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.ProjectOxford.Face.Contract;
    using Newtonsoft.Json.Linq;

    [Serializable]
    internal class AnalyseAttachmentDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public async virtual Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            var message = await result;
            var attachment = message.Attachments[0];
            using (HttpClient httpClient = new HttpClient())
            {
                // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                if ((message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) || message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
                    && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                {
                    var token = await new MicrosoftAppCredentials().GetTokenAsync();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;

                var bytes = await responseMessage.Content.ReadAsByteArrayAsync();

                ImageAnalyzer image = new ImageAnalyzer(bytes);
                image.ShowDialogOnFaceApiErrors = true;
                await image.AnalyzeImageAsync(true);

                if (image.AnalysisResult != null && image.AnalysisResult.Description != null && image.AnalysisResult.Description.Captions.Length > 0)
                {
                    var confidence = image.AnalysisResult.Description.Captions[0].Confidence;
                    var desc = image.AnalysisResult.Description.Captions[0].Text;
                    if (confidence > 0.95)
                    {
                        await context.PostAsync($"I'm a almost certain that this is {desc}");
                    }
                    else if (confidence > 0.9)
                    {
                        await context.PostAsync($"I'm a pretty sure that this is {desc}");
                    }
                    else if (confidence < 0.9 && confidence > 0.5)
                    {
                        await context.PostAsync($"I'm a reasonably confident that this is {desc}");
                    }
                    else
                    {
                        await context.PostAsync($"I'm not sure but at a guess I think this is {desc}");
                    }

                    context.ConversationData.SetValue<double>("confidence", confidence);

                    //// see if there were faces
                    //await image.DetectFacesAsync(true, true);
                    //await image.IdentifyFacesAsync();
                    //await image.RecognizeTextAsync();

                    //foreach (Face face in image.DetectedFaces)
                    //{
                    //    // Get the border for the associated face id
                    //    string gender = face.FaceAttributes.Gender;
                    //    double age = face.FaceAttributes.Age;

                    //    await context.PostAsync($"Face found: " + gender.ToString() + ", age: " + age.ToString());
                    //}

                }
                else
                {
                    await context.PostAsync($"I'm sorry but I didn't understand the image. It's a bit embarrasing");
                }
            }

            context.Wait(this.MessageReceivedAsync);

        }
     
    }
}
