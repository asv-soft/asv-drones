using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones.Api;

public interface IMavParamsEditorFactory
{
    IMavParamPropertyViewModel Create(IMavParamTypeMetadata param, IParamsClientEx client);
}

public interface IMavParamPropertyViewModel : IPropertyViewModel { }

public class MavParamPropertyViewModel : PropertyViewModel, IMavParamPropertyViewModel
{
    private readonly IMavParamContext _context;

    public MavParamPropertyViewModel(IMavParamContext context)
        : base(context.Info.Id)
    {
        _context = context;
        Header = Info.Title;
        ShortHeader = Info.Metadata.Name;
        Description = Info.Description;
        Icon = Info.Icon;
        if (Info.IconColor.HasValue)
        {
            IconColor = Info.IconColor.Value;
        }
    }

    public MavParamInfo Info => _context.Info;
}
