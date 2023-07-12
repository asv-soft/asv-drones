using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Sdr;

public interface ISdrRttWidget : IViewModel
{
    int Order { get; }
    bool IsVisible { get; set; }
}