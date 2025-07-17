// Views/SourcesPanel.xaml.cs
using RssReader.Business;
using RssReader.Models;
using RssReader.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RssReader.Views
{
    public partial class SourcesPanel : UserControl
    {
        private RssManager _rssManager;
        private ObservableCollection<SourceViewModel> _sources;
        
        public event EventHandler<int> SourceSelected;
        
        public SourcesPanel()
        {
            InitializeComponent();
            _sources = new ObservableCollection<SourceViewModel>();
        }
        
        public void Initialize(RssManager rssManager)
        {
            _rssManager = rssManager;
            RefreshSources();
        }
        
        public async void RefreshSources()
        {
            if (_rssManager == null)
                return;
                
            var sources = await _rssManager.GetAllSourcesAsync();
            var unreadArticles = await _rssManager.GetUnreadArticlesAsync();
            
            _sources.Clear();
            
            foreach (var source in sources)
            {
                var unreadCount = unreadArticles.Count(a => a.SourceId == source.Id);
                
                _sources.Add(new SourceViewModel
                {
                    Id = source.Id,
                    Name = source.Name,
                    Url = source.Url,
                    Category = source.Category,
                    LastUpdated = source.LastUpdated,
                    UnreadCount = unreadCount,
                    HasUnread = unreadCount > 0
                });
            }
            
            // Group by category if available
            var view = CollectionViewSource.GetDefaultView(_sources);
            
            if (_sources.Any(s => !string.IsNullOrEmpty(s.Category)))
            {
                view.GroupDescriptions.Clear();
                view.GroupDescriptions.Add(new PropertyGroupDescription("Category", new CategoryConverter()));
                view.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
            else
            {
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
            
            sourceListView.ItemsSource = view;
            
            // Reselect if we had a selection
            if (sourceListView.SelectedItem != null)
            {
                var selectedId = ((SourceViewModel)sourceListView.SelectedItem).Id;
                var selectedSource = _sources.FirstOrDefault(s => s.Id == selectedId);
                if (selectedSource != null)
                {
                    sourceListView.SelectedItem = selectedSource;
                }
            }
        }
        
        public async void RefreshSelectedSource()
        {
            if (sourceListView.SelectedItem is SourceViewModel selectedSource)
            {
                await _rssManager.RefreshFeed(selectedSource.Id);
                RefreshSources();
            }
        }
        
        private void SourceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sourceListView.SelectedItem is SourceViewModel selectedSource)
            {
                SourceSelected?.Invoke(this, selectedSource.Id);
            }
        }
        
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditFeedDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _rssManager.AddSourceAsync(dialog.FeedName, dialog.FeedUrl, dialog.FeedCategory);
                    RefreshSources();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding feed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sourceListView.SelectedItem is SourceViewModel selectedSource)
            {
                var dialog = new AddEditFeedDialog
                {
                    FeedName = selectedSource.Name,
                    FeedUrl = selectedSource.Url,
                    FeedCategory = selectedSource.Category,
                    IsEditMode = true
                };
                
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        await _rssManager.UpdateSourceAsync(
                            selectedSource.Id, 
                            dialog.FeedName, 
                            dialog.FeedUrl, 
                            dialog.FeedCategory);
                            
                        RefreshSources();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating feed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a feed to edit.", "No Feed Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sourceListView.SelectedItem is SourceViewModel selectedSource)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the feed '{selectedSource.Name}'?\n\nThis will also delete all articles from this feed.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                    
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _rssManager.DeleteSourceAsync(selectedSource.Id);
                        RefreshSources();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting feed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a feed to delete.", "No Feed Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
    
    public class SourceViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public DateTime LastUpdated { get; set; }
        public int UnreadCount { get; set; }
        public bool HasUnread { get; set; }
    }
    
    public class CategoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string category = value as string;
            return string.IsNullOrEmpty(category) ? "Uncategorized" : category;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}