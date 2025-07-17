// Views/ArticleListPanel.xaml.cs
using RssReader.Business;
using RssReader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RssReader.Views
{
    public partial class ArticleListPanel : UserControl
    {
        private RssManager _rssManager;
        private ObservableCollection<ArticleViewModel> _articles;
        private int _currentSourceId;
        private string _currentSearchText = "";
        private ArticleFilter _currentFilter = ArticleFilter.All;
        
        public event EventHandler<int> ArticleSelected;
        
        private enum ArticleFilter
        {
            All,
            Unread,
            Favorites
        }
        
        public ArticleListPanel()
        {
            InitializeComponent();
            _articles = new ObservableCollection<ArticleViewModel>();
            articleListView.ItemsSource = _articles;
        }
        
        public void Initialize(RssManager rssManager)
        {
            _rssManager = rssManager;
        }
        
        public async void LoadArticlesBySourceId(int sourceId)
        {
            if (_rssManager == null)
                return;
                
            _currentSourceId = sourceId;
            
            var articles = await _rssManager.GetArticlesBySourceIdAsync(sourceId);
            
            LoadArticlesToView(articles);
            
            // Apply current filter
            ApplyFilter();
        }
        
        public async void ShowAllArticles()
        {
            if (_rssManager == null)
                return;
                
            _currentSourceId = -1;
            
            // Get all articles (we'll limit to the top 200 for performance)
            var sources = await _rssManager.GetAllSourcesAsync();
            var allArticles = new List<Article>();
            
            foreach (var source in sources)
            {
                var articles = await _rssManager.GetArticlesBySourceIdAsync(source.Id);
                allArticles.AddRange(articles);
            }
            
            // Sort by date (newest first) and take the top 200
            allArticles = allArticles
                .OrderByDescending(a => a.PublishDate)
                .Take(200)
                .ToList();
                
            LoadArticlesToView(allArticles);
            
            // Apply current filter
            ApplyFilter();
        }
        
        public async void ShowUnreadArticles()
        {
            if (_rssManager == null)
                return;
                
            _currentSourceId = -2;
            
            var articles = await _rssManager.GetUnreadArticlesAsync();
            
            LoadArticlesToView(articles);
            
            // Apply current filter
            ApplyFilter();
        }
        
        public async void ShowFavoriteArticles()
        {
            if (_rssManager == null)
                return;
                
            _currentSourceId = -3;
            
            var articles = await _rssManager.GetFavoriteArticlesAsync();
            
            LoadArticlesToView(articles);
            
            // Apply current filter
            ApplyFilter();
        }
        
        public async void RefreshArticles()
        {
            switch (_currentSourceId)
            {
                case -1:
                    ShowAllArticles();
                    break;
                case -2:
                    ShowUnreadArticles();
                    break;
                case -3:
                    ShowFavoriteArticles();
                    break;
                default:
                    if (_currentSourceId > 0)
                    {
                        LoadArticlesBySourceId(_currentSourceId);
                    }
                    break;
            }
        }
        
        public async void MarkSelectedAsRead()
        {
            if (articleListView.SelectedItem is ArticleViewModel selectedArticle)
            {
                await _rssManager.MarkArticleAsReadAsync(selectedArticle.Id);
                RefreshArticles();
            }
        }
        
        private void LoadArticlesToView(List<Article> articles)
        {
            _articles.Clear();
            
            foreach (var article in articles)
            {
                _articles.Add(new ArticleViewModel(article));
            }
            
            // Sort by publish date (newest first)
            var view = CollectionViewSource.GetDefaultView(_articles);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("PublishDate", ListSortDirection.Descending));
        }
        
        private void ApplyFilter()
        {
            if (_articles == null)
                return;
                
            var view = CollectionViewSource.GetDefaultView(_articles);
            view.Filter = item =>
            {
                var article = item as ArticleViewModel;
                
                // Apply search filter
                bool matchesSearch = string.IsNullOrEmpty(_currentSearchText) || 
                    article.Title.IndexOf(_currentSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    article.Summary.IndexOf(_currentSearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                
                // Apply status filter
                bool matchesFilter = _currentFilter == ArticleFilter.All ||
                    (_currentFilter == ArticleFilter.Unread && !article.IsRead) ||
                    (_currentFilter == ArticleFilter.Favorites && article.IsFavorite);
                
                return matchesSearch && matchesFilter;
            };
        }
        
        private void ArticleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (articleListView.SelectedItem is ArticleViewModel selectedArticle)
            {
                ArticleSelected?.Invoke(this, selectedArticle.Id);
                
                // Mark as read if not already
                if (!selectedArticle.IsRead)
                {
                    _ = _rssManager.MarkArticleAsReadAsync(selectedArticle.Id);
                    selectedArticle.IsRead = true;
                }
            }
        }
        
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearchText = searchBox.Text;
            ApplyFilter();
        }
        
        private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (filterCombo.SelectedIndex)
            {
                case 0:
                    _currentFilter = ArticleFilter.All;
                    break;
                case 1:
                    _currentFilter = ArticleFilter.Unread;
                    break;
                case 2:
                    _currentFilter = ArticleFilter.Favorites;
                    break;
            }
            
            ApplyFilter();
        }
    }
    
    public class ArticleViewModel : INotifyPropertyChanged
    {
        private readonly Article _article;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public int Id => _article.Id;
        public string Title => _article.Title;
        public string Summary => _article.Summary;
        public DateTime PublishDate => _article.PublishDate;
        public bool IsFavorite => _article.IsFavorite;
        
        private bool _isRead;
        public bool IsRead
        {
            get => _isRead;
            set
            {
                if (_isRead != value)
                {
                    _isRead = value;
                    OnPropertyChanged(nameof(IsRead));
                    OnPropertyChanged(nameof(TitleFontWeight));
                    OnPropertyChanged(nameof(TitleColor));
                }
            }
        }
        
        public string PublishTimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - PublishDate;
                
                if (timeSpan.TotalDays > 365)
                {
                    int years = (int)(timeSpan.TotalDays / 365);
                    return $"{years}y ago";
                }
                if (timeSpan.TotalDays > 30)
                {
                    int months = (int)(timeSpan.TotalDays / 30);
                    return $"{months}mo ago";
                }
                if (timeSpan.TotalDays > 1)
                {
                    return $"{(int)timeSpan.TotalDays}d ago";
                }
                if (timeSpan.TotalHours > 1)
                {
                    return $"{(int)timeSpan.TotalHours}h ago";
                }
                if (timeSpan.TotalMinutes > 1)
                {
                    return $"{(int)timeSpan.TotalMinutes}m ago";
                }
                
                return "Just now";
            }
        }
        
        public FontWeight TitleFontWeight => IsRead ? FontWeights.Normal : FontWeights.SemiBold;
        
        public Brush TitleColor
        {
            get
            {
                if (IsRead)
                {
                    return Application.Current.Resources["ReadColor"] as SolidColorBrush;
                }
                else
                {
                    return Application.Current.Resources["UnreadColor"] as SolidColorBrush;
                }
            }
        }
        
        public ArticleViewModel(Article article)
        {
            _article = article;
            _isRead = article.IsRead;
        }
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}