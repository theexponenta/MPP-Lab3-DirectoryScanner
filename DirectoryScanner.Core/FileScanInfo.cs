namespace DirectoryScanner.Core;


public class FileScanInfo
{
    public string Name {get;}
    public long Size {get;}
    public bool IsSymlink {get;}

    internal FileScanInfo(string name, long size, bool isSymlink)
    {
        Name = name;
        Size = size;
        IsSymlink = isSymlink;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FileScanInfo other)
        {
            return false;
        }
        
        return Name == other.Name;
    }
}
