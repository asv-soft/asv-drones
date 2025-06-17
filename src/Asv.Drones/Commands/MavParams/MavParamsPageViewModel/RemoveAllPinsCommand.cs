using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class RemoveAllPinsCommand : ContextCommand<MavParamsPageViewModel, DictArg>
{
    public const string Id = $"{BaseId}.params.remove-all-pins";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UnpinAllParamsCommand_CommandInfo_Name,
        Description = RS.UnpinAllParamsCommand_CommandInfo_Description,
        Icon = MaterialIconKind.PinOff,
        DefaultHotKey = null, // TODO: make a key bind when new key listener system appears
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    public override ValueTask<DictArg?> InternalExecute(
        MavParamsPageViewModel context,
        DictArg arg,
        CancellationToken cancel
    )
    {
        if (context.AllParams is null)
        {
            return ValueTask.FromResult<DictArg?>(null);
        }

        if (arg.Count == 0)
        {
            var oldValue = new DictArg();
            foreach (var item in context.AllParams.Where(item => item.IsPinned.ViewValue.Value))
            {
                oldValue.Add(
                    new KeyValuePair<string, CommandArg>(item.Id.ToString(), new BoolArg(true))
                );
                item.IsPinned.ModelValue.Value = false;
            }

            var notSelected = context
                .ViewedParams.Where(it => it.Id != context.SelectedItem.Value?.Id)
                .ToArray();

            foreach (var item in notSelected)
            {
                context.ViewedParams.Remove(item);
            }

            return ValueTask.FromResult<DictArg?>(oldValue);
        }

        foreach (var item in context.AllParams.Where(item => arg.ContainsKey(item.Id.ToString())))
        {
            item.IsPinned.ModelValue.Value = !item.IsPinned.ModelValue.Value;
        }

        return ValueTask.FromResult<DictArg?>(arg);
    }
}
