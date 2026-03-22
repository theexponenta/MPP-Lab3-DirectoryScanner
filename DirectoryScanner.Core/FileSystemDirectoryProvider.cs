namespace DirectoryScanner.Core;

using Exceptions;

public class FileSystemDirectoryProvider : IDirectoryProvider
{
    public IDirectoryInfo GetDirectory(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        if (!directoryInfo.Exists)
        {
            throw new DirectoryDoesNotExistException(path);
        }

        return new FileSystemDirectoryInfo(directoryInfo);
    }
}
