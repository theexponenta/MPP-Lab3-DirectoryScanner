namespace DirectoryScanner.Tests;

using DirectoryScanner.Core;

internal class FakeDirectoryProvider : IDirectoryProvider
{
    private FakeDirectoryInfo directory;

    public FakeDirectoryProvider(FakeDirectoryInfo directory)
    {
        this.directory = directory;
    }

    public IDirectoryInfo GetDirectory(string path)
    {
        return directory;
    }
}
