using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class ParametersEditorParameterViewModel : ViewModelBase
{
    public const string UriString = ShellMenuItem.UriString + ".parameter";
    public static readonly Uri Uri = new Uri(UriString);
    
    public ParametersEditorParameterViewModel() : base(Uri)
    {
        
    }

    public ParametersEditorParameterViewModel(ParameterItem parameterItem) : this()
    {
        Parameter = parameterItem;
    }
    
    public ParameterItem Parameter { get; }
    
    public ICommand Write { get; set; }

    public ICommand Update { get; set; }

}