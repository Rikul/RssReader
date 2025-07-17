using RssReader.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace RssReader.Business
{
    public class ThemeManager
    {
        private Theme _currentTheme;
        
        public event EventHandler ThemeChanged;
        
        public ThemeManager()
        {
            _currentTheme = Theme.Light; // Default theme
        }
        
        public Theme GetCurrentTheme()
        {
            return _currentTheme;
        }
        
        public void ApplyTheme(string themeName)
        {
            _currentTheme = themeName.ToLower() == "dark" ? Theme.Dark : Theme.Light;
            
            var resources = Application.Current.Resources;
            resources["PrimaryColor"] = new SolidColorBrush(_currentTheme.PrimaryColor);
            resources["SecondaryColor"] = new SolidColorBrush(_currentTheme.SecondaryColor);
            resources["BackgroundColor"] = new SolidColorBrush(_currentTheme.BackgroundColor);
            resources["PrimaryTextColor"] = new SolidColorBrush(_currentTheme.PrimaryTextColor);
            resources["SecondaryTextColor"] = new SolidColorBrush(_currentTheme.SecondaryTextColor);
            resources["HighlightColor"] = new SolidColorBrush(_currentTheme.HighlightColor);
            resources["AccentColor"] = new SolidColorBrush(_currentTheme.AccentColor);
            resources["ReadColor"] = new SolidColorBrush(_currentTheme.ReadColor);
            resources["UnreadColor"] = new SolidColorBrush(_currentTheme.UnreadColor);
            resources["FavoriteColor"] = new SolidColorBrush(_currentTheme.FavoriteColor);
            
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}