using System.Windows.Media;

namespace RssReader.Models
{
    public class Theme
    {
        public string Name { get; set; }
        
        // Main colors
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public Color BackgroundColor { get; set; }
        
        // Text colors
        public Color PrimaryTextColor { get; set; }
        public Color SecondaryTextColor { get; set; }
        
        // Highlight colors
        public Color HighlightColor { get; set; }
        public Color AccentColor { get; set; }
        
        // Status colors
        public Color ReadColor { get; set; }
        public Color UnreadColor { get; set; }
        public Color FavoriteColor { get; set; }
        
        public static Theme Light => new Theme
        {
            Name = "Light",
            PrimaryColor = (Color)ColorConverter.ConvertFromString("#FFFFFF"),
            SecondaryColor = (Color)ColorConverter.ConvertFromString("#F5F5F5"),
            BackgroundColor = (Color)ColorConverter.ConvertFromString("#FFFFFF"),
            PrimaryTextColor = (Color)ColorConverter.ConvertFromString("#333333"),
            SecondaryTextColor = (Color)ColorConverter.ConvertFromString("#666666"),
            HighlightColor = (Color)ColorConverter.ConvertFromString("#E0E0E0"),
            AccentColor = (Color)ColorConverter.ConvertFromString("#2196F3"),
            ReadColor = (Color)ColorConverter.ConvertFromString("#9E9E9E"),
            UnreadColor = (Color)ColorConverter.ConvertFromString("#000000"),
            FavoriteColor = (Color)ColorConverter.ConvertFromString("#FFD700")
        };
        
        public static Theme Dark => new Theme
        {
            Name = "Dark",
            PrimaryColor = (Color)ColorConverter.ConvertFromString("#212121"),
            SecondaryColor = (Color)ColorConverter.ConvertFromString("#333333"),
            BackgroundColor = (Color)ColorConverter.ConvertFromString("#121212"),
            PrimaryTextColor = (Color)ColorConverter.ConvertFromString("#FFFFFF"),
            SecondaryTextColor = (Color)ColorConverter.ConvertFromString("#BBBBBB"),
            HighlightColor = (Color)ColorConverter.ConvertFromString("#424242"),
            AccentColor = (Color)ColorConverter.ConvertFromString("#2196F3"),
            ReadColor = (Color)ColorConverter.ConvertFromString("#9E9E9E"),
            UnreadColor = (Color)ColorConverter.ConvertFromString("#FFFFFF"),
            FavoriteColor = (Color)ColorConverter.ConvertFromString("#FFD700")
        };
    }
}