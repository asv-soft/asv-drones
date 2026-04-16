using System.Collections.Generic;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class FlightControlViewModel : RoutableViewModel, IDashboardWidget
{
    private readonly IDeviceManager _deviceManager;
    private const string WidgetId = "widget-dashboard-item";

    private readonly IUnit _altitudeUnit;

    public FlightControlViewModel()
        : base(WidgetId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        TakeOff = new ReactiveCommand();
        AutoMode = new ReactiveCommand();
        Rtl = new ReactiveCommand();
        Guided = new ReactiveCommand();

        Land = new ReactiveCommand();
        InitArgs("1");
    }

    public FlightControlViewModel(
        INavigationService navigation,
        IUnitService unitService,
        IDeviceManager deviceManager,
        ILoggerFactory loggerFactory
    )
        : base(WidgetId, loggerFactory)
    {
        _deviceManager = deviceManager;

        _altitudeUnit = unitService.Units[AltitudeUnit.Id];

        TakeOff = new ReactiveCommand(
            async (_, ct) =>
            {
                using var vm = new SetAltitudeDialogViewModel(_altitudeUnit, loggerFactory);
                var dialog = new ContentDialog(vm, navigation)
                {
                    Title = RS.UavWidgetViewModel_SetAltitudeDialog_Title,
                    PrimaryButtonText =
                        RS.SetAltitudeDialogViewModel_ApplyDialog_PrimaryButton_TakeOff,
                    SecondaryButtonText =
                        RS.SetAltitudeDialogViewModel_ApplyDialog_SecondaryButton_Cancel,
                    IsSecondaryButtonEnabled = true,
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await this.ExecuteCommand(
                        TakeOffCommand.Id,
                        new DoubleArg(
                            _altitudeUnit.CurrentUnitItem.Value.ParseToSi(vm.Altitude.Value)
                        ),
                        ct
                    );
                }
            }
        ).DisposeItWith(Disposable);
        Rtl = new BindableAsyncCommand(RTLCommand.Id, this);
        Land = new BindableAsyncCommand(LandCommand.Id, this);
        Guided = new BindableAsyncCommand(GuidedModeCommand.Id, this);
        AutoMode = new BindableAsyncCommand(AutoModeCommand.Id, this);
    }

    public ReactiveCommand TakeOff { get; }
    public ICommand AutoMode { get; }
    public ICommand Rtl { get; }
    public ICommand Land { get; }
    public ICommand Guided { get; }

    public IClientDevice Device { get; private set; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    public DeviceId DeviceId { get; }

    public void Attach(DeviceId deviceId)
    {
        var device = _deviceManager.Explorer.Devices[deviceId];
        Device = device;
        InitArgs(device.Id.AsString());
    }
}
