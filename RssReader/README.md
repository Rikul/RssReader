# RSS Reader for Windows

A modern Windows desktop application that allows users to subscribe to RSS feeds and read articles in a clean, customizable interface.

## Features

- **Three-pane layout**:
  - Left pane: List of RSS sources (categorized)
  - Right top pane: List of articles from the selected source
  - Right bottom pane: Article content viewer

- **Feed Management**:
  - Add, edit, and delete RSS feed sources
  - Import/Export OPML files
  - Organize feeds with categories
  - Automatically refresh feeds at configurable intervals

- **Article Management**:
  - Mark articles as read/unread
  - Mark articles as favorites
  - Filter articles (all, unread, favorites)
  - Search article content
  - Open articles in external browser

- **Customization**:
  - Light and dark themes
  - Customizable fonts and sizes
  - Control image display in articles

- **Application Settings**:
  - Start with Windows
  - Minimize to system tray
  - Control notification behavior
  - Configure article storage settings

## Project Structure

The application follows a multi-layer architecture:

### Model Layer
- `Source.cs`: Represents an RSS feed source
- `Article.cs`: Represents an individual article
- `Settings.cs`: User preferences and application settings
- `Theme.cs`: Theme definitions and color schemes

### Data Layer
- `DatabaseContext.cs`: Entity Framework Core context for SQLite database
- `SourceRepository.cs`: CRUD operations for feed sources
- `ArticleRepository.cs`: CRUD operations for articles
- `SettingsRepository.cs`: Settings storage and retrieval

### Business Logic Layer
- `RssManager.cs`: Core feed management and article operations
- `FeedParser.cs`: RSS/Atom feed parsing
- `SettingsManager.cs`: Settings management and application
- `ThemeManager.cs`: Theme management and application

### Presentation Layer
- `MainWindow.xaml`: Primary application window and layout
- `SourcesPanel.xaml`: Feed sources list component
- `ArticleListPanel.xaml`: Article list component
- `ArticlePanel.xaml`: Article content viewer
- Dialog windows for settings and feed management

## Technical Details

- Built with C# and WPF
- Uses Entity Framework Core with SQLite for local storage
- Implements MVVM pattern for UI components
- Uses System.ServiceModel.Syndication for RSS parsing

## Requirements

- Windows 10 or later
- .NET 6.0 or higher
- Internet connection for fetching feeds

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Build and run the application
4. Add your first RSS feed using the "Add Feed" button

## License

This project is licensed under the MIT License - see the LICENSE file for details.