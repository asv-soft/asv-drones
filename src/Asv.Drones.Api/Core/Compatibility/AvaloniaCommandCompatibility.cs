using System.Collections;
using System.Windows.Input;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public interface ICommandInfo
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    MaterialIconKind Icon { get; }
    string? DefaultHotKey { get; }
}

public sealed class CommandInfo : ICommandInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required MaterialIconKind Icon { get; init; }
    public string? DefaultHotKey { get; init; }
}

public class CommandArg
{
    public static class Id
    {
        public const string String = "string";
    }

    public static CommandArg Empty { get; } = new();

    public static StringArg CreateString(string? value)
    {
        return new StringArg(value ?? string.Empty);
    }

    public static IntegerArg CreateInteger(int value)
    {
        return new IntegerArg(value);
    }

    public static DoubleArg CreateDouble(double value)
    {
        return new DoubleArg(value);
    }

    public static DictArg CreateDictionary()
    {
        return new DictArg();
    }

    public static DictArg CreateDictionary(Dictionary<string, CommandArg> value)
    {
        return new DictArg(value);
    }

    public static ActionArg ChangeAction(string subjectId, CommandArg value)
    {
        return new ActionArg(subjectId, value);
    }

    public virtual string AsString()
    {
        return ToString() ?? string.Empty;
    }

    public virtual int AsInt()
    {
        return int.TryParse(AsString(), out var value) ? value : 0;
    }

    public virtual double AsDouble()
    {
        return double.TryParse(AsString(), out var value) ? value : 0;
    }
}

public sealed class StringArg(string value) : CommandArg
{
    public string Value { get; } = value;

    public override string AsString()
    {
        return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}

public sealed class IntegerArg(int value) : CommandArg
{
    public int Value { get; } = value;

    public override int AsInt()
    {
        return Value;
    }

    public override string AsString()
    {
        return Value.ToString();
    }
}

public sealed class DoubleArg(double value) : CommandArg
{
    public double Value { get; } = value;

    public override double AsDouble()
    {
        return Value;
    }

    public override string AsString()
    {
        return Value.ToString();
    }
}

public sealed class BoolArg(bool value) : CommandArg
{
    public bool Value { get; } = value;

    public override string AsString()
    {
        return Value.ToString();
    }
}

public sealed class ActionArg(string subjectId, CommandArg? value) : CommandArg
{
    public string SubjectId { get; } = subjectId;
    public CommandArg? Value { get; } = value;
}

public sealed class DictArg : CommandArg, IEnumerable<KeyValuePair<string, CommandArg>>
{
    private readonly Dictionary<string, CommandArg> _items;

    public DictArg()
    {
        _items = [];
    }

    public DictArg(Dictionary<string, CommandArg> items)
    {
        _items = items;
    }

    public int Count => _items.Count;

    public void Add(KeyValuePair<string, CommandArg> item)
    {
        _items.Add(item.Key, item.Value);
    }

    public bool TryGetValue(string key, out CommandArg value)
    {
        return _items.TryGetValue(key, out value!);
    }

