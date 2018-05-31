using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Newtonsoft.Json.Linq;

namespace jfkfiles.bot.Models
{
    [Serializable]
    public class ImageAnalysisResults
    {
        public ImageAnalysisResults(ImageAnalyzer img)
        {
            this.Image = img;

            if (img.AnalysisResult.Description == null || !img.AnalysisResult.Description.Captions.Any(d => d.Confidence >= 0.2))
            {
                this.Description = "Not sure what that is";
            }
            else
            {
                var confidence = Math.Round( img.AnalysisResult.Description.Captions[0].Confidence *100);
                var desc = img.AnalysisResult.Description.Captions[0].Text;
                string title = string.Empty;
                if (confidence > 95)
                {
                    title = $"I'm a almost certain that this is {desc}";
                }
                else if (confidence > 90)
                {
                    title = $"I'm a pretty sure that this is {desc}";
                }
                else if (confidence < 90 && confidence > 50)
                {
                    title = $"I'm a reasonably confident that this is {desc}";
                }
                else
                {
                    title = $"I'm not sure but at a guess I think this is {desc}";
                }
                this.Description = title;
                this.DescriptionConfidence = confidence;
            }

            var celebNames = this.GetCelebrityNames(img);
            if (celebNames == null || !celebNames.Any())
            {
                this.Celebritynames = string.Empty;
            }
            else
            {
                this.Celebritynames = string.Join(", ", celebNames.OrderBy(name => name));
            }

            if (img.AnalysisResult.Color == null)
            {
                this.Colours = "Not available" ;
            }
            else
            {
                var list = new[]
                {
                    "Dominant background color:" + img.AnalysisResult.Color.DominantColorBackground,
                    "Dominant foreground color:" + img.AnalysisResult.Color.DominantColorForeground,
                    "Dominant colors:" + img.AnalysisResult.Color.DominantColors,
                    "Accent color:" + "#" + img.AnalysisResult.Color.AccentColor
                };
                this.Colours = GetListmarkdown(list);
            }
        }

        private IEnumerable<String> GetCelebrityNames(ImageAnalyzer analyzer)
        {
            if (analyzer.AnalysisResult?.Categories != null)
            {
                foreach (var category in analyzer.AnalysisResult.Categories.Where(c => c.Detail != null))
                {
                    dynamic detail = JObject.Parse(category.Detail.ToString());
                    if (detail.celebrities != null)
                    {
                        foreach (var celebrity in detail.celebrities)
                        {

                            yield return celebrity.name.ToString();
                        }
                    }
                }
            }
        }

        private string GetListmarkdown( IEnumerable<string> list )
        {
            StringBuilder md = new StringBuilder();
            int i = 1;
            foreach( string s in list)
            {
                md.AppendFormat("{1}. {0}", s, i.ToString() );
                i++;
            }
            return md.ToString();
        }

        private string GetMainText(IEnumerable<string> list)
        {
            if (list.Count() > 0)
                return list.First();
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets a list of tags as markdown for display in a card
        /// </summary>
        public string Tags { get; set; }

        public string Colours { get; set; }

        public string Description { get; set; }

        public double DescriptionConfidence { get; set; }

        public string Celebritynames { get; set; }

        public ImageAnalyzer Image { get; set; }
    }

    public class Celebrity
    {
        public faceRectangle faceRectangle { get; set; }
        public string name { get; set; }
        public double confidence { get; set; }
    }

    public class Landmark
    {
        public string name { get; set; }
        public double confidence { get; set; }
    }


    public class faceRectangle
    {
        public int top { get; set; }
        public int left { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}