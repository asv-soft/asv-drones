using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Api;

public static class MavParamFactory
{
    public static MavParamViewModel Create(
        IMavParamTypeMetadata metadata,
        Observable<MavParamValue> update,
        InitialReadParamDelegate initReadCallback,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IUnitService measureService
    )
    {
        var info = new MavParamInfo(metadata);
        return info.WidgetType switch
        {
            MavParamWidgetType.Altitude => new MavParamAltitudeTextBoxViewModel(
                info,
                update,
                initReadCallback,
                layoutService,
                loggerFactory,
                measureService
            ),
            MavParamWidgetType.Latitude => new MavParamLatLonTextBoxViewModel(
                info,
                update,
                initReadCallback,
                layoutService,
                loggerFactory,
                measureService,
                true
            ),
            MavParamWidgetType.Longitude => new MavParamLatLonTextBoxViewModel(
                info,
                update,
                initReadCallback,
                layoutService,
                loggerFactory,
                measureService,
                false
            ),
            MavParamWidgetType.Button => new MavParamButtonViewModel(
                info,
                update,
                initReadCallback,
                layoutService,
                loggerFactory
            ),
            MavParamWidgetType.ComboBox => new MavParamComboBoxViewModel(
                info,
                update,
                initReadCallback,
                layoutService,
                loggerFactory
            ),
            _ => new MavParamTextBoxViewModel(
                info,
                update,
                initReadCallback,
                layoutService,
                loggerFactory
            ),
        };
    }

    public static MavParamViewModel Create(
        IMavParamTypeMetadata param,
        IParamsClientEx svc,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IUnitService measureService
    )
    {
        return Create(
            param,
            svc.Filter(param.Name),
            svc.GetFromCacheOrReadOnce,
            layoutService,
            loggerFactory,
            measureService
        );
    }
}
