using Asv.Avalonia;
using Asv.Avalonia.InfoMessage;
using R3;

namespace Asv.Drones.Api;

public abstract class MenuAction<TTarget> : IExtensionFor<TTarget>
    where TTarget : class, IMenuActionTarget
{
    private readonly string _actionId;

    protected MenuAction(string baseId, string id)
    {
        _actionId = baseId + id;
    }

    public abstract string Id { get; }

    public void Extend(TTarget context, CompositeDisposable contextDispose)
    {
        var action = TryCreateAction(context, contextDispose);
        if (action is not null)
        {
            context.Menu.Add(action);
        }
    }

    protected abstract IMenuItem? TryCreateAction(
        TTarget target,
        CompositeDisposable contextDispose
    );

    protected MenuItem CreateMenuItem(string header)
    {
        return new MenuItem(_actionId, header);
    }

    protected static ReactiveCommand CreateCommand(
        IViewModel owner,
        Func<CancellationToken, ValueTask> execute
    )
    {
        return new ReactiveCommand(
            async (_, ct) =>
            {
                try
                {
                    await execute(ct);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await owner.RiseShellErrorMessage(
                        RS.FlightWidgetAction_CreateCommand_Title,
                        ex.Message,
                        ex,
                        ct
                    );
                }
            }
        );
    }
}
