using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

public interface ISdrRttProvider
{
    public SdrRttViewModelBase Create(ISdrClientDevice device, AsvSdrCustomMode mode);
}