namespace DirectoryScanner.Tests;

using DirectoryScanner.Core;

internal class FakeDirectoryProvider : IDirectoryProvider
{
    private FakeDirectoryInfo _directory;

    public FakeDirectoryProvider(FakeDirectoryInfo directory)
    {
        _directory = directory;
    }

    public IDirectoryInfo GetDirectory(string path)
    {
        return _directory;
    }
}
