using System.Composition;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Api;

[ExportCommand]
[Shared]
[method: ImportingConstructor]
public class MavlinkParamsSetCommand
    : ContextCommand<IDevicePage>
{
    public const string Id = $"{BaseId}.params.set";
    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Set mavlink params",
        Description = "Set mavlink params",
        Icon = MaterialIconKind.Set,
        Source = IoModule.Instance,
        HotKeyInfo = new HotKeyInfo
        {
            DefaultHotKey = null
        }
    };

    public override ICommandInfo Info => StaticInfo;

    public static ICommandArg CreateArg(string paramName, MavParamValue newConfig)
    {
        return CommandArg.FromChangeAction(paramName, newConfig.ToString());
    }

    protected override async ValueTask<ICommandArg?> InternalExecute(IDevicePage context, ICommandArg newValue, CancellationToken cancel)
    {
        var ifc = context.Device?.GetMicroservice<IParamsClientEx>();
        if (ifc == null)
        {
            return null;
        }
        if (newValue is ActionCommandArg { Action: CommandParameterActionType.Change, Value: not null, Id: not null } action)
        {
            var value = MavParamValue.Parse(action.Value);
            ICommandArg? result = null;
            if (ifc.Items.TryGetValue(action.Id, out var param))
            {
                result = CreateArg(action.Id, param.Value.Value);
            }
            await ifc.WriteOnce(action.Id, value, cancel).ConfigureAwait(false);
            return result;
        }

        return null;
    }
}