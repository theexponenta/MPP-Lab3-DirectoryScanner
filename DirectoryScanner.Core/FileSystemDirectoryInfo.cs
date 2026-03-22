namespace DirectoryScanner.Core;

public class FileSystemDirectoryInfo : IDirectoryInfo
{
    private DirectoryInfo directoryInfo;

    public string Name { get { return directoryInfo.Name; } }
    public string? LinkTarget { get { return directoryInfo.LinkTarget; } }

    public FileSystemDirectoryInfo(DirectoryInfo directoryInfo)
    {
        this.directoryInfo = directoryInfo;
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories()
    {
        foreach (DirectoryInfo subidrInfo in directoryInfo.EnumerateDirectories())
        {
            yield return new FileSystemDirectoryInfo(subidrInfo);
        }
    }

    public IEnumerable<IFileInfo> EnumerateFiles()
    {
        foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
        {
            yield return new FileSystemFileInfo(fileInfo);
        }
    }
}
