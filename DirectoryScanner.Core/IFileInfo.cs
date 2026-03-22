namespace DirectoryScanner.Core;

public interface IFileInfo
{
    public long Size {get;}
    public string Name {get;}
    public string? LinkTarget {get;}
}
