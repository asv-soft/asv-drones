using Asv.Avalonia;
using Asv.Avalonia.InfoMessage;
using R3;

namespace Asv.Drones.Api;

/// <summary>
/// Base class for a flight-widget action. It decides whether it applies to the given widget (capability gate)
/// and, if so, adds a <see cref="IMenuItem"/> to the widget's <see cref="IFlightWidget.Menu"/>.
/// <para/>
/// Target selection is done purely via registration: register against <c>IDroneFlightWidget</c>,
/// <c>IPlaneWidget</c>, or both. Resolution is by the widget's exact <c>TSelf</c> type, so an action
/// bound to a more specific widget interface simply won't be offered to the others.
/// </summary>
/// <typeparam name="TWidget">The widget interface this action attaches to.</typeparam>
public abstract class FlightWidgetAction<TWidget> : IExtensionFor<TWidget>
    where TWidget : class, IFlightWidget
{
    private const string BaseId = "drone-action.";

    protected FlightWidgetAction(string id)
    {
        ActionId = BaseId + id;
    }

    protected string ActionId { get; }

    public void Extend(TWidget context, CompositeDisposable contextDispose)
    {
        var action = TryCreateAction(context, contextDispose);
        if (action is not null)
        {
            context.Menu.Add(action);
        }
    }

    protected abstract IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    );

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
