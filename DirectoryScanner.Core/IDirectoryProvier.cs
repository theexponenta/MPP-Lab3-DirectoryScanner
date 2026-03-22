namespace DirectoryScanner.Core;

public interface IDirectoryProvider
{
    public IDirectoryInfo GetDirectory(string path);
}
