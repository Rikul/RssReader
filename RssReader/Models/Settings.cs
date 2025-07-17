using System.ComponentModel.DataAnnotations;

namespace RssReader.Models
{
    public class Settings
    {
        [Key]
        public int Id { get; set; }
        
        // Font settings
        public string ContentFontFamily { get; set; } = "Segoe UI";
        public int ContentFontSize { get; set; } = 12;
        public string TitleFontFamily { get; set; } = "Segoe UI Semibold";
        public int TitleFontSize { get; set; } = 14;
        
        // Display settings
        public bool ShowImages { get; set; } = true;
        public int MaxArticlesPerFeed { get; set; } = 100;
        public int RefreshIntervalMinutes { get; set; } = 60;
        
        // Theme settings
        public string ThemeName { get; set; } = "Light";
        
        // Behavior settings
        public bool MarkReadOnView { get; set; } = true;
        public bool NotifyNewArticles { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
    }
}