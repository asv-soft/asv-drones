using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Mavlink;
using R3;
using ValidationResult = Asv.Avalonia.ValidationResult;

namespace Asv.Drones;

public class BurstDownloadDialogViewModel : DialogViewModelBase
{
    private const string DialogId = "burst.dialog";

    public BurstDownloadDialogViewModel()
        : base(DialogId, DesignTime.LoggerFactory)
    {
        PacketSize = new BindableReactiveProperty<byte?>(MavlinkFtpHelper.MaxDataSize);
        PacketSize.EnableValidation(
            arg =>
            {
                if (arg is >= 1 and <= MavlinkFtpHelper.MaxDataSize)
                {
                    return ValidationResult.Success;
                }

                return ValidationResult.Create(
                    new ArgumentOutOfRangeException(
                        $"Invalid value (1 - {MavlinkFtpHelper.MaxDataSize})"
                    )
                );
            },
            this,
            isForceValidation: true
        );
    }

    public BindableReactiveProperty<byte?> PacketSize { get; }

    public override void ApplyDialog(ContentDialog dialog)
    {
        dialog.DefaultButton = ContentDialogButton.Primary;
        _sub1 = IsValid.Subscribe(b => dialog.IsPrimaryButtonEnabled = b);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    private IDisposable? _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1?.Dispose();
            PacketSize.Dispose();
        }

        base.Dispose(disposing);
    }
}
