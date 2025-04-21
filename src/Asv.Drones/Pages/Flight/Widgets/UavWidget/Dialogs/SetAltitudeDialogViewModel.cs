using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asv.Avalonia;
using R3;

namespace Asv.Drones;

public class SetAltitudeDialogViewModel : DialogViewModelBase
{
    public const string DialogId = "dialog.altitude";
    private readonly INavigationService _navigation;

    public SetAltitudeDialogViewModel()
        : this(NullNavigationService.Instance, NullUnitService.Instance) { }

    public SetAltitudeDialogViewModel(INavigationService navigation, IUnitService unitService)
        : base(DialogId)
    {
        AltitudeUnit.Value = unitService.Units[AltitudeBase.Id];
        _navigation = navigation;
        _sub1 = Altitude.EnableValidation(
            s =>
            {
                var valid = AltitudeUnit.Value.Current.Value.ValidateValue(s);
                return valid;
            },
            this,
            true
        );
    }

    public async Task<ContentDialogResult> ApplyDialog()
    {
        var dialog = new ContentDialog(_navigation)
        {
            PrimaryButtonText = RS.SetAltitudeDialogViewModel_ApplyDialog_PrimaryButton_TakeOff,
            SecondaryButtonText = RS.SetAltitudeDialogViewModel_ApplyDialog_SecondaryButton_Cancel,
            IsPrimaryButtonEnabled = IsValid.CurrentValue,
            IsSecondaryButtonEnabled = true,
            Content = this,
        };
        _sub2 = IsValid.Subscribe(enabled => dialog.IsPrimaryButtonEnabled = enabled);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            AltitudeResult.Value = AltitudeUnit.Value.Current.Value.ParseToSi(Altitude.Value);
        }

        return result;
    }

    public BindableReactiveProperty<IUnit> AltitudeUnit { get; } = new();
    public BindableReactiveProperty<string> Altitude { get; } = new();
    public ReactiveProperty<double> AltitudeResult { get; } = new();

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            Altitude.Dispose();
            AltitudeUnit.Dispose();
            AltitudeResult.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
