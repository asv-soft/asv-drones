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

    public IMavParamPropertyViewModel Create(IMavParamTypeMetadata param, IParamsClientEx client)
    {
        var info = new MavParamInfo(param);
        var context = new MavParamContext(info, client);
        if (info.WidgetType != null)
        {
            return _container.CreateViewModel<IMavParamPropertyViewModel, IMavParamContext>(
                info.WidgetType,
                context
            );
        }

        return new MavParamTextBoxPropertyViewModel(context);
    }
}

public class MavParamTextBoxPropertyViewModel : MavParamPropertyViewModel
{
    public const string TypeId = "textbox";

    public MavParamTextBoxPropertyViewModel(IMavParamContext context)
        : base(context) { }
}

public class MavParamTextBoxPropertyView : PropertyTextBoxView { }
