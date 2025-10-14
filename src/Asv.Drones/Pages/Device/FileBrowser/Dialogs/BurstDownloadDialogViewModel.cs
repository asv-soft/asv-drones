using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public class BurstDownloadDialogViewModel : DialogViewModelBase
{
    private const string DialogId = $"{BaseId}.burst";

    public BurstDownloadDialogViewModel()
        : base(DialogId, DesignTime.LoggerFactory)
    {
        PacketSize = new BindableReactiveProperty<byte?>(MavlinkFtpHelper.MaxDataSize);
        PacketSize
            .EnableValidationRoutable(
                arg =>
                    arg is >= 1 and <= MavlinkFtpHelper.MaxDataSize
                        ? ValidationResult.Success
                        : ValidationResult.FailAsOutOfRange(
                            "1",
                            MavlinkFtpHelper.MaxDataSize.ToString()
                        ),
                this,
                isForceValidation: true
            )
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<byte?> PacketSize { get; }

    public override void ApplyDialog(ContentDialog dialog)
    {
        dialog.DefaultButton = ContentDialogButton.Primary;
        IsValid.Subscribe(b => dialog.IsPrimaryButtonEnabled = b).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
