// Views/Dialogs/SettingsDialog.xaml.cs
using RssReader.Business;
using RssReader.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace RssReader.Views.Dialogs
{
    public partial class SettingsDialog : Window
    {
        private readonly SettingsManager _settingsManager;
        private readonly ThemeManager _themeManager;
        private Settings _currentSettings;
        
        public SettingsDialog(SettingsManager settingsManager, ThemeManager themeManager)
        {
            InitializeComponent();
            
            _settingsManager = settingsManager;
            _themeManager = themeManager;
            
            // Load current settings
            _currentSettings = _settingsManager.GetCurrentSettings();
            
            // Initialize UI controls
            InitializeControls();
            
            // Set initial values
            LoadSettings();
            
            // Set up event handlers for preview
            contentFontCombo.SelectionChanged += (s, e) => UpdatePreview();
            contentFontSizeCombo.SelectionChanged += (s, e) => UpdatePreview();
            titleFontCombo.SelectionChanged += (s, e) => UpdatePreview();
            titleFontSizeCombo.SelectionChanged += (s, e) => UpdatePreview();
        }
        
        private void InitializeControls()
        {
            // Initialize font family combos
            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            foreach (var family in fontFamilies)
            {
                contentFontCombo.Items.Add(family.Source);
                titleFontCombo.Items.Add(family.Source);
            }
            
            // Initialize font size combos
            int[] fontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36 };
            foreach (var size in fontSizes)
            {
                contentFontSizeCombo.Items.Add(size);
                titleFontSizeCombo.Items.Add(size);
            }
            
            // Initialize refresh interval combo
            int[] refreshIntervals = { 5, 10, 15, 30, 60, 120, 180, 240, 360, 720, 1440 };
            foreach (var interval in refreshIntervals)
            {
                refreshIntervalCombo.Items.Add(interval);
            }
            
            // Initialize max articles combo
            int[] maxArticleOptions = { 10, 25, 50, 100, 200, 500, 1000 };
            foreach (var count in maxArticleOptions)
            {
                maxArticlesCombo.Items.Add(count);
            }
        }
        
        private void LoadSettings()
        {
            // Theme settings
            if (_currentSettings.ThemeName.ToLower() == "dark")
            {
                darkThemeRadio.IsChecked = true;
            }
            else
            {
                lightThemeRadio.IsChecked = true;
            }
            
            // Font settings
            contentFontCombo.SelectedItem = _currentSettings.ContentFontFamily;
            titleFontCombo.SelectedItem = _currentSettings.TitleFontFamily;
            
            contentFontSizeCombo.SelectedItem = _currentSettings.ContentFontSize;
            titleFontSizeCombo.SelectedItem = _currentSettings.TitleFontSize;
            
            // Display settings
            showImagesCheck.IsChecked = _currentSettings.ShowImages;
            
            // Refresh interval
            int refreshInterval = _currentSettings.RefreshIntervalMinutes;
            if (!refreshIntervalCombo.Items.Contains(refreshInterval))
            {
                refreshIntervalCombo.Items.Add(refreshInterval);
            }
            refreshIntervalCombo.SelectedItem = refreshInterval;
            
            // Max articles
            int maxArticles = _currentSettings.MaxArticlesPerFeed;
            if (!maxArticlesCombo.Items.Contains(maxArticles))
            {
                maxArticlesCombo.Items.Add(maxArticles);
            }
            maxArticlesCombo.SelectedItem = maxArticles;
            
            // Reading settings
            markReadOnViewCheck.IsChecked = _currentSettings.MarkReadOnView;
            notifyNewArticlesCheck.IsChecked = _currentSettings.NotifyNewArticles;
            
            // Application settings
            startWithWindowsCheck.IsChecked = _currentSettings.StartWithWindows;
            minimizeToTrayCheck.IsChecked = _currentSettings.MinimizeToTray;
            
            // Update preview
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            // Update title preview
            if (titleFontCombo.SelectedItem is string titleFont && titleFontSizeCombo.SelectedItem is int titleSize)
            {
                previewTitleText.FontFamily = new FontFamily(titleFont);
                previewTitleText.FontSize = titleSize;
            }
            
            // Update content preview
            if (contentFontCombo.SelectedItem is string contentFont && contentFontSizeCombo.SelectedItem is int contentSize)
            {
                previewContentText.FontFamily = new FontFamily(contentFont);
                previewContentText.FontSize = contentSize;
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update settings object
                
                // Theme
                _currentSettings.ThemeName = darkThemeRadio.IsChecked == true ? "Dark" : "Light";
                
                // Fonts
                if (contentFontCombo.SelectedItem is string contentFont)
                {
                    _currentSettings.ContentFontFamily = contentFont;
                }
                
                if (titleFontCombo.SelectedItem is string titleFont)
                {
                    _currentSettings.TitleFontFamily = titleFont;
                }
                
                if (contentFontSizeCombo.SelectedItem is int contentSize)
                {
                    _currentSettings.ContentFontSize = contentSize;
                }
                
                if (titleFontSizeCombo.SelectedItem is int titleSize)
                {
                    _currentSettings.TitleFontSize = titleSize;
                }
                
                // Display settings
                _currentSettings.ShowImages = showImagesCheck.IsChecked ?? true;
                
                // Refresh settings
                if (refreshIntervalCombo.SelectedItem is int refreshInterval)
                {
                    _currentSettings.RefreshIntervalMinutes = refreshInterval;
                }
                
                // Article storage
                if (maxArticlesCombo.SelectedItem is int maxArticles)
                {
                    _currentSettings.MaxArticlesPerFeed = maxArticles;
                }
                
                // Reading settings
                _currentSettings.MarkReadOnView = markReadOnViewCheck.IsChecked ?? true;
                _currentSettings.NotifyNewArticles = notifyNewArticlesCheck.IsChecked ?? true;
                
                // Application settings
                _currentSettings.StartWithWindows = startWithWindowsCheck.IsChecked ?? false;
                _currentSettings.MinimizeToTray = minimizeToTrayCheck.IsChecked ?? true;
                
                // Save settings
                _settingsManager.SaveSettingsAsync(_currentSettings);
                
                // Apply theme
                _themeManager.ApplyTheme(_currentSettings.ThemeName);
                
                // Close dialog
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog without saving
            DialogResult = false;
        }
    }
}