    public bool ContainsKey(string key)
    {
        return _items.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<string, CommandArg>> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public interface IAsyncCommand
{
    ICommandInfo Info { get; }
    ValueTask<CommandArg?> Execute(
        IViewModel context,
        CommandArg parameter,
        CancellationToken cancel
    );
    bool CanExecute(IViewModel context, CommandArg parameter, out IViewModel targetContext);
}

public abstract class AsyncCommand : IAsyncCommand
{
    public const string BaseId = "cmd";

    public abstract ICommandInfo Info { get; }

    public virtual bool CanExecute(
        IViewModel context,
        CommandArg parameter,
        out IViewModel targetContext
    )
    {
        targetContext = context;
        return true;
    }

    public async ValueTask<CommandArg?> Execute(
        IViewModel context,
        CommandArg parameter,
        CancellationToken cancel
    )
    {
        return CanExecute(context, parameter, out var targetContext)
            ? await InternalExecute(targetContext, parameter, cancel)
            : null;
    }

    protected abstract ValueTask<CommandArg?> InternalExecute(
        IViewModel context,
        CommandArg parameter,
        CancellationToken cancel
    );
}

public abstract class ContextCommand<TContext> : ContextCommand<TContext, CommandArg>
    where TContext : class, IViewModel;

public abstract class ContextCommand<TContext, TArg> : AsyncCommand
    where TContext : class, IViewModel
    where TArg : CommandArg
{
    public override bool CanExecute(
        IViewModel context,
        CommandArg parameter,
        out IViewModel targetContext
    )
    {
        var target = context as TContext ?? context.FindParentOfType<TContext>();
        targetContext = target ?? context;
        return target != null && parameter is TArg;
    }

    protected sealed override ValueTask<CommandArg?> InternalExecute(
        IViewModel context,
        CommandArg parameter,
        CancellationToken cancel
    )
    {
        return ExecuteTyped((TContext)context, (TArg)parameter, cancel);
    }

    private async ValueTask<CommandArg?> ExecuteTyped(
        TContext context,
        TArg parameter,
        CancellationToken cancel
    )
    {
        return await InternalExecute(context, parameter, cancel);
    }

    public abstract ValueTask<TArg?> InternalExecute(
        TContext context,
        TArg arg,
        CancellationToken cancel
    );
}

public sealed class BindableAsyncCommand(string commandId, IViewModel context) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        var arg = parameter as CommandArg ?? CommandArg.Empty;
        return CommandRegistry.TryGet(commandId, out var command)
            && command.CanExecute(context, arg, out _);
    }

    public async void Execute(object? parameter)
    {
        var arg = parameter as CommandArg ?? CommandArg.Empty;
        await context.ExecuteCommand(commandId, arg);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

public interface ICommandService
{
    ValueTask<CommandArg?> Execute(
        IViewModel context,
        string commandId,
        CommandArg arg,
        CancellationToken cancel = default
    );
}

public interface INavigationService
{
    ValueTask GoTo(string pageId, CommandArg? arg = null, CancellationToken cancel = default);
}

public sealed class NullNavigationService : INavigationService
{
    public static NullNavigationService Instance { get; } = new();

    private NullNavigationService() { }

    public ValueTask GoTo(string pageId, CommandArg? arg = null, CancellationToken cancel = default)
    {
        return ValueTask.CompletedTask;
    }
}

public abstract class OpenPageCommandBase(string pageId, INavigationService? navigation = null)
    : AsyncCommand
{
    private readonly INavigationService _navigation = navigation ?? NullNavigationService.Instance;

    protected override async ValueTask<CommandArg?> InternalExecute(
        IViewModel context,
        CommandArg arg,
        CancellationToken cancel
    )
    {
        await _navigation.GoTo(pageId, arg, cancel);
        return null;
    }
}

public sealed class CommandService : ICommandService
{
    public ValueTask<CommandArg?> Execute(
        IViewModel context,
        string commandId,
        CommandArg arg,
        CancellationToken cancel = default
    )
    {
        return context.ExecuteCommand(commandId, arg, cancel);
    }
}

public sealed class NullCommandService : ICommandService
{
    public static NullCommandService Instance { get; } = new();

    private NullCommandService() { }

    public ValueTask<CommandArg?> Execute(
        IViewModel context,
        string commandId,
        CommandArg arg,
        CancellationToken cancel = default
    )
    {
        return ValueTask.FromResult<CommandArg?>(null);
    }
}

public static class CommandMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public CommandRegistryBuilder Commands => new(builder);
    }

    public static ValueTask<CommandArg?> ExecuteCommand(
        this IViewModel context,
        string commandId,
        CommandArg arg,
        CancellationToken cancel = default
    )
    {
        if (!CommandRegistry.TryGet(commandId, out var command))
        {
            throw new InvalidOperationException($"Command '{commandId}' is not registered.");
        }

        return command.Execute(context, arg, cancel);
    }

