using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class ShellPage : ViewModelBase,IShellPage
{
    protected ShellPage(Uri uri) : base(uri)
    {
            
    }
    protected ShellPage(string uri) : base(uri)
    {
            
    }
    
    [Reactive]
    public MaterialIconKind Icon { get; set; }
    [Reactive]
    public string Title { get; set; }

    public virtual void SetArgs(Uri link)
    {
            
    }

    public virtual Task<bool> TryClose()
    {
        return Task.FromResult(true);
    }
}