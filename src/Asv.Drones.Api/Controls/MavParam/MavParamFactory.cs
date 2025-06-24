using System.Collections.Specialized;
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
                loggerFactory,
                measureService
            ),
            MavParamWidgetType.Latitude => new MavParamLatLonTextBoxViewModel(
                info,
                update,
                initReadCallback,
                loggerFactory,
                measureService,
                true
            ),
            MavParamWidgetType.Longitude => new MavParamLatLonTextBoxViewModel(
                info,
                update,
                initReadCallback,
                loggerFactory,
                measureService,
                false
            ),
            MavParamWidgetType.Button => new MavParamButtonViewModel(
                info,
                update,
                initReadCallback,
                loggerFactory
            ),
            MavParamWidgetType.ComboBox => new MavParamComboBoxViewModel(
                info,
                update,
                initReadCallback,
                loggerFactory
            ),
            _ => new MavParamTextBoxViewModel(info, update, initReadCallback, loggerFactory),
        };
    }

    public static MavParamViewModel Create(
        IMavParamTypeMetadata param,
        IParamsClientEx svc,
        ILoggerFactory loggerFactory,
        IUnitService measureService
    )
    {
        return Create(
            param,
            svc.Filter(param.Name),
            svc.GetFromCacheOrReadOnce,
            loggerFactory,
            measureService
        );
    }
}
