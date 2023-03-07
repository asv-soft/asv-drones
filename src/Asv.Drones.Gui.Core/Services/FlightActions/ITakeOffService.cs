using Asv.Common;

namespace Asv.Drones.Gui.Core;

public interface ITakeOffService
{
    IRxEditableValue<double?> CurrentAltitude { get; }
}