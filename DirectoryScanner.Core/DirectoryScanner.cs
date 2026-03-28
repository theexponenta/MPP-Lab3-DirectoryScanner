namespace DirectoryScanner.Core;

using System.Collections.Concurrent;

public class DirectoryScanner
{
    private struct ScanTask
    {
        public IDirectoryInfo directoryInfo;
        public DirectoryScanInfo directoryScanInfo;

        public ScanTask(IDirectoryInfo directoryInfo, DirectoryScanInfo directoryScanInfo)
        {
            this.directoryInfo = directoryInfo;
            this.directoryScanInfo = directoryScanInfo;
        }
    }

    private IDirectoryProvider _directoryProvider;
    private int _maxWorkers = 0;
    private int _workersCount = 0;
    private int _tasksLeft = 0;
    private ConcurrentQueue<ScanTask> _tasks = new();
    private ConcurrentDictionary<string, DirectoryScanInfo> _symlinks = new();
    private CancellationTokenSource _cts = new();

    public DirectoryScanInfo? Result {get; private set;}
    public event EventHandler? Completed;

    public DirectoryScanner(IDirectoryProvider directoryProvider)
    {
        _directoryProvider = directoryProvider;
    }

    public DirectoryScanner() : this(new FileSystemDirectoryProvider()) {}

    protected virtual void OnCompleted() 
    {
        _cts.Dispose();
        _symlinks.Clear();
        _tasks.Clear();
        Completed?.Invoke(this, EventArgs.Empty);
    }

    private void EnqueueTask(ScanTask task)
    {
        Interlocked.Increment(ref _tasksLeft);
        _tasks.Enqueue(task);
        if (_workersCount < _maxWorkers)
        {
            int newValue = Interlocked.Increment(ref _workersCount);
            if (newValue <= _maxWorkers)
            {
                ThreadPool.QueueUserWorkItem(ProcessDirectories, _cts.Token);
            } 
            else
            {
                Interlocked.Decrement(ref _workersCount);
            }
        }
    }

    private void ProcessDirectories(object? obj)
    {
        if (obj is null)
            return;

        CancellationToken token = (CancellationToken)obj;

        while (!token.IsCancellationRequested)
        {
            if (_tasksLeft == 0)
            {
                return;
            }

            ScanTask task;
            if (!_tasks.TryDequeue(out task!))
            {
                Thread.Yield();                
                continue;        
            }

            foreach (IDirectoryInfo subdirInfo in task.directoryInfo.EnumerateDirectories())
            {
                string? linkTarget = subdirInfo.LinkTarget;
                bool needEnqueue = true;
                DirectoryScanInfo subdirScanInfo = new DirectoryScanInfo(subdirInfo.Name, linkTarget is not null);
                if (linkTarget is not null)
                {
                    DirectoryScanInfo symlinkScanInfo = _symlinks.GetOrAdd(linkTarget, subdirScanInfo);
                    if (subdirScanInfo != symlinkScanInfo)
                    {
                        needEnqueue = false;
                        subdirScanInfo.CopyAsSymlink(symlinkScanInfo);
                    }
                } 

                task.directoryScanInfo.AddDirectory(subdirScanInfo);
                if (needEnqueue)
                {
                    EnqueueTask(new ScanTask(subdirInfo, subdirScanInfo));
                }
            }

            foreach (IFileInfo fileInfo in task.directoryInfo.EnumerateFiles())
            {
                FileScanInfo fileScanInfo = new FileScanInfo(fileInfo.Name, fileInfo.Size, fileInfo.LinkTarget is not null);
                task.directoryScanInfo.AddFile(fileScanInfo);
            }

            int newDirsLeft = Interlocked.Decrement(ref _tasksLeft);
            if (newDirsLeft == 0)
            {
                OnCompleted();
                return;
            }
        }

        int newWorkersCount = Interlocked.Decrement(ref _workersCount);
        if (newWorkersCount == 0)
        {
            OnCompleted();
        }
    }

    public void StartScan(string path, int maxWorkers)
    {
        IDirectoryInfo directoryInfo = _directoryProvider.GetDirectory(path);

        _tasks.Clear();
        _symlinks.Clear();
        _workersCount = 0;
        this._maxWorkers = maxWorkers;
        _cts.Dispose();
        _cts = new CancellationTokenSource();

        Result = new DirectoryScanInfo(directoryInfo.Name, directoryInfo.LinkTarget is not null);
        EnqueueTask(new ScanTask(directoryInfo, Result));
    } 

    public void ScanSync(string path, int maxWorkers)
    {
        ManualResetEventSlim completedWaiter = new ManualResetEventSlim(false);
        EventHandler completedHandler = (sender, e) => { completedWaiter.Set(); };
        Completed += completedHandler;

        StartScan(path, maxWorkers);
        completedWaiter.Wait();
        Completed -= completedHandler;

        completedWaiter.Dispose();
    }

    public void Cancel()
    {
        _cts.Cancel();
    }
}
