using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public class BurstDownloadDialogViewModel : FtpDialogViewModelBase
{
    private const string DialogId = "burst";

    private readonly SerialDisposable _sub = new();

    public BurstDownloadDialogViewModel()
        : base(DialogId)
    {
        PacketSize = new BindableReactiveProperty<byte?>(
            MavlinkFtpHelper.MaxDataSize
        ).DisposeItWith(Disposable);
        PacketSize
            .EnableValidationRoutable(
                arg =>
                {
                    if (arg is < 1 or > MavlinkFtpHelper.MaxDataSize)
                    {
                        return ValidationResult.FailAsOutOfRange(
                            "1",
                            MavlinkFtpHelper.MaxDataSize.ToString()
                        );
                    }

                    return ValidationResult.Success;
                },
                this,
                isForceValidation: true
            )
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<byte?> PacketSize { get; }

    public override void ApplyDialog(ContentDialog dialog)
    {
        dialog.DefaultButton = ContentDialogButton.Primary;
        _sub.Disposable = IsValid.Subscribe(b => dialog.IsPrimaryButtonEnabled = b);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub.Dispose();
        }

        base.Dispose(disposing);
    }
}
