namespace DirectoryScanner.Core;

using Exceptions;

public class DirectoryScanInfo
{
    public string Name {get;}
    public long Size 
    {
        get
        {
            if (size >= 0)
                return size;
            
            size = 0;
            foreach (FileScanInfo fileScanInfo in files)
            {
                if (!fileScanInfo.IsSymlink)
                {
                    size += fileScanInfo.Size;
                }
            }

            foreach (DirectoryScanInfo directoryScanInfo in directories.Values)
            {
                if (!directoryScanInfo.IsSymlink)
                {
                    size += directoryScanInfo.Size;
                }
            }

            return size;
        }
    }

    public int DirectoriesCount { get { return directories.Count; } }
    public int FilesCount { get { return files.Count; } }
    public bool IsSymlink { get; }

    private long size = -1;
    private Dictionary<string, DirectoryScanInfo> directories = new Dictionary<string, DirectoryScanInfo>();
    private HashSet<FileScanInfo> files = new HashSet<FileScanInfo>();

    internal DirectoryScanInfo(string name, bool isSymlink)
    {
        Name = name;
        IsSymlink = isSymlink;
    }

    internal void CopyAsSymlink(DirectoryScanInfo other)
    {
        directories = other.directories;
        files = other.files;
    }

    internal void AddDirectory(DirectoryScanInfo directory)
    {
        if (!directories.TryAdd(directory.Name, directory))
        {
            throw new DirectoryAlreadyExistsException(directory.Name);
        }
    }

    internal void AddFile(FileScanInfo file)
    {
        if (files.Contains(file))
        {
            throw new FileAlreadyExistsException(file.Name);
        }

        files.Add(file);
    }

    public DirectoryScanInfo? GetDirectory(string name)
    {
        DirectoryScanInfo? directory;
        if (directories.TryGetValue(name, out directory))
        {
            return directory;
        }

        return null;
    }

    public IEnumerable<DirectoryScanInfo> EnumerateDirectories()
    {
        return directories.Values.AsEnumerable();
    }

    public IEnumerable<FileScanInfo> EnumerateFiles()
    {
        return files.AsEnumerable();
    }
}
