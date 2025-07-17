// Views/Dialogs/AddEditFeedDialog.xaml.cs
using System;
using System.Windows;

namespace RssReader.Views.Dialogs
{
    public partial class AddEditFeedDialog : Window
    {
        public string FeedName { get; set; }
        public string FeedUrl { get; set; }
        public string FeedCategory { get; set; }
        public bool IsEditMode { get; set; }
        
        public AddEditFeedDialog()
        {
            InitializeComponent();
            IsEditMode = false;
        }
        
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            
            // Set window title and heading based on mode
            if (IsEditMode)
            {
                this.Title = "Edit Feed";
                dialogTitleText.Text = "Edit Feed";
                
                // Populate fields with existing data
                nameTextBox.Text = FeedName;
                urlTextBox.Text = FeedUrl;
                categoryTextBox.Text = FeedCategory;
            }
            
            // Set focus to first field
            nameTextBox.Focus();
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the feed.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                nameTextBox.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(urlTextBox.Text))
            {
                MessageBox.Show("Please enter a URL for the feed.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                urlTextBox.Focus();
                return;
            }
            
            if (!Uri.TryCreate(urlTextBox.Text, UriKind.Absolute, out Uri uriResult) || 
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show("Please enter a valid URL starting with http:// or https://.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                urlTextBox.Focus();
                return;
            }
            
            // Save values
            FeedName = nameTextBox.Text.Trim();
            FeedUrl = urlTextBox.Text.Trim();
            FeedCategory = categoryTextBox.Text.Trim();
            
            // Close dialog with success
            DialogResult = true;
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog without saving
            DialogResult = false;
        }
    }
}