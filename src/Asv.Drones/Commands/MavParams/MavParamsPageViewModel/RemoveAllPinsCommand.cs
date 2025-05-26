using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;
using R3;

namespace Asv.Drones;

[ExportCommand]
public class RemoveAllPinsCommand : ContextCommand<MavParamsPageViewModel>
{
    public const string Id = $"{BaseId}.params.remove-all-pins";

    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.UnpinAllParamsCommand_CommandInfo_Name,
        Description = RS.UnpinAllParamsCommand_CommandInfo_Description,
        Icon = MaterialIconKind.PinOff,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null }, // TODO: make a key bind when new key listener system appears
        Source = SystemModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        MavParamsPageViewModel context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var value = newValue as ListCommandArg<ParamItemViewModel>;

        context.SelectedItem.Value = null;

        if (value?.Items is null)
        {
            var pinned = new List<ParamItemViewModel>();
            foreach (var item in context.AllParams.Where(item => item.IsPinned.ViewValue.Value))
            {
                pinned.Add(item);
                item.IsPinned.ViewValue.Value = false;
            }

            foreach (var item in pinned)
            {
                context.ViewedParams.Remove(item);
            }

            var oldValue = new ListCommandArg<ParamItemViewModel>(pinned);

            return ValueTask.FromResult<ICommandArg?>(oldValue);
        }

        foreach (var item in context.AllParams)
        {
            var oldItem = value.Items.FirstOrDefault(it => it.Name == item.Name);

            if (oldItem is null)
            {
                continue;
            }

            item.PinItem.Execute(Unit.Default);

            if (item.IsPinned.ViewValue.Value)
            {
                context.ViewedParams.Add(item);
            }
        }

        var oldValue1 = new ListCommandArg<ParamItemViewModel>(value.Items);

        return ValueTask.FromResult<ICommandArg?>(oldValue1);
    }
}
