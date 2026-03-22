using System.Collections.ObjectModel;

namespace DirectoryScanner.GUI.ViewModels;

public abstract class ExplorerNodeViewModel : ViewModelBase
{
    protected ExplorerNodeViewModel(long parentDirectorySize)
    {
        ParentDirectorySize = parentDirectorySize;
    }

    public long ParentDirectorySize { get; }

    public abstract string Name { get; }

    public abstract long SizeBytes { get; }

    public abstract bool IsFolder { get; }

    public abstract bool IsPendingPlaceholder { get; }

    public virtual bool IsFile => false;

    public bool ShowInTree => !IsPendingPlaceholder;

    public ObservableCollection<ExplorerNodeViewModel> Children { get; } = new();

    public virtual string SizeAndPercentText
    {
        get
        {
            string sizePart = $"{SizeBytes} B";
            if (ParentDirectorySize <= 0)
                return $"{Name} — {sizePart}, 0%";

            double pct = 100.0 * SizeBytes / ParentDirectorySize;
            return $"{Name} — {sizePart}, {pct:F2}%";
        }
    }
}
