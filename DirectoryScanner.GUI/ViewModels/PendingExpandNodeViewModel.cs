namespace DirectoryScanner.GUI.ViewModels;

/// <summary>
/// Placeholder so TreeView shows an expander before children are materialized.
/// </summary>
public sealed class PendingExpandNodeViewModel : ExplorerNodeViewModel
{
    public PendingExpandNodeViewModel()
        : base(0)
    {
    }

    public override string Name => string.Empty;

    public override long SizeBytes => 0;

    public override bool IsFolder => false;

    public override bool IsPendingPlaceholder => true;

    public override string SizeAndPercentText => string.Empty;
}
