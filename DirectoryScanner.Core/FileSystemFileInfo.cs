namespace DirectoryScanner.Core;

public class FileSystemFileInfo : IFileInfo
{
    private FileInfo _fileInfo;

    public long Size { get {return _fileInfo.Length;} }
    public string Name { get { return _fileInfo.Name; } }
    public string? LinkTarget { get { return _fileInfo.LinkTarget; } }

    public FileSystemFileInfo(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }
}
