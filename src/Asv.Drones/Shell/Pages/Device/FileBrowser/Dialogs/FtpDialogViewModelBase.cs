using Asv.Avalonia;

namespace Asv.Drones;

public abstract class FtpDialogViewModelBase : DialogViewModelBase
{
    private const string FtpDialogId = $"{BaseId}-ftp";

    protected FtpDialogViewModelBase(string typeId)
        : base($"{FtpDialogId}-{typeId}") { }
}
