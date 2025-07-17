using RssReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace RssReader.Business
{
    public class FeedParser
    {
        private readonly HttpClient _httpClient;
        
        public FeedParser()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "RSS Reader Application");
        }
        
        public bool IsValidFeedUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
            {
                return false;
            }
            
            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }
            
            return true;
        }
        
        public async Task<List<Article>> ParseFeedAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                
                using (var stringReader = new System.IO.StringReader(response))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    var feed = SyndicationFeed.Load(xmlReader);
                    return feed.Items.Select(item => ConvertToArticle(item)).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error parsing feed: {ex.Message}");
                return new List<Article>();
            }
        }
        
        private Article ConvertToArticle(SyndicationItem item)
        {
            var article = new Article
            {
                Title = item.Title?.Text ?? "Untitled",
                Link = item.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                PublishDate = item.PublishDate.DateTime != DateTime.MinValue ? 
                              item.PublishDate.DateTime : DateTime.Now
            };
            
            // Try to get content
            var content = item.Content as TextSyndicationContent;
            if (content != null)
            {
                article.Content = content.Text;
                article.Summary = TruncateHtml(content.Text, 300);
            }
            else if (item.Summary != null)
            {
                article.Content = item.Summary.Text;
                article.Summary = TruncateHtml(item.Summary.Text, 300);
            }
            
            return article;
        }
        
        private string TruncateHtml(string html, int maxLength)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
                
            // Simple HTML tag removal for summary
            var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "");
            
            if (text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength) + "...";
        }
    }
}
