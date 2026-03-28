namespace DirectoryScanner.Tests;

using DirectoryScanner.Core;

class FakeDirectoryInfo : IDirectoryInfo
{
    public string Name {get;}
    public string? LinkTarget {get;}
    public int DirectoriesCount { get { return _directories.Count; } }
    public int FilesCount { get { return _files.Count; } }
    
    private List<FakeDirectoryInfo> _directories;
    private List<FakeFileInfo> _files;
    private int _delay;


    public FakeDirectoryInfo(string name, string? linkTarget, List<FakeFileInfo> files, List<FakeDirectoryInfo> directories, int delay = 0)
    {
        Name = name;
        LinkTarget = linkTarget;
        _directories = directories;
        _files = files;
        _delay = delay;
    }

    public FakeDirectoryInfo(string name, string linkTarget, FakeDirectoryInfo symlinkTo) 
        : this(name, linkTarget, symlinkTo._files, symlinkTo._directories)
    {   
    }

    public FakeDirectoryInfo(string name, string? linkTarget)
        : this(name, linkTarget, new List<FakeFileInfo>(), new List<FakeDirectoryInfo>())
    {
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories()
    {
        foreach (IDirectoryInfo directory in _directories)
        {
            if (_delay > 0)
            {
                Thread.Sleep(_delay);
            }

            yield return directory;
        }
    }

    public IEnumerable<IFileInfo> EnumerateFiles()
    {
        foreach (IFileInfo file in _files)
        {
            if (_delay > 0)
            {
                Thread.Sleep(_delay);
            }

            yield return file;
        }
    }

    public void AddDirectory(FakeDirectoryInfo directory)
    {
        _directories.Add(directory);
    }

    public FakeDirectoryInfo? GetDirectory(string name)
    {
        foreach (FakeDirectoryInfo directory in _directories)
        {
            if (directory.Name == name)
            {
                return directory;
            }
        }

        return null;
    }

    public bool HasFile(string name)
    {
        foreach (FakeFileInfo file in _files)
        {
            if (file.Name == name)
            {
                return true;
            }
        }

        return false;
    }
}
