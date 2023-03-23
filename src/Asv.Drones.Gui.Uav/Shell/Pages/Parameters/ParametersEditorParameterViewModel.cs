using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class ParametersEditorParameterViewModel : ViewModelBase
{
    public const string UriString = ShellMenuItem.UriString + ".somebody";
    public static readonly Uri Uri = new Uri(UriString);
    
    public ParametersEditorParameterViewModel() : base(Uri)
    {
        IsRebootRequired = false;    
    }

    [Reactive]
    public string RawName { get; set; }
    
    [Reactive]
    public string Name { get; set; }

    [Reactive]
    public string Value { get; set; }

    [Reactive]
    public string Units { get; set; }

    [Reactive]
    public string RangeDescription { get; set; }

    [Reactive]
    public string Description { get; set; }

    [Reactive] 
    public bool IsRebootRequired { get; set; }
    
    public ICommand Pin { get; set; }
    
    public ICommand Write { get; set; }

    public ICommand Update { get; set; }

}