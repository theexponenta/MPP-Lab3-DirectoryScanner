namespace DirectoryScanner.Tests;

using DirectoryScanner.Core;

class FakeFileInfo : IFileInfo
{
    public long Size {get;}
    public string Name {get;}
    public string? LinkTarget {get;}

    public FakeFileInfo(string name, string? linkTarget, long size)
    {
        Size = size;
        Name = name;
        LinkTarget = linkTarget;
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
