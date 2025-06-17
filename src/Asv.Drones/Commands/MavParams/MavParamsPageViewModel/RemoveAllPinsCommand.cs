using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
public class RemoveAllPinsCommand : ContextCommand<MavParamsPageViewModel, ListArg>
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

    /*protected override void InternalExecute(
        MavParamsPageViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        /*var value = newValue as ListCommandArg<ParamItemViewModel>;

        

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

            return ValueTask.FromResult<CommandArg?>(oldValue);
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

        return ValueTask.FromResult<CommandArg?>(oldValue1);
    }*/

    public override async ValueTask<ListArg?> InternalExecute(
        MavParamsPageViewModel context,
        ListArg arg,
        CancellationToken cancel
    )
    {
        return null; // TODO: implement this method
    }
}
