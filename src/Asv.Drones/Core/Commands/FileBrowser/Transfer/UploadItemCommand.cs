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
/// <para>- <c>arg["typ"]</c> as <c>string</c> entryType.</para>
/// </summary>
[ExportCommand]
public class UploadItemCommand : TransferCommandBase
{
    public const string Id = $"{BaseIdTransferCmd}.upload";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UploadItemCommand_CommandInfo_Name,
        Description = RS.UploadItemCommand_CommandInfo_Description,
        Icon = MaterialIconKind.TransferRight,
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

        if (!TryReadRequiredEntryType(newValue, EntryType, out var type))
        {
            return null;
        }

        await context.UploadItem(src, dst, type, cancel);
        return null;
    }
}
