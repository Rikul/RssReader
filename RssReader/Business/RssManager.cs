using RssReader.Data;
using RssReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace RssReader.Business
{
    public class RssManager
    {
        private readonly SourceRepository _sourceRepository;
        private readonly ArticleRepository _articleRepository;
        private readonly FeedParser _feedParser;
        private readonly SettingsRepository _settingsRepository;
        private Timer _refreshTimer;
        
        public event EventHandler<List<Article>> NewArticlesReceived;
        
        public RssManager(DatabaseContext context)
        {
            _sourceRepository = new SourceRepository(context);
            _articleRepository = new ArticleRepository(context);
            _settingsRepository = new SettingsRepository(context);
            _feedParser = new FeedParser();
            
            InitializeRefreshTimer();
        }
        
        private async void InitializeRefreshTimer()
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            _refreshTimer = new Timer(settings.RefreshIntervalMinutes * 60 * 1000);
            _refreshTimer.Elapsed += async (s, e) => await RefreshAllFeeds();
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();
        }
        
        public async Task<Settings> UpdateRefreshInterval(int minutes)
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            settings.RefreshIntervalMinutes = minutes;
            await _settingsRepository.UpdateSettingsAsync(settings);
            
            _refreshTimer.Interval = minutes * 60 * 1000;
            
            return settings;
        }
        
        public async Task<List<Source>> GetAllSourcesAsync()
        {
            return await _sourceRepository.GetAllSourcesAsync();
        }
        
        public async Task<Source> AddSourceAsync(string name, string url, string category = "")
        {
            // Validate URL
            if (!_feedParser.IsValidFeedUrl(url))
            {
                throw new ArgumentException("Invalid feed URL");
            }
            
            var source = new Source
            {
                Name = name,
                Url = url,
                Category = category,
                LastUpdated = DateTime.Now
            };
            
            source = await _sourceRepository.AddSourceAsync(source);
            
            // Fetch initial articles
            await RefreshFeed(source.Id);
            
            return source;
        }
        
        public async Task<Source> UpdateSourceAsync(int id, string name, string url, string category)
        {
            var source = await _sourceRepository.GetSourceByIdAsync(id);
            if (source == null)
            {
                throw new ArgumentException("Source not found");
            }
            
            // Validate URL if changed
            if (source.Url != url && !_feedParser.IsValidFeedUrl(url))
            {
                throw new ArgumentException("Invalid feed URL");
            }
            
            source.Name = name;
            source.Url = url;
            source.Category = category;
            
            await _sourceRepository.UpdateSourceAsync(source);
            
            // Refresh feed if URL changed
            if (source.Url != url)
            {
                await RefreshFeed(source.Id);
            }
            
            return source;
        }
        
        public async Task<bool> DeleteSourceAsync(int id)
        {
            return await _sourceRepository.DeleteSourceAsync(id);
        }
        
        public async Task<List<Article>> GetArticlesBySourceIdAsync(int sourceId)
        {
            return await _articleRepository.GetArticlesBySourceIdAsync(sourceId);
        }
        
        public async Task<List<Article>> GetUnreadArticlesAsync()
        {
            return await _articleRepository.GetUnreadArticlesAsync();
        }
        
        public async Task<List<Article>> GetFavoriteArticlesAsync()
        {
            return await _articleRepository.GetFavoriteArticlesAsync();
        }
        
        public async Task<bool> MarkArticleAsReadAsync(int id)
        {
            return await _articleRepository.MarkArticleAsReadAsync(id);
        }
        
        public async Task<bool> ToggleArticleFavoriteAsync(int id)
        {
            return await _articleRepository.ToggleArticleFavoriteAsync(id);
        }
        
        public async Task<Article> GetArticleByIdAsync(int id)
        {
            return await _articleRepository.GetArticleByIdAsync(id);
        }
        
        public async Task<Source> GetSourceByIdAsync(int id)
        {
            return await _sourceRepository.GetSourceByIdAsync(id);
        }
        
        public async Task<List<Article>> RefreshAllFeeds()
        {
            var sources = await _sourceRepository.GetAllSourcesAsync();
            var newArticles = new List<Article>();
            
            foreach (var source in sources)
            {
                var articles = await RefreshFeed(source.Id);
                newArticles.AddRange(articles);
            }
            
            // Clean up old articles
            var settings = await _settingsRepository.GetSettingsAsync();
            await _articleRepository.DeleteOldArticlesAsync(settings.MaxArticlesPerFeed);
            
            if (newArticles.Count > 0)
            {
                NewArticlesReceived?.Invoke(this, newArticles);
            }
            
            return newArticles;
        }
        
        public async Task<List<Article>> RefreshFeed(int sourceId)
        {
            var source = await _sourceRepository.GetSourceByIdAsync(sourceId);
            if (source == null)
            {
                throw new ArgumentException("Source not found");
            }
            
            var existingArticles = await _articleRepository.GetArticlesBySourceIdAsync(sourceId);
            var existingUrls = new HashSet<string>(existingArticles.Select(a => a.Link));
            
            var parsedArticles = await _feedParser.ParseFeedAsync(source.Url);
            var newArticles = new List<Article>();
            
            foreach (var article in parsedArticles)
            {
                if (!existingUrls.Contains(article.Link))
                {
                    article.SourceId = sourceId;
                    await _articleRepository.AddArticleAsync(article);
                    newArticles.Add(article);
                }
            }
            
            // Update last updated timestamp
            source.LastUpdated = DateTime.Now;
            await _sourceRepository.UpdateSourceAsync(source);
            
            return newArticles;
        }
    }
}
