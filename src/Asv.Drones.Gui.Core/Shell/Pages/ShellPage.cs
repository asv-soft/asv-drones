namespace Asv.Drones.Gui.Core;

public class ShellPage : ViewModelBase,IShellPage
{
    protected ShellPage(Uri uri) : base(uri)
    {
            
    }
    protected ShellPage(string uri) : base(uri)
    {
            
    }

    public virtual void SetArgs(Uri link)
    {
            
    }
}