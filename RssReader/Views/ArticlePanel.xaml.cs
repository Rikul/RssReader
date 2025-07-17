// Views/ArticlePanel.xaml.cs
using RssReader.Business;
using RssReader.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace RssReader.Views
{
    public partial class ArticlePanel : UserControl
    {
        private RssManager _rssManager;
        private int _currentArticleId;
        private string _currentArticleUrl;
        private bool _isFavorite;
        
        public ArticlePanel()
        {
            InitializeComponent();
        }
        
        public void Initialize(RssManager rssManager)
        {
            _rssManager = rssManager;
        }
        
        public async void LoadArticle(int articleId)
        {
            if (_rssManager == null)
                return;
                
            var article = await _rssManager.GetArticleByIdAsync(articleId);
            if (article == null)
                return;
                
            _currentArticleId = articleId;
            _currentArticleUrl = article.Link;
            _isFavorite = article.IsFavorite;
            
            // Update UI
            titleTextBlock.Text = article.Title;
            
            // Get source name
            var source = await _rssManager.GetSourceByIdAsync(article.SourceId);
            sourceTextBlock.Text = source?.Name ?? "Unknown Source";
            
            // Format date
            dateTextBlock.Text = article.PublishDate.ToString("MMMM d, yyyy h:mm tt");
            
            // Update favorite button
            UpdateFavoriteButton();
            
            // Create HTML content with dynamic styles based on the app theme
            var htmlContent = CreateFormattedHtmlContent(article.Content);
            
            // Load content in the web browser
            contentWebBrowser.NavigateToString(htmlContent);
            
            // Show content, hide empty state
            emptyStatePanel.Visibility = Visibility.Collapsed;
        }
        
        private void UpdateFavoriteButton()
        {
            if (_isFavorite)
            {
                favoriteIcon.Foreground = Application.Current.Resources["FavoriteColor"] as SolidColorBrush;
            }
            else
            {
                favoriteIcon.Foreground = Application.Current.Resources["SecondaryTextColor"] as SolidColorBrush;
            }
        }
        
        private string CreateFormattedHtmlContent(string content)
        {
            // Get current theme colors from resources
            var backgroundColor = ((SolidColorBrush)Application.Current.Resources["BackgroundColor"]).Color;
            var textColor = ((SolidColorBrush)Application.Current.Resources["PrimaryTextColor"]).Color;
            var linkColor = ((SolidColorBrush)Application.Current.Resources["AccentColor"]).Color;
            
            // Convert colors to hex
            string bgColorHex = $"#{backgroundColor.R:X2}{backgroundColor.G:X2}{backgroundColor.B:X2}";
            string textColorHex = $"#{textColor.R:X2}{textColor.G:X2}{textColor.B:X2}";
            string linkColorHex = $"#{linkColor.R:X2}{linkColor.G:X2}{linkColor.B:X2}";
            
            // Get font settings from app settings
            var dbContext = new Data.DatabaseContext();
            var settingsRepository = new Data.SettingsRepository(dbContext);
            var settings = settingsRepository.GetSettingsAsync().Result;
            
            // Create HTML with dynamic styles
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{
                        font-family: '{settings.ContentFontFamily}', sans-serif;
                        font-size: {settings.ContentFontSize}px;
                        color: {textColorHex};
                        background-color: {bgColorHex};
                        margin: 0;
                        padding: 0;
                        line-height: 1.5;
                    }}
                    h1, h2, h3, h4, h5, h6 {{
                        font-family: '{settings.TitleFontFamily}', sans-serif;
                        margin-top: 1.2em;
                        margin-bottom: 0.5em;
                    }}
                    a {{
                        color: {linkColorHex};
                        text-decoration: none;
                    }}
                    a:hover {{
                        text-decoration: underline;
                    }}
                    img {{
                        max-width: 100%;
                        height: auto;
                        {(settings.ShowImages ? "" : "display: none;")}
                    }}
                    pre, code {{
                        background-color: rgba(0, 0, 0, 0.05);
                        border-radius: 3px;
                        padding: 2px 4px;
                    }}
                    blockquote {{
                        border-left: 4px solid rgba(0, 0, 0, 0.1);
                        margin-left: 1em;
                        padding-left: 1em;
                        font-style: italic;
                    }}
                </style>
            </head>
            <body>
                {content ?? "No content available for this article."}
            </body>
            </html>";
            
            return html;
        }
        
        private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_rssManager == null || _currentArticleId <= 0)
                return;
                
            // Toggle favorite status
            await _rssManager.ToggleArticleFavoriteAsync(_currentArticleId);
            
            // Update local state
            _isFavorite = !_isFavorite;
            UpdateFavoriteButton();
        }
        
        private void OpenInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentArticleUrl))
                return;
                
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _currentArticleUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening article in browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ContentWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            // Remove focus from WebBrowser to avoid focus issues
            contentWebBrowser.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
        }
    }
}