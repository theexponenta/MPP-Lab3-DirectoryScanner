namespace DirectoryScanner.Core;

public class FileSystemFileInfo : IFileInfo
{
    private FileInfo fileInfo;

    public long Size { get {return fileInfo.Length;} }
    public string Name { get { return fileInfo.Name; } }
    public string? LinkTarget { get { return fileInfo.LinkTarget; } }

    public FileSystemFileInfo(FileInfo fileInfo)
    {
        this.fileInfo = fileInfo;
    }
}
