using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public interface ITreePageMenuItem : IViewModel
{
    Uri ParentId { get; }
    string? Name { get; }
    string? Description { get; }
    MaterialIconKind Icon { get; }
    int Order { get; }
    public string? Status { get; }
    ITreePage? CreatePage(ITreePageContext context);
}

public abstract class TreePageMenuItem : ViewModelBase, ITreePageMenuItem
{
    public abstract Uri ParentId { get; }
    public abstract string? Name { get; }
    public abstract string? Description { get; }
    public abstract MaterialIconKind Icon { get; }
    public abstract int Order { get; }
    [Reactive] public string? Status { get; set; }

    public abstract ITreePage? CreatePage(ITreePageContext context);

    protected TreePageMenuItem(Uri id) : base(id)
    {
    }

    protected TreePageMenuItem(string id) : base(id)
    {
    }
}

public class TreePageCallbackMenuItem : TreePageMenuItem
{
    private readonly Func<ITreePageContext, ITreePage>? _factory;

    public TreePageCallbackMenuItem(Uri id, string name, MaterialIconKind icon, string? desc = null, int order = 0,
        Uri? parentId = null, Func<ITreePageContext, ITreePage>? factory = null) : base(id)
    {
        ParentId = parentId ?? WellKnownUri.UndefinedUri;
        Name = name;
        Description = desc;
        Icon = icon;
        Order = order;
        _factory = factory;
    }

    public TreePageCallbackMenuItem(string id, string name, MaterialIconKind icon, string? desc = null, int order = 0,
        Uri? parentId = null, Func<ITreePageContext, ITreePage>? factory = null) : this(new Uri(id), name, icon, desc,
        order, parentId, factory)
    {
    }

    public override Uri ParentId { get; }
    public override string? Name { get; }
    public override string? Description { get; }
    public override MaterialIconKind Icon { get; }
    public override int Order { get; }

    public override ITreePage? CreatePage(ITreePageContext context)
    {
        return _factory?.Invoke(context);
    }
}