using System.Linq;
using DirectoryScanner.Core;

namespace DirectoryScanner.GUI.ViewModels;

public sealed class FolderNodeViewModel : ExplorerNodeViewModel
{
    private readonly DirectoryScanInfo _info;
    private bool _childrenLoaded;

    public FolderNodeViewModel(DirectoryScanInfo info, long parentDirectorySize)
        : base(parentDirectorySize)
    {
        _info = info;
        if (HasExpandableContent)
            Children.Add(new PendingExpandNodeViewModel());
    }

    public override string Name => _info.Name;

    public override long SizeBytes => _info.Size;

    public override bool IsFolder => true;

    public override bool IsPendingPlaceholder => false;

    public bool IsSymlink => _info.IsSymlink;

    public bool HasExpandableContent => _info.DirectoriesCount + _info.FilesCount > 0;

    public void EnsureChildrenLoaded()
    {
        if (_childrenLoaded)
            return;

        _childrenLoaded = true;
        Children.Clear();

        long parentSize = _info.Size;
        foreach (DirectoryScanInfo sub in _info.EnumerateDirectories().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
        {
            Children.Add(new FolderNodeViewModel(sub, parentSize));
        }

        foreach (FileScanInfo file in _info.EnumerateFiles().OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
        {
            Children.Add(new FileNodeViewModel(file, parentSize));
        }
    }
}
