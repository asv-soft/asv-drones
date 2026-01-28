using System;
using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones;

public abstract class TransferCommandBase : ContextCommand<ITransferFtpEntries, DictArg>
{
    protected const string BaseIdTransferCmd = $"{BaseId}.transfer";

    public const string SourcePath = "src";
    public const string DestinationPath = "dst";
    public const string PartSize = "prt";
    public const string EntryType = "typ";

    protected static bool TryReadRequiredString(DictArg args, string key, out string value)
    {
        value = string.Empty;
        if (!args.TryGetValue(key, out var v))
        {
            return false;
        }

        value = v.AsString();
        return value.Length > 0;
    }

    protected static bool TryReadRequiredByte(DictArg args, string key, out byte value)
    {
        value = 0;
        if (!args.TryGetValue(key, out var v))
        {
            return false;
        }

        var i = v.AsInt();
        if (i is < byte.MinValue or > byte.MaxValue)
        {
            return false;
        }

        value = (byte)i;
        return true;
    }

    protected static bool TryReadRequiredEntryType(
        DictArg args,
        string key,
        out FtpEntryType entryType
    )
    {
        entryType = default;
        return args.TryGetValue(key, out var v)
            && Enum.TryParse(v.AsString(), ignoreCase: true, out entryType);
    }
}
