using System.Threading;
using System.Threading.Tasks;

namespace Asv.Drones.Gui;

public interface IQuickParameter
{
    public int Order { get; set; }
    string ParameterTitle { get; set; }
    string ParameterName { get; set; }
    string ParameterDescription { get; set; }
    bool IsChanged { get; set; }
    Task WriteParam(CancellationToken cancel = default);
    Task ReadParam(CancellationToken cancel = default);
}