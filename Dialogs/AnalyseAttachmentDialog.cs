namespace jfkfiles.bot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.ProjectOxford.Face.Contract;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;

    [Serializable]
    internal class AnalyseAttachmentDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public async virtual Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                await context.PostAsync($"Analysing the image, please wait ...");

                // get the attchment
                var message = await result;
                var attachment = message.Attachments[0];

                // get the image uploaded
                var bytes = await GetImage(context, message);

                // analyse the image using cognitive vision api
                ImageAnalyzer image = new ImageAnalyzer(bytes);
                image.ShowDialogOnFaceApiErrors = true;
                await image.AnalyzeImageAsync(true);

                // detect faces and text from the image
                await image.DetectFacesAsync(true, true);
                await image.IdentifyFacesAsync();
                await image.RecognizeTextAsync();

                // get the results object
                Models.ImageAnalysisResults results = new Models.ImageAnalysisResults(image);

                if (image.AnalysisResult != null && image.AnalysisResult.Description != null && image.AnalysisResult.Description.Captions.Length > 0)
                {
                    // store this card so we can use the intent dialog to get more information if needed
                 // context.ConversationData.SetValue<Models.ImageAnalysisResults>("imageanalysed", results);

                    // display the results
                    var card = context.MakeMessage();
                    card.Text = results.Description;
                    await DisplayImageFacts(card, results);
                    await context.PostAsync(card);              
                }
                else
                {
                    await context.PostAsync($"I'm sorry but I didn't understand the image. It's a bit embarrasing");
                }
            }
            catch( Exception ex)
            {
                await context.PostAsync($"Oops, somethign went wrong: " + ex.Message);
            }

            context.Wait(this.MessageReceivedAsync);
        }

        /// <summary>
        /// Gets a byte array from an image
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<byte[]> GetImage(IDialogContext context, IMessageActivity message)
        {
            // get the attachment from the message
            if (message.Attachments.Count > 0)
            {
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
                    return await responseMessage.Content.ReadAsByteArrayAsync();
                }
            }

            return null;
        }

        /// <summary>
        /// returns a thumbnail card for displaying the reults of the image analysis
        /// </summary>
        /// <returns></returns>
        private async Task DisplayImageFacts(IMessageActivity message, Models.ImageAnalysisResults image)
        {
            var img = image.Image;

            // create a new adaptive card
            var card = new AdaptiveCards.AdaptiveCard();

            // add a title
            card.Body.Add( new AdaptiveCards.AdaptiveTextBlock( "Facts about the image:" ));

            // create a container for each of the sections
            var container = new AdaptiveCards.AdaptiveContainer();
            container.Separator = true;
            card.Body.Add( container );

            #region Tags
            // create the fact set for the tags
            var tags = new AdaptiveCards.AdaptiveFactSet();

            // create facts for tags
            if (img.AnalysisResult.Tags == null || !img.AnalysisResult.Tags.Any() )
            {
                tags.Facts.Add(new AdaptiveCards.AdaptiveFact("Tags: ", "no tags"));
            }
            else
            {
                var list = img.AnalysisResult.Tags.Select(t => new { Confidence = string.Format("{0}%", Math.Round(t.Confidence * 100)), Name = t.Name });
                foreach( var tag in list)
                {
                    tags.Facts.Add(new AdaptiveCards.AdaptiveFact(tag.Name, tag.Confidence.ToString() ));
                }
            }

            // add the factset
            container.Items.Add(tags);
            #endregion

            #region Celebrities

            // create a fact set for celebrities and landmarks
            var celebs = new AdaptiveCards.AdaptiveFactSet();
          
            // create facts for celebs
            if (img.AnalysisResult?.Categories != null)
            {
                // just making sure the same celebs and landmarks dont get added twice
                List<string> celebnames = new List<string>();
                List<string> landnames = new List<string>();

                foreach (var category in img.AnalysisResult.Categories.Where(c => c.Detail != null))
                {
                    dynamic detail = JObject.Parse(category.Detail.ToString());
                    if (detail.celebrities != null)
                    {
                         foreach (var celebrity in detail.celebrities)
                        {
                            Models.Celebrity celeb = JsonConvert.DeserializeObject<Models.Celebrity>(celebrity.ToString());
                            if (!celebnames.Contains(celeb.name))
                            {
                                celebnames.Add(celeb.name);
                                celebs.Facts.Add(new AdaptiveCards.AdaptiveFact(celeb.name, Math.Round(celeb.confidence * 100).ToString() + "%"));
                            }
                        }
                    }
                    else if (detail.landmarks != null)
                    {
                        foreach (var landmark in detail.landmarks)
                        {
                            Models.Landmark land = JsonConvert.DeserializeObject<Models.Landmark>(landmark.ToString());
                            if (!landnames.Contains(land.name))
                            {
                                landnames.Add(land.name);
                                celebs.Facts.Add(new AdaptiveCards.AdaptiveFact(land.name, Math.Round(land.confidence * 100).ToString() + "%"));
                            }
                        }
                    }
                }
            }

            container.Items.Add(celebs);

            #endregion

            

            // get the attchment
            Attachment att = new Attachment(contentType: AdaptiveCards.AdaptiveCard.ContentType, content: card);

            // add the attchment to the message
            message.Attachments.Add(att);
        }
    }
}
