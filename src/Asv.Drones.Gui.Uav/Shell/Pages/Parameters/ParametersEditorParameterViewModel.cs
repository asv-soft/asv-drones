using System.Reactive;
using System.Reactive.Linq;
using System.Text;
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

        Value = (object)(parameterItem.Parameter.IntegerValue ?? (parameterItem.Parameter.RealValue ?? 0));

        StringBuilder builder = new StringBuilder();

        if (Parameter.Description.Min != null & Parameter.Description.Max != null)
        {
            builder.Append($"{Parameter.Description.Min} - {Parameter.Description.Max}");
        }
        
        if (Parameter.Description.Increment != null)
        {
            builder.Append($" by {Parameter.Description.Increment}");
        }
        
        if (Parameter.Description.UnitsDisplayName != null)
        {
            builder.Append($" {Parameter.Description.UnitsDisplayName}");
        }

        ValueDescription = builder.ToString();
    }

    [Reactive]
    public object Value { get; set; }

    [Reactive]
    public string ValueDescription { get; set; }

    [Reactive]
    public ParameterItem Parameter { get; set; }
    
    [Reactive]
    public ICommand Write { get; set; }
    
    [Reactive]
    public ICommand Update { get; set; }

}