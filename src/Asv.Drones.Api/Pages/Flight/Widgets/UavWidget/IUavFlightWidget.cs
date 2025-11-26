using Asv.Avalonia;
using Asv.IO;

namespace Asv.Drones.Api;

public interface IUavFlightWidget : IWorkspaceWidget
{
    IClientDevice Device { get; }
}
