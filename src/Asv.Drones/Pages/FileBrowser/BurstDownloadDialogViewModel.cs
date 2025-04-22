using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Asv.Avalonia;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public class BurstDownloadDialogViewModel(string id) : DialogViewModelBase(id)
{
    [Range(1, MavlinkFtpHelper.MaxDataSize)]
    public BindableReactiveProperty<byte?> PacketSize { get; } =
        new BindableReactiveProperty<byte?>(MavlinkFtpHelper.MaxDataSize).EnableValidation();

    public byte DialogResult { get; private set; }

    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.PrimaryButtonCommand = IsValid.ToReactiveCommand(_ =>
            DialogResult = PacketSize.Value ?? MavlinkFtpHelper.MaxDataSize
        );
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
