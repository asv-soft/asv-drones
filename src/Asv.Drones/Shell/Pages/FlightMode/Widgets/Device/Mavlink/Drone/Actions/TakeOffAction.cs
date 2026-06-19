using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class TakeOffAction<TWidget>(IUnitService unitService, ILoggerFactory loggerFactory)
    : FlightWidgetAction<TWidget>("take-off")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        var control = widget.Device.GetMicroservice<IControlClient>();
        if (control is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.TakeOffAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.AeroplaneTakeoff;
        item.Description = RS.TakeOffAction_TryCreateAction_Description;
        item.Order = 40;
        item.Command = CreateCommand(
                item,
                async ct =>
                {
                    var altitudeUnit = unitService.Units[AltitudeUnit.Id];

                    using var vm = new SetAltitudeDialogViewModel(altitudeUnit, loggerFactory);
                    var dialog = new ContentDialog(vm)
                    {
                        Title = RS.UavWidgetViewModel_SetAltitudeDialog_Title,
                        PrimaryButtonText =
                            RS.SetAltitudeDialogViewModel_ApplyDialog_PrimaryButton_TakeOff,
                        SecondaryButtonText =
                            RS.SetAltitudeDialogViewModel_ApplyDialog_SecondaryButton_Cancel,
                        IsSecondaryButtonEnabled = true,
                    };
                    vm.ApplyDialog(dialog);

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        var altitude = altitudeUnit.CurrentUnitItem.Value.ParseToSi(
                            vm.Altitude.Value
                        );
                        await control.TakeOff(altitude, ct);
                    }
                }
            )
            .DisposeItWith(contextDispose);
        return item;
    }
}
