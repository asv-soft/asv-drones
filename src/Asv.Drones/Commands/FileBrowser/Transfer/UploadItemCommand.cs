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
public class UploadItemCommand : ContextCommand<ITransferFtpEntries, DictArg>
{
    public const string Id = $"{BaseId}.transfer.upload";

    public const string SourcePath = "src";
    public const string DestinationPath = "dst";
    public const string EntryType = "typ";

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
        if (!newValue.TryGetValue(SourcePath, out var src))
        {
            return null;
        }

        if (!newValue.TryGetValue(DestinationPath, out var dst))
        {
            return null;
        }

        if (!newValue.TryGetValue(EntryType, out var typ))
        {
            return null;
        }

        var source = src.AsString();
        if (source.Length == 0)
        {
            return null;
        }

        var destination = dst.AsString();
        if (destination.Length == 0)
        {
            return null;
        }

        if (!Enum.TryParse<FtpEntryType>(typ.AsString(), true, out var entryType))
        {
            return null;
        }

        await context.UploadItem(source, destination, entryType, cancel);

        return null;
    }
}
