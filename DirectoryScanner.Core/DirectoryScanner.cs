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

    private IDirectoryProvider directoryProvider;
    private int maxWorkers = 0;
    private int workersCount = 0;
    private volatile int dirsLeft = 0;
    private ConcurrentQueue<ScanTask> tasks = new ConcurrentQueue<ScanTask>();
    private ConcurrentDictionary<string, DirectoryScanInfo> symlinks = new ConcurrentDictionary<string, DirectoryScanInfo>();
    private CancellationTokenSource cts = new CancellationTokenSource();
    public DirectoryScanInfo? Result {get; private set;}
    public event EventHandler? Completed;

    public DirectoryScanner(IDirectoryProvider directoryProvider)
    {
        this.directoryProvider = directoryProvider;
    }

    public DirectoryScanner() : this(new FileSystemDirectoryProvider()) {}

    protected virtual void OnCompleted() 
    {
        cts.Dispose();
        symlinks.Clear();
        tasks.Clear();
        Completed?.Invoke(this, EventArgs.Empty);
    }

    private void EnqueueTask(ScanTask task)
    {
        Interlocked.Increment(ref dirsLeft);
        tasks.Enqueue(task);
        if (workersCount < maxWorkers)
        {
            int newValue = Interlocked.Increment(ref workersCount);
            if (newValue <= maxWorkers)
            {
                ThreadPool.QueueUserWorkItem(ProcessDirectories, cts.Token);
            } 
            else
            {
                Interlocked.Decrement(ref workersCount);
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
            if (dirsLeft == 0)
            {
                return;
            }

            ScanTask task;
            if (!tasks.TryDequeue(out task!))
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
                    DirectoryScanInfo symlinkScanInfo = symlinks.GetOrAdd(linkTarget, subdirScanInfo);
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

            int newDirsLeft = Interlocked.Decrement(ref dirsLeft);
            if (newDirsLeft == 0)
            {
                OnCompleted();
                return;
            }
        }

        int newWorkersCount = Interlocked.Decrement(ref workersCount);
        if (newWorkersCount == 0)
        {
            OnCompleted();
        }
    }

    public void StartScan(string path, int maxWorkers)
    {
        IDirectoryInfo directoryInfo = directoryProvider.GetDirectory(path);

        tasks.Clear();
        symlinks.Clear();
        workersCount = 0;
        this.maxWorkers = maxWorkers;
        cts.Dispose();
        cts = new CancellationTokenSource();

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
        cts.Cancel();
    }
}
