using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class VoltageUavRttItemViewModel : UavRttItem
{

    public VoltageUavRttItemViewModel()
    {
        Voltage = "0.7 V";
    }

    public VoltageUavRttItemViewModel(IVehicle vehicle, ILocalizationService localization) : base(vehicle,
        GenerateRtt(vehicle, "voltage"))
    {
        Order = 5;

        Vehicle.VoltageBattery
            .DistinctUntilChanged()
            .Sample(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Voltage = $"{localization.Voltage.ConvertToString(_)} {localization.Voltage.GetUnit(_)}")
            .DisposeWith(Disposable);

    }

    [Reactive] public string Voltage { get; set; }
} 