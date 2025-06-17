using System;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones;

[ExportCommand]
[Shared]
public class MavlinkParamsWriteCommand : MavlinkMicroserviceCommand<IParamsClientEx, ActionArg>
{
    public const string Id = $"{BaseId}.mavlink.param.write";
    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.WritePatamCommand_CommandInfo_Name,
        Description = RS.WriteParamCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Set,
        Source = ApiModule.Instance,
        DefaultHotKey = null,
    };

    public static ValueTask Execute(
        IRoutable context,
        string name,
        MavParamValue value,
        CancellationToken cancel = default
    )
    {
        return context.ExecuteCommand(
            Id,
            CommandArg.ChangeAction(name, CommandArg.CreateString(value.PrintValue())),
            cancel
        );
    }

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ActionArg?> InternalExecute(
        IParamsClientEx microservice,
        ActionArg arg,
        CancellationToken cancel
    )
    {
        if (string.IsNullOrWhiteSpace(arg.SubjectId))
        {
            throw new ArgumentException(
                $"{nameof(arg.SubjectId)} cannot be null or empty.",
                nameof(arg.SubjectId)
            );
        }

        MavParamValue prevValue;
        if (!microservice.Items.TryGetValue(arg.SubjectId, out var param))
        {
            prevValue = await microservice.ReadOnce(arg.SubjectId, cancel);
        }
        else
        {
            prevValue = param.Value.Value;
        }

        var stringValue = arg.Value?.AsString();
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            throw new ArgumentException(
                $"{nameof(arg.Value)} must be of type {CommandArg.Id.String}.",
                nameof(arg.Value)
            );
        }

        var result = MavParamValue.TryParseValue(stringValue, prevValue.Type, out var value);
        if (!result.IsSuccess)
        {
            Debug.Assert(result.ValidationException != null, "result.ValidationException != null");
            throw new ArgumentException(
                $"Cannot parse value '{stringValue}' to type {prevValue.Type}: {result.ValidationException.Message}",
                nameof(arg.Value),
                result.ValidationException
            );
        }

        Debug.Assert(value != null, nameof(value) + " != null");
        await microservice.WriteOnce(arg.SubjectId, value.Value, cancel);

        return CommandArg.ChangeAction(
            arg.SubjectId,
            CommandArg.CreateString(prevValue.PrintValue())
        );
    }
}
