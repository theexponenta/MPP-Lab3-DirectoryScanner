using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DirectoryScanner.Core;
using DirectoryScanner.Core.Exceptions;
using CoreDirectoryScanner = global::DirectoryScanner.Core.DirectoryScanner;

namespace DirectoryScanner.GUI.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly Window _owner;
    private readonly CoreDirectoryScanner _scanner = new();
    private readonly RelayCommand _browseCommand;
    private readonly RelayCommand _scanCommand;
    private readonly RelayCommand _cancelCommand;

    private string _selectedPath = string.Empty;
    private string _statusMessage = "Выберите каталог и нажмите «Сканировать».";
    private bool _isScanning;
    private int _maxWorkers = Math.Max(1, Environment.ProcessorCount);

    public MainViewModel(Window owner)
    {
        _owner = owner;

        _browseCommand = new RelayCommand(async _ => await PickFolderAsync());
        _scanCommand = new RelayCommand(_ => StartScan(), _ => !_isScanning && !string.IsNullOrWhiteSpace(_selectedPath));
        _cancelCommand = new RelayCommand(_ => _scanner.Cancel(), _ => _isScanning);
    }

    public RelayCommand BrowseCommand => _browseCommand;

    public RelayCommand ScanCommand => _scanCommand;

    public RelayCommand CancelCommand => _cancelCommand;

    public ObservableCollection<ExplorerNodeViewModel> RootNodes { get; } = new();

    public string SelectedPath
    {
        get => _selectedPath;
        set
        {
            if (SetProperty(ref _selectedPath, value))
                _scanCommand.RaiseCanExecuteChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (SetProperty(ref _isScanning, value))
            {
                _scanCommand.RaiseCanExecuteChanged();
                _cancelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int MaxWorkers
    {
        get => _maxWorkers;
        set => SetProperty(ref _maxWorkers, Math.Max(1, value));
    }

    private async Task PickFolderAsync()
    {
        IReadOnlyList<IStorageFolder> folders = await _owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Каталог для сканирования",
            AllowMultiple = false,
        });

        if (folders.Count == 0)
            return;

        Uri uri = folders[0].Path;
        if (uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeFile)
        {
            string path = Uri.UnescapeDataString(uri.LocalPath);
            if (!string.IsNullOrEmpty(path))
                SelectedPath = path;
        }
    }

    private void StartScan()
    {
        string path = _selectedPath.Trim();
        RootNodes.Clear();

        try
        {
            StatusMessage = "Сканирование…";
            IsScanning = true;
            _scanner.Completed -= OnScanCompleted;
            _scanner.Completed += OnScanCompleted;
            _scanner.StartScan(path, _maxWorkers);
        }
        catch (DirectoryDoesNotExistException ex)
        {
            StatusMessage = ex.Message;
            IsScanning = false;
        }
    }

    private void OnScanCompleted(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsScanning = false;
            DirectoryScanInfo? result = _scanner.Result;
            RootNodes.Clear();

            if (result is null)
            {
                StatusMessage = "Результат недоступен.";
                return;
            }

            StatusMessage = "Сканирование завершено.";
            var root = new FolderNodeViewModel(result, result.Size);
            RootNodes.Add(root);
        });
    }
}
