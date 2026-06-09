using Asv.Avalonia;
using Asv.Modeling;

namespace Asv.Drones;

/// <summary>
/// Raised by a browser item after it has been successfully renamed.
/// The owning page catches it to record an undoable change.
/// </summary>
public sealed class BrowserItemRenamedEvent(
    IViewModel source,
    string oldPath,
    string newPath,
    FtpBrowserSourceType sourceType
) : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public string OldPath => oldPath;
    public string NewPath => newPath;
    public FtpBrowserSourceType SourceType => sourceType;
}
