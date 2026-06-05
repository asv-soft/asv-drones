using Asv.Avalonia;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Api;

public sealed class MavParamButtonPropertyViewModel
    : PropertyButtonViewModel,
        IMavParamPropertyViewModel,
        ISupportRefresh
{
    private readonly IMavParamContext _context;

    public const string TypeId = nameof(MavParamWidgetType.Button);

    public MavParamButtonPropertyViewModel(IMavParamContext context)
        : base(context.Info.Id, cancel => Execute(context, cancel))
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
        this.ApplyMavParamMetadata(Info, Client, Refresh);
        if (Icon is null)
        {
            Icon = MaterialIconKind.PlayCircle;
        }
    }

    public MavParamInfo Info => _context.Info;

    private IParamsClientEx Client => _context.Client;

    public async ValueTask Refresh(CancellationToken cancel = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await Client.GetFromCacheOrReadOnce(Info.Metadata.Name, cancel).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            ApplyErrorFromModel(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async ValueTask Execute(IMavParamContext context, CancellationToken cancel)
    {
        var value = context.Info.Convert((ValueType)1);
        await context.Client.WriteOnce(context.Info.Metadata.Name, value, cancel);
    }
}
