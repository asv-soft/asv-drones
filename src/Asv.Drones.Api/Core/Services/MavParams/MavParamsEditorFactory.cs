using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public class MavParamsEditorFactory : IMavParamsEditorFactory
{
    private readonly IServiceProvider _container;

    public MavParamsEditorFactory(IServiceProvider container)
    {
        ArgumentNullException.ThrowIfNull(container);
        _container = container;
    }

    public IEnumerable<IMavParamPropertyViewModel> CreateList(
        IParamsClientEx client,
        params IEnumerable<IMavParamTypeMetadata> paramList
    )
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(paramList);

        foreach (var param in paramList)
        {
            var editor = Create(param, client);
            if (editor is not null)
            {
                yield return editor;
            }
        }
    }

    public IMavParamPropertyViewModel? Create(IMavParamTypeMetadata param, IParamsClientEx client)
    {
        ArgumentNullException.ThrowIfNull(param);
        ArgumentNullException.ThrowIfNull(client);

        var info = new MavParamInfo(param);
        if (MavParamWidgetIds.GetIdByName(info.WidgetType) == MavParamWidgetIds.Hidden)
        {
            return null;
        }

        var context = new MavParamContext(info, client);
        foreach (var key in GetWidgetTypeKeys(info.WidgetType))
        {
            var editor = _container.TryCreateViewModel<
                IMavParamPropertyViewModel,
                IMavParamContext
            >(key, context);
            if (editor is not null)
            {
                return editor;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetWidgetTypeKeys(string? widgetType)
    {
        if (!string.IsNullOrWhiteSpace(widgetType))
        {
            var widgetId = MavParamWidgetIds.GetIdByName(widgetType);
            yield return widgetId;

            var rawWidgetType = widgetType.Trim();
            if (rawWidgetType != widgetId)
            {
                yield return rawWidgetType;
            }
        }

        yield return MavParamTextBoxPropertyViewModel.TypeId;
    }
}