    public static IActionViewModel CreateAction(
        this ICommandInfo info,
        IViewModel context,
        CommandArg? parameter = null
    )
    {
        return new ActionViewModel(info.Id)
        {
            Header = info.Name,
            Description = info.Description,
            Icon = info.Icon,
            Command = new BindableAsyncCommand(info.Id, context),
            CommandParameter = parameter ?? CommandArg.Empty,
        };
    }

    public static IActionViewModel CreateAction(
        this ICommandInfo info,
        ILoggerFactory loggerFactory,
        CommandArg? parameter = null
    )
    {
        var action = new ActionViewModel(info.Id)
        {
            Header = info.Name,
            Description = info.Description,
            Icon = info.Icon,
            CommandParameter = parameter ?? CommandArg.Empty,
        };
        action.Command = new BindableAsyncCommand(info.Id, action);
        return action;
    }

    public static IDisposable Subscribe(
        this IRoutedEventController<IViewModel> events,
        Func<IViewModel, AsyncRoutedEvent<IViewModel>, ValueTask> handler
    )
    {
        return events.Catch((src, e, _) => handler(src, e));
    }
}

public sealed class CommandRegistryBuilder(IHostApplicationBuilder builder)
{
    public CommandRegistryBuilder Register<TCommand>()
        where TCommand : class, IAsyncCommand
    {
        builder.Services.TryAddSingleton<ICommandService, CommandService>();
        builder.Services.TryAddSingleton<INavigationService>(NullNavigationService.Instance);
        builder.Services.AddSingleton<TCommand>();
        CommandRegistry.Register(CreateCommand<TCommand>());
        return this;
    }

    private static TCommand CreateCommand<TCommand>()
        where TCommand : class, IAsyncCommand
    {
        try
        {
            if (Activator.CreateInstance(typeof(TCommand)) is TCommand command)
            {
                return command;
            }
        }
        catch (MissingMethodException) { }

        return
            Activator.CreateInstance(typeof(TCommand), NullNavigationService.Instance)
                is TCommand commandWithNavigation
            ? commandWithNavigation
            : throw new InvalidOperationException(
                $"Unable to create command instance of type {typeof(TCommand).FullName}."
            );
    }
}

internal static class CommandRegistry
{
    private static readonly Dictionary<string, IAsyncCommand> Commands = [];

    public static void Register(IAsyncCommand command)
    {
        Commands[command.Info.Id] = command;
    }

    public static bool TryGet(string id, out IAsyncCommand command)
    {
        return Commands.TryGetValue(id, out command!);
    }
}

public abstract class RoutableViewModel : ViewModel
{
    protected RoutableViewModel(string typeId)
        : base(typeId)
    {
        Logger = NullLogger.Instance;
    }

    protected RoutableViewModel(string typeId, ILoggerFactory loggerFactory)
        : base(typeId)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }

    protected RoutableViewModel(NavId id)
        : base(id.TypeId, id.Args)
    {
        Logger = NullLogger.Instance;
    }

    protected RoutableViewModel(NavId id, ILoggerFactory loggerFactory)
        : base(id.TypeId, id.Args)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }

    protected ILogger Logger { get; }
}

public abstract class ExtendableViewModel<TSelf> : ViewModel<TSelf>
    where TSelf : class
{
    protected ExtendableViewModel(NavId id, ILoggerFactory loggerFactory, IExtensionService ext)
        : base(id.TypeId, id.Args, ext)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }

    protected ExtendableViewModel(string typeId, IExtensionService ext)
        : base(typeId, NavArgs.Empty, ext)
    {
        Logger = NullLogger.Instance;
    }

    protected ILogger Logger { get; }
}

public class ProgressInPartsUnitItem : ProgressNormalizedUnitItem;
