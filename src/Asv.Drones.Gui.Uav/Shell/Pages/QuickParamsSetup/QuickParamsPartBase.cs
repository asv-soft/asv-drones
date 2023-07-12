using Asv.Drones.Gui.Core;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public abstract class QuickParamsPartBase : ViewModelBase, IQuickParamsPart
{
    public const string UriString = SettingsViewModel.UriString + ".quickparampart";
    
    public static readonly Uri Uri = new(UriString);
    
    protected QuickParamsPartBase(Uri id) : base(id)
    {
        IsVisible = true;

        IsSynced = true;
    }
    
    public abstract int Order { get; }
    
    public abstract bool IsRebootRequired { get; }
    
    public abstract bool IsVisibleInAdvancedMode { get; }
    
    [Reactive]
    public bool IsSynced { get; set; }
    
    [Reactive]
    public bool IsVisible { get; set; }
    
    public virtual Task Write()
    {
        throw new NotImplementedException();
    }

    public virtual Task Read()
    {
        throw new NotImplementedException();
    }
}