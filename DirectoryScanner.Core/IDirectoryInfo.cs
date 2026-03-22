namespace DirectoryScanner.Core;

public interface IDirectoryInfo
{
    public string Name {get;}
    public string? LinkTarget {get;}
    public IEnumerable<IDirectoryInfo> EnumerateDirectories();
    public IEnumerable<IFileInfo> EnumerateFiles();

}

