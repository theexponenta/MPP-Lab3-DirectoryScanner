namespace DirectoryScanner.Core;

using Exceptions;

public class DirectoryScanInfo
{
    public string Name {get;}
    public long Size 
    {
        get
        {
            if (_size >= 0)
                return _size;
            
            _size = 0;
            foreach (FileScanInfo fileScanInfo in _files)
            {
                if (!fileScanInfo.IsSymlink)
                {
                    _size += fileScanInfo.Size;
                }
            }

            foreach (DirectoryScanInfo directoryScanInfo in _directories.Values)
            {
                if (!directoryScanInfo.IsSymlink)
                {
                    _size += directoryScanInfo.Size;
                }
            }

            return _size;
        }
    }

    public int DirectoriesCount { get { return _directories.Count; } }
    public int FilesCount { get { return _files.Count; } }
    public bool IsSymlink { get; }

    private long _size = -1;
    private Dictionary<string, DirectoryScanInfo> _directories = new();
    private HashSet<FileScanInfo> _files = new();

    internal DirectoryScanInfo(string name, bool isSymlink)
    {
        Name = name;
        IsSymlink = isSymlink;
    }

    internal void CopyAsSymlink(DirectoryScanInfo other)
    {
        _directories = other._directories;
        _files = other._files;
    }

    internal void AddDirectory(DirectoryScanInfo directory)
    {
        if (!_directories.TryAdd(directory.Name, directory))
        {
            throw new DirectoryAlreadyExistsException(directory.Name);
        }
    }

    internal void AddFile(FileScanInfo file)
    {
        if (_files.Contains(file))
        {
            throw new FileAlreadyExistsException(file.Name);
        }

        _files.Add(file);
    }

    public DirectoryScanInfo? GetDirectory(string name)
    {
        DirectoryScanInfo? directory;
        if (_directories.TryGetValue(name, out directory))
        {
            return directory;
        }

        return null;
    }

    public IEnumerable<DirectoryScanInfo> EnumerateDirectories()
    {
        return _directories.Values.AsEnumerable();
    }

    public IEnumerable<FileScanInfo> EnumerateFiles()
    {
        return _files.AsEnumerable();
    }
}
