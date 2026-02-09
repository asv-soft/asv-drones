using System;
using System.Collections.Generic;
using System.Linq;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class SetAltitudeDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.altitude";

    public SetAltitudeDialogViewModel()
        : this(
            NullUnitService.Instance.Units.Values.First(u => u.UnitId == AltitudeUnit.Id),
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SetAltitudeDialogViewModel(IUnit unit, ILoggerFactory loggerFactory)
        : base(DialogId, loggerFactory)
    {
        var altitudeUnit =
            unit as AltitudeUnit
            ?? throw new InvalidCastException($"Unit must be an {nameof(AltitudeUnit)}");

        Altitude = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        AltitudeUnitSymbol = altitudeUnit
            .CurrentUnitItem.ObserveOnUIThreadDispatcher()
            .Select(item => item.Symbol)
            .ToReadOnlyBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        Altitude
            .EnableValidationRoutable(
                s =>
                {
                    var valid = altitudeUnit.CurrentUnitItem.Value.ValidateValue(s);
                    return valid;
                },
                this,
                true
            )
            .DisposeItWith(Disposable);
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        _sub2.Disposable = IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    public BindableReactiveProperty<string> Altitude { get; }
    public IReadOnlyBindableReactiveProperty<string> AltitudeUnitSymbol { get; }

    #region Dispose

    private readonly SerialDisposable _sub2 = new();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub2.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
