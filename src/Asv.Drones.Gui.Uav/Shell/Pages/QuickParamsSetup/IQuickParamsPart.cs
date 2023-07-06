using Asv.Common;
using Asv.Drones.Gui.Core;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public interface IQuickParamsPart : IViewModel
{
    int Order { get; }
    
    bool IsRebootRequired { get; }

    bool IsVisibleInAdvancedMode { get; }
    
    bool IsSynced { get; set; }
    
    bool IsVisible { get; set; }
    
    Task Write();
    
    Task Read();
}