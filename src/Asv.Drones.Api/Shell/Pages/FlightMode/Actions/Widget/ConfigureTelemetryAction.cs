using Asv.Avalonia;
using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class ConfigureTelemetryAction<TWidget>()
    : FlightWidgetAction<TWidget>("configure-telemetry")
    where TWidget : class, IFlightWidget
{
    public const string StaticId = "ext.flight-widget.action.configure-telemetry";
    public override string Id => StaticId;

    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        var telemetrySection = widget.Sections.OfType<ITelemetrySection>().FirstOrDefault();
        if (telemetrySection is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.ConfigureTelemetryAction_Header);
        item.Icon = MaterialIconKind.ViewDashboardEdit;
        item.Description = RS.ConfigureTelemetryAction_Description;
        item.Order = 100;
        item.Command = CreateCommand(
                item,
                async cancel =>
                {
                    cancel.ThrowIfCancellationRequested();
                    using var vm = new ConfigureTelemetryDialogViewModel(telemetrySection.Tiles);
                    var dialog = new ContentDialog(vm)
                    {
                        Title = RS.ConfigureTelemetryAction_DialogTitle,
                        PrimaryButtonText = RS.ConfigureTelemetryAction_DialogPrimaryButtonText,
                        CloseButtonText = RS.ConfigureTelemetryAction_DialogCloseButtonText,
                    };
                    var result = await dialog.ShowAsync();
                    cancel.ThrowIfCancellationRequested();

                    if (result == ContentDialogResult.Primary)
                    {
                        var visibility = vm.GetVisibility();
                        foreach (var tile in telemetrySection.Tiles)
                        {
                            if (visibility.TryGetValue(tile.Id.ToString(), out var isVisible))
                            {
                                tile.IsVisible = isVisible;
                            }
                        }
                    }
                }
            )
            .DisposeItWith(contextDispose);
        return item;
    }
}
