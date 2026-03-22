using DirectoryScanner.Core;

namespace DirectoryScanner.GUI.ViewModels;

public sealed class FileNodeViewModel : ExplorerNodeViewModel
{
    private readonly FileScanInfo _info;

    public FileNodeViewModel(FileScanInfo info, long parentDirectorySize)
        : base(parentDirectorySize)
    {
        _info = info;
    }

    public override string Name => _info.Name;

    public override long SizeBytes => _info.Size;

    public override bool IsFolder => false;

    public override bool IsPendingPlaceholder => false;

    public override bool IsFile => true;

    public bool IsSymlink => _info.IsSymlink;
}
