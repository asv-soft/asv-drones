using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones;

/// <summary>
/// <para>Executes with:</para>
/// <para>- <c>arg["src"]</c> as <c>string</c> sourcePath.</para>
/// <para>- <c>arg["dst"]</c> as <c>string</c> destinationPath.</para>
/// <para>- <c>arg["prt"]</c> as <c>int</c> partSize.</para>
/// <para>- <c>arg["typ"]</c> as <c>string</c> entryType.</para>
/// </summary>
[ExportCommand]
public class BurstDownloadItemCommand : TransferCommandBase
{
    public const string Id = $"{BaseIdTransferCmd}.burst_download";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.BurstDownloadItemCommand_CommandInfo_Name,
        Description = RS.BurstDownloadItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.TransferLeft,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    public override async ValueTask<DictArg?> InternalExecute(
        ITransferFtpEntries context,
        DictArg newValue,
        CancellationToken cancel
    )
    {
        if (!TryReadRequiredString(newValue, SourcePath, out var src))
        {
            return null;
        }

        if (!TryReadRequiredString(newValue, DestinationPath, out var dst))
        {
            return null;
        }

        if (!TryReadRequiredByte(newValue, PartSize, out var part))
        {
            return null;
        }

        if (!TryReadRequiredEntryType(newValue, EntryType, out var type))
        {
            return null;
        }

        await context.BurstDownloadItem(src, dst, part, type, cancel);
        return null;
    }
}
