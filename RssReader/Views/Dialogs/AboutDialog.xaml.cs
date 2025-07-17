// Views/Dialogs/AboutDialog.xaml.cs
using System.Windows;

namespace RssReader.Views.Dialogs
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }
        
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}