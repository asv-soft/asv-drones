using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Drones.Gui.Api;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public abstract class QuickParameterViewModel : ViewModelBase, IQuickParameter
{
    protected QuickParameterViewModel(Uri id) : base(id)
    {
        
    }
    
    protected QuickParameterViewModel(string id) : base(id)
    {
        
    }

    [Reactive] public int Order { get; set; }
    [Reactive] public string ParameterTitle { get; set; }
    [Reactive] public string ParameterName { get; set; }
    [Reactive] public string ParameterDescription { get; set; }
    [Reactive] public bool IsChanged { get; set; }
    [Reactive] public bool IsInitialized { get; set; }
    public virtual async Task WriteParam(CancellationToken cancel)
    {
        throw new NotImplementedException();
    }

    public virtual async Task ReadParam(CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}