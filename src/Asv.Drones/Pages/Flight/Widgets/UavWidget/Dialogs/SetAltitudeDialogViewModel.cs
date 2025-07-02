using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class SetAltitudeDialogViewModel : DialogViewModelBase
{
    public const string DialogId = "dialog.altitude";

    public SetAltitudeDialogViewModel()
        : this(new NullUnitBase([new NullUnitItemInternational()]), DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SetAltitudeDialogViewModel(IUnit unit, ILoggerFactory loggerFactory)
        : base(DialogId, loggerFactory)
    {
        AltitudeUnit =
            unit as AltitudeBase
            ?? throw new InvalidCastException($"Unit must be an {nameof(AltitudeBase)}");
        AltitudeUnitSymbol = AltitudeUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        _sub1 = Altitude.EnableValidation(
            s =>
            {
                var valid = AltitudeUnit.CurrentUnitItem.Value.ValidateValue(s);
                return valid;
            },
            this,
            true
        );
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        _sub2?.Dispose();
        _sub2 = null;

        _sub2 = IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);
    }

    public AltitudeBase AltitudeUnit { get; }
    public BindableReactiveProperty<string> Altitude { get; } = new();
    public BindableReactiveProperty<string> AltitudeUnitSymbol { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private IDisposable? _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2?.Dispose();
            Altitude.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
