using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones.Api;

public class MavParamsEditorFactory : IMavParamsEditorFactory
{
    private readonly IServiceProvider _container;

    public MavParamsEditorFactory(IServiceProvider container)
    {
        ArgumentNullException.ThrowIfNull(container);
        _container = container;
    }

    public IMavParamPropertyViewModel Create(IMavParamTypeMetadata param, IParamsClientEx client)
    {
        ArgumentNullException.ThrowIfNull(param);
        ArgumentNullException.ThrowIfNull(client);

        var info = new MavParamInfo(param);
        var context = new MavParamContext(info, client);
        foreach (var key in GetWidgetTypeKeys(info.WidgetType))
        {
            var factory = _container.GetKeyedService<ViewModelMixin.FactoryDelegate<
                IMavParamPropertyViewModel,
                IMavParamContext
            >>(key);
            if (factory is not null)
            {
                return factory(context);
            }
        }

        return _container.CreateViewModel<IMavParamPropertyViewModel, IMavParamContext>(
            MavParamTextBoxPropertyViewModel.TypeId,
            context
        );
    }

    private static IEnumerable<string> GetWidgetTypeKeys(string? widgetType)
    {
        if (!string.IsNullOrWhiteSpace(widgetType))
        {
            yield return widgetType.Trim();

            var normalizedWidgetType = widgetType
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .Replace(" ", string.Empty);
            if (
                Enum.TryParse<MavParamWidgetType>(
                    normalizedWidgetType,
                    true,
                    out var knownWidgetType
                )
            )
            {
                yield return knownWidgetType.ToString();
            }
        }

        yield return MavParamTextBoxPropertyViewModel.TypeId;
    }
}
