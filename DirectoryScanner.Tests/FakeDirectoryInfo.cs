namespace DirectoryScanner.Tests;

using DirectoryScanner.Core;

class FakeDirectoryInfo : IDirectoryInfo
{
    public string Name {get;}
    public string? LinkTarget {get;}
    public int DirectoriesCount { get { return directories.Count; } }
    public int FilesCount { get { return files.Count; } }
    
    private List<FakeDirectoryInfo> directories;
    private List<FakeFileInfo> files;
    private int delay;


    public FakeDirectoryInfo(string name, string? linkTarget, List<FakeFileInfo> files, List<FakeDirectoryInfo> directories, int delay = 0)
    {
        Name = name;
        LinkTarget = linkTarget;
        this.directories = directories;
        this.files = files;
        this.delay = delay;
    }

    public FakeDirectoryInfo(string name, string linkTarget, FakeDirectoryInfo symlinkTo) 
        : this(name, linkTarget, symlinkTo.files, symlinkTo.directories)
    {   
    }

    public FakeDirectoryInfo(string name, string? linkTarget)
        : this(name, linkTarget, new List<FakeFileInfo>(), new List<FakeDirectoryInfo>())
    {
    }

    public IEnumerable<IDirectoryInfo> EnumerateDirectories()
    {
        foreach (IDirectoryInfo directory in directories)
        {
            if (delay > 0)
            {
                Thread.Sleep(delay);
            }

            yield return directory;
        }
    }

    public IEnumerable<IFileInfo> EnumerateFiles()
    {
        foreach (IFileInfo file in files)
        {
            if (delay > 0)
            {
                Thread.Sleep(delay);
            }

            yield return file;
        }
    }

    public void AddDirectory(FakeDirectoryInfo directory)
    {
        directories.Add(directory);
    }

    public FakeDirectoryInfo? GetDirectory(string name)
    {
        foreach (FakeDirectoryInfo directory in directories)
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
        foreach (FakeFileInfo file in files)
        {
            if (file.Name == name)
            {
                return true;
            }
        }

        return false;
    }
}
