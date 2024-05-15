using System;
using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(ITreePageMenuItem))]
[method: ImportingConstructor]
public class MeasureTreeMenuItem(ILocalizationService loc)
    : TreePageMenuItem(WellKnownUri.ShellPageSettingsMeasure)
{
    public override Uri ParentId => WellKnownUri.ShellPageSettingsAppearanceUri;
    public override string? Name => RS.MeasureUnitsSettingsViewModel_MeasurementUnits_Header_Title;
    public override string? Description => RS.MeasureUnitsSettingsViewModel_MeasurementUnits_Header_Description;
    public override MaterialIconKind Icon => MaterialIconKind.TemperatureCelsius;
    public override int Order => ushort.MaxValue;

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return new MeasureViewModel((ISettingsPageContext)context, loc);
    }
}