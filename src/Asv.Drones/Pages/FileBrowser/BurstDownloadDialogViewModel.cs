using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Asv.Avalonia;

using Asv.Common;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class BurstDownloadDialogViewModel(string id, ILoggerFactory loggerFactory) : DialogViewModelBase(id, loggerFactory)
{
    [Range(1, MavlinkFtpHelper.MaxDataSize)]
    public BindableReactiveProperty<byte?> PacketSize { get; } =
        new BindableReactiveProperty<byte?>(MavlinkFtpHelper.MaxDataSize).EnableValidation();

    public byte DialogResult { get; private set; }

    public override void ApplyDialog(ContentDialog dialog)
    {
        IsValid.Subscribe(x => dialog.IsPrimaryButtonEnabled = x).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PacketSize.Dispose();
        }

        base.Dispose(disposing);
    }
}
