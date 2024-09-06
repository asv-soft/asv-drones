using System;
using System.Composition;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui;

public class TakeOffViewModelConfig
{
    public double TakeOffAltitudeMeter { get; set; } = 30;
}

public class TakeOffViewModel : DisposableReactiveObjectWithValidation
{
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _loc;

    private readonly TakeOffViewModelConfig _config;
    public const double MinimumAltitudeMeter = 1;

    [ImportingConstructor]
    public TakeOffViewModel(IConfiguration cfg, ILocalizationService loc)
    {
        _cfg = cfg;
        _loc = loc;
        _config = cfg.Get<TakeOffViewModelConfig>();
        Altitude = _loc.Altitude.FromSiToString(_config.TakeOffAltitudeMeter);

        this.ValidationRule(x => x.Altitude, 
            _ => _loc.Altitude.IsValid(_), 
            _ => _loc.Altitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Altitude,
                _ => _loc.Altitude.IsValid(_) && _loc.Altitude.ConvertToSi(_) >= MinimumAltitudeMeter,
                string.Format(RS.TakeOffAnchorActionViewModel_ValidValue,
                    _loc.Altitude.FromSiToString(MinimumAltitudeMeter)))
            .DisposeItWith(Disposable);
    }

    public TakeOffViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        dialog.PrimaryButtonCommand =
            ReactiveCommand.Create(() =>
                {
                    _config.TakeOffAltitudeMeter = _loc.Altitude.ConvertToSi(Altitude);
                    _cfg.Set(_config);
                },
                this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
                .DisposeItWith(Disposable);
    }

    [Reactive] public string Altitude { get; set; }

    public string Units => _loc.Altitude.CurrentUnit.Value.Unit;
}