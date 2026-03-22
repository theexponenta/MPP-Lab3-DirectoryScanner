using Avalonia.Controls;
using Avalonia.Interactivity;
using DirectoryScanner.GUI.ViewModels;

namespace DirectoryScanner.GUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(this);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ScanTree.AddHandler(TreeViewItem.ExpandedEvent, OnTreeItemExpanded, RoutingStrategies.Bubble);
    }

    private static void OnTreeItemExpanded(object? sender, RoutedEventArgs e)
    {
        if (e.Source is TreeViewItem { DataContext: FolderNodeViewModel folder })
            folder.EnsureChildrenLoaded();
    }
}
