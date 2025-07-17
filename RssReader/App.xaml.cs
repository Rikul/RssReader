// App.xaml.cs
using System;
using System.Windows;
using RssReader.Data; 
using RssReader.Business;

namespace RssReader
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set up exception handling
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
                HandleException((Exception)args.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, args) =>
            {
                HandleException(args.Exception, "Application.Current.DispatcherUnhandledException");
                args.Handled = true;
            };

            // Initialize database and add default feeds
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    if (!context.Database.EnsureCreated())
                        return; // Database already exists

                    // Add default RSS feeds
                    var rssManager = new RssManager(context);
                    rssManager.AddSourceAsync("TechCrunch", "http://feeds.feedburner.com/TechCrunch/", "Technology");
                    rssManager.AddSourceAsync("The Verge", "https://www.theverge.com/rss/index.xml", "Technology");
                    rssManager.AddSourceAsync("Hacker News", "https://news.ycombinator.com/rss", "Technology");

                    context.SaveChanges(); // Save changes to the database
                    rssManager.RefreshAllFeeds(); // Refresh feeds to fetch articles
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "Database Initialization");
            }
        }

        private void HandleException(Exception ex, string source)
        {
            // Log the exception
            try
            {
                string logPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "RssReader",
                    "error.log");

                string logMessage = $"[{DateTime.Now}] {source}: {ex.Message}\n{ex.StackTrace}\n\n";

                System.IO.File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // If can't log, just continue to show error
            }

            // Show error to user
            MessageBox.Show(
                $"An unhandled exception occurred: {ex.Message}\n\nPlease restart the application.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}