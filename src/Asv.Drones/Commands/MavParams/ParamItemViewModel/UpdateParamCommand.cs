using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class UpdateParamCommand : ContextCommand<ParamItemViewModel>
{
    public const string Id = $"{BaseId}.params.item.update";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UpdateParamCommand_CommandInfo_Name,
        Description = RS.UpdateParamCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Update,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null }, // TODO: make a key bind when new key listener system appears
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ICommandArg?> InternalExecute(
        ParamItemViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        await context.UpdateImpl(cancel);
        return null;
    }
}
