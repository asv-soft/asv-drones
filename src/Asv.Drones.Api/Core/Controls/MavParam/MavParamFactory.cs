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
        WriteParamDelegate? writeCallback,
        ILoggerFactory loggerFactory,
        IUnitService measureService
    )
    {
        var info = new MavParamInfo(metadata);
        return info.WidgetType switch
        {
            "AsciiChars" => new MavParamAsciiCharViewModel(
                info,
                update,
                initReadCallback,
                writeCallback,
                loggerFactory
            ),
            "Altitude" => new MavParamAltitudeTextBoxViewModel(
                info,
                update,
                initReadCallback,
                writeCallback,
                loggerFactory,
                measureService
            ),
            "Latitude" => new MavParamLatLonTextBoxViewModel(
                info,
                update,
                initReadCallback,
                writeCallback,
                loggerFactory,
                measureService,
                true
            ),
            "Longitude" => new MavParamLatLonTextBoxViewModel(
                info,
                update,
                initReadCallback,
                writeCallback,
                loggerFactory,
                measureService,
                false
            ),
            "Button" => new MavParamButtonViewModel(
                info,
                update,
                initReadCallback,
                writeCallback,
                loggerFactory
            ),
            "ComboBox" => new MavParamComboBoxViewModel(
                info,
                update,
                initReadCallback,
                writeCallback,
                loggerFactory
            ),
            _ => new MavParamTextBoxViewModel(
                info,
                update,
                initReadCallback,
                writeCallback,
                loggerFactory
            ),
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
            async (name, value, cancel) =>
            {
                await svc.WriteOnce(name, value, cancel);
            },
            loggerFactory,
            measureService
        );
    }
}
