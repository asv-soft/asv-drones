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
        ILoggerFactory loggerFactory
    )
    {
        var info = new MavParamInfo(metadata);
        switch (info.WidgetType)
        {
            case MavParamWidgetType.Button:
                return new MavParamButtonViewModel(info, update, initReadCallback, loggerFactory);
            case MavParamWidgetType.ComboBox:
                return new MavParamComboBoxViewModel(info, update, initReadCallback, loggerFactory);
            case MavParamWidgetType.TextBox:
            default:
                return new MavParamTextBoxViewModel(info, update, initReadCallback, loggerFactory);
        }
    }

    public static MavParamViewModel Create(
        IMavParamTypeMetadata param,
        IParamsClientEx svc,
        ILoggerFactory loggerFactory
    )
    {
        return Create(param, svc.Filter(param.Name), svc.GetFromCacheOrReadOnce, loggerFactory);
    }
}
