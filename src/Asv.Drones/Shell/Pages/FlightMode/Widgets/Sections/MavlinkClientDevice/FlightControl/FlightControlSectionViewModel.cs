using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public class FlightControlSectionViewModel : ViewModel, IFlightWidgetSection
{
    public const string SectionId = "flight-control-widget-section";

    private readonly IUnit _altitudeUnit = null!;
    private readonly ILogger<FlightControlSectionViewModel> _logger;

    public FlightControlSectionViewModel()
        : base(SectionId)
    {
        DesignTime.ThrowIfNotDesignMode();
        _logger = NullLoggerFactory.Instance.CreateLogger<FlightControlSectionViewModel>();
        TakeOff = new ReactiveCommand();
        AutoMode = new ReactiveCommand();
        Rtl = new ReactiveCommand();
        Guided = new ReactiveCommand();

        Land = new ReactiveCommand();
    }

    public FlightControlSectionViewModel(
        MavlinkClientDevice device,
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
        : base(SectionId)
    {
        Device = device;
        _logger = loggerFactory.CreateLogger<FlightControlSectionViewModel>();
        _altitudeUnit = unitService.Units[AltitudeUnit.Id];

        TakeOff = new ReactiveCommand(
            async (_, ct) =>
            {
                using var vm = new SetAltitudeDialogViewModel(_altitudeUnit, loggerFactory);
                var dialog = new ContentDialog(vm)
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
                    var altitude = _altitudeUnit.CurrentUnitItem.Value.ParseToSi(vm.Altitude.Value);
                    await ExecuteControlCommand(
                        (control, cancel) => control.TakeOff(altitude, cancel),
                        ct
                    );
                }
            }
        ).DisposeItWith(Disposable);
        Rtl = new ReactiveCommand(
            async (_, ct) =>
                await ExecuteControlCommand((control, cancel) => control.DoRtl(cancel), ct)
        ).DisposeItWith(Disposable);
        Land = new ReactiveCommand(
            async (_, ct) =>
                await ExecuteControlCommand((control, cancel) => control.DoLand(cancel), ct)
        ).DisposeItWith(Disposable);
        Guided = new ReactiveCommand(
            async (_, ct) =>
                await ExecuteControlCommand((control, cancel) => control.SetGuidedMode(cancel), ct)
        ).DisposeItWith(Disposable);
        AutoMode = new ReactiveCommand(
            async (_, ct) =>
                await ExecuteControlCommand((control, cancel) => control.SetAutoMode(cancel), ct)
        ).DisposeItWith(Disposable);
    }

    public ReactiveCommand TakeOff { get; }
    public ICommand AutoMode { get; }
    public ICommand Rtl { get; }
    public ICommand Land { get; }
    public ICommand Guided { get; }
    public int Order => 1;

    public IClientDevice? Device { get; }

    private async Task ExecuteControlCommand(
        Func<IControlClient, CancellationToken, Task> command,
        CancellationToken cancel
    )
    {
        cancel.ThrowIfCancellationRequested();

        var controlClient = Device?.GetMicroservice<IControlClient>();
        if (controlClient is null)
        {
            _logger.LogWarning(
                "Unable to load {Service} from {Device}",
                nameof(IControlClient),
                Device?.Id
            );
            return;
        }

        await command(controlClient, cancel);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
