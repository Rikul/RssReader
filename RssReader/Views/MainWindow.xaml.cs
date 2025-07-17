// Views/MainWindow.xaml.cs
using RssReader.Business;
using RssReader.Data;
using RssReader.Models;
using RssReader.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace RssReader.Views
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseContext _dbContext;
        private readonly RssManager _rssManager;
        private readonly SettingsManager _settingsManager;
        private readonly ThemeManager _themeManager;
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize database context
            _dbContext = new DatabaseContext();
            _dbContext.Database.EnsureCreated();
            
            // Initialize managers
            _rssManager = new RssManager(_dbContext);
            _settingsManager = new SettingsManager(_dbContext);
            _themeManager = new ThemeManager();
            
            // Initialize UI components with managers
            sourcesPanel.Initialize(_rssManager);
            articleListPanel.Initialize(_rssManager);
            articlePanel.Initialize(_rssManager);
            
            // Wire up events
            sourcesPanel.SourceSelected += SourcesPanel_SourceSelected;
            articleListPanel.ArticleSelected += ArticleListPanel_ArticleSelected;
            _rssManager.NewArticlesReceived += RssManager_NewArticlesReceived;
            
            // Load settings and apply theme
            LoadSettings();
            
            // Initialize system tray icon
            InitializeNotifyIcon();
            
            // Initial feed refresh
            RefreshAllFeeds();
            
            // Set window events
            Closing += MainWindow_Closing;
            StateChanged += MainWindow_StateChanged;
        }
        
        private async void LoadSettings()
        {
            var settings = await _settingsManager.LoadSettingsAsync();
            _themeManager.ApplyTheme(settings.ThemeName);
        }
        
        private void InitializeNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/rss_icon.ico")).Stream),
                Visible = true,
                Text = "RSS Reader"
            };
            
            // Create context menu
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            
            var openMenuItem = new System.Windows.Forms.ToolStripMenuItem("Open");
            openMenuItem.Click += (s, e) => 
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            };
            
            var refreshMenuItem = new System.Windows.Forms.ToolStripMenuItem("Refresh All Feeds");
            refreshMenuItem.Click += (s, e) => RefreshAllFeeds();
            
            var exitMenuItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
            exitMenuItem.Click += (s, e) => 
            {
                _notifyIcon.Visible = false;
                Application.Current.Shutdown();
            };
            
            contextMenu.Items.Add(openMenuItem);
            contextMenu.Items.Add(refreshMenuItem);
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);
            
            _notifyIcon.ContextMenuStrip = contextMenu;
            
            _notifyIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            };
        }
        
        #region Event Handlers
        
        private void SourcesPanel_SourceSelected(object sender, int sourceId)
        {
            articleListPanel.LoadArticlesBySourceId(sourceId);
        }
        
        private void ArticleListPanel_ArticleSelected(object sender, int articleId)
        {
            articlePanel.LoadArticle(articleId);
        }
        
        private void RssManager_NewArticlesReceived(object sender, List<Article> newArticles)
        {
            var settings = _settingsManager.GetCurrentSettings();
            
            // Update UI on the UI thread
            Dispatcher.Invoke(() =>
            {
                // Update last updated text
                lastUpdatedText.Text = DateTime.Now.ToString("g");
                
                // Show notification if enabled
                if (settings.NotifyNewArticles && newArticles.Count > 0)
                {
                    _notifyIcon.ShowBalloonTip(
                        5000,
                        "New Articles",
                        $"{newArticles.Count} new article(s) available",
                        System.Windows.Forms.ToolTipIcon.Info
                    );
                }
                
                // Refresh article list if needed
                articleListPanel.RefreshArticles();
            });
        }
        
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var settings = _settingsManager.GetCurrentSettings();
            
            if (settings.MinimizeToTray)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                _notifyIcon.Visible = false;
            }
        }
        
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            var settings = _settingsManager.GetCurrentSettings();
            
            if (settings.MinimizeToTray && WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }
        
        #endregion
        
        #region Menu Actions
        
        private async void AddFeed_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditFeedDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    statusMessage.Text = "Adding feed...";
                    await _rssManager.AddSourceAsync(dialog.FeedName, dialog.FeedUrl, dialog.FeedCategory);
                    sourcesPanel.RefreshSources();
                    statusMessage.Text = "Feed added successfully.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding feed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    statusMessage.Text = "Error adding feed.";
                }
            }
        }
        
        private void ImportOPML_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".opml",
                Filter = "OPML Files (*.opml)|*.opml|XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ImportOpmlFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing OPML: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private async void ImportOpmlFile(string fileName)
        {
            statusMessage.Text = "Importing OPML...";
            
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            
            var outlines = xmlDoc.SelectNodes("//outline");
            int importedCount = 0;
            
            foreach (XmlNode node in outlines)
            {
                var typeAttr = node.Attributes["type"];
                var urlAttr = node.Attributes["xmlUrl"];
                
                if (typeAttr != null && typeAttr.Value == "rss" && urlAttr != null)
                {
                    var title = node.Attributes["title"]?.Value ?? node.Attributes["text"]?.Value ?? "Unnamed Feed";
                    var url = urlAttr.Value;
                    var category = "";
                    
                    try
                    {
                        await _rssManager.AddSourceAsync(title, url, category);
                        importedCount++;
                    }
                    catch (Exception)
                    {
                        // Continue with next feed
                    }
                }
            }
            
            sourcesPanel.RefreshSources();
            statusMessage.Text = $"Imported {importedCount} feeds from OPML.";
            
            if (importedCount > 0)
            {
                MessageBox.Show($"Successfully imported {importedCount} feeds.", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No valid feeds found in the OPML file.", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private async void ExportOPML_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".opml",
                Filter = "OPML Files (*.opml)|*.opml|All Files (*.*)|*.*"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    statusMessage.Text = "Exporting OPML...";
                    
                    var sources = await _rssManager.GetAllSourcesAsync();
                    
                    using (var writer = XmlWriter.Create(dialog.FileName, new XmlWriterSettings { Indent = true }))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("opml");
                        writer.WriteAttributeString("version", "1.0");
                        
                        writer.WriteStartElement("head");
                        writer.WriteElementString("title", "RSS Reader Subscriptions");
                        writer.WriteElementString("dateCreated", DateTime.Now.ToString("r"));
                        writer.WriteEndElement(); // head
                        
                        writer.WriteStartElement("body");
                        
                        foreach (var source in sources)
                        {
                            writer.WriteStartElement("outline");
                            writer.WriteAttributeString("text", source.Name);
                            writer.WriteAttributeString("title", source.Name);
                            writer.WriteAttributeString("type", "rss");
                            writer.WriteAttributeString("xmlUrl", source.Url);
                            writer.WriteEndElement(); // outline
                        }
                        
                        writer.WriteEndElement(); // body
                        writer.WriteEndElement(); // opml
                        writer.WriteEndDocument();
                    }
                    
                    statusMessage.Text = "OPML exported successfully.";
                    MessageBox.Show($"Successfully exported {sources.Count} feeds to OPML.", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting OPML: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    statusMessage.Text = "Error exporting OPML.";
                }
            }
        }
        
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsDialog(_settingsManager, _themeManager);
            dialog.ShowDialog();
            
            // Update refresh timer if interval changed
            var settings = _settingsManager.GetCurrentSettings();
            _rssManager.UpdateRefreshInterval(settings.RefreshIntervalMinutes);
        }
        
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _notifyIcon.Visible = false;
            Application.Current.Shutdown();
        }
        
        private async void MarkAllRead_Click(object sender, RoutedEventArgs e)
        {
            var articles = await _rssManager.GetUnreadArticlesAsync();
            foreach (var article in articles)
            {
                await _rssManager.MarkArticleAsReadAsync(article.Id);
            }
            
            articleListPanel.RefreshArticles();
            statusMessage.Text = $"Marked {articles.Count} articles as read.";
        }
        
        private void MarkSelectedRead_Click(object sender, RoutedEventArgs e)
        {
            articleListPanel.MarkSelectedAsRead();
        }
        
        private async void RefreshAll_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAllFeeds();
        }
        
        private void RefreshSelected_Click(object sender, RoutedEventArgs e)
        {
            sourcesPanel.RefreshSelectedSource();
        }
        
        private void ViewAllArticles_Click(object sender, RoutedEventArgs e)
        {
            articleListPanel.ShowAllArticles();
        }
        
        private void ViewUnreadArticles_Click(object sender, RoutedEventArgs e)
        {
            articleListPanel.ShowUnreadArticles();
        }
        
        private void ViewFavorites_Click(object sender, RoutedEventArgs e)
        {
            articleListPanel.ShowFavoriteArticles();
        }
        
        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            _themeManager.ApplyTheme("Light");
            UpdateSettingsTheme("Light");
        }
        
        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            _themeManager.ApplyTheme("Dark");
            UpdateSettingsTheme("Dark");
        }
        
        private async void UpdateSettingsTheme(string themeName)
        {
            var settings = await _settingsManager.LoadSettingsAsync();
            settings.ThemeName = themeName;
            await _settingsManager.SaveSettingsAsync(settings);
        }
        
        private void About_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.ShowDialog();
        }
        
        #endregion
        
        private async System.Threading.Tasks.Task RefreshAllFeeds()
        {
            statusMessage.Text = "Refreshing feeds...";
            await _rssManager.RefreshAllFeeds();
            lastUpdatedText.Text = DateTime.Now.ToString("g");
            statusMessage.Text = "Feeds refreshed successfully.";
        }
    }
}