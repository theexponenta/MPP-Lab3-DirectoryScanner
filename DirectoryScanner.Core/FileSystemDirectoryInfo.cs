namespace DirectoryScanner.Core;

public class FileSystemDirectoryInfo : IDirectoryInfo
{
    private DirectoryInfo _directoryInfo;

    public string Name { get { return _directoryInfo.Name; } }
    public string? LinkTarget { get { return _directoryInfo.LinkTarget; } }

    public FileSystemDirectoryInfo(DirectoryInfo directoryInfo)
    {
        _directoryInfo = directoryInfo;
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories()
    {
        foreach (DirectoryInfo subidrInfo in _directoryInfo.EnumerateDirectories())
        {
            yield return new FileSystemDirectoryInfo(subidrInfo);
        }
    }

    public IEnumerable<IFileInfo> EnumerateFiles()
    {
        foreach (FileInfo fileInfo in _directoryInfo.EnumerateFiles())
        {
            yield return new FileSystemFileInfo(fileInfo);
        }
    }
}
