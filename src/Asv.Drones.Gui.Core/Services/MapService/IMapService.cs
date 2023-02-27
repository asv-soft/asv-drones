using Asv.Avalonia.Map;
using Asv.Common;

namespace Asv.Drones.Gui.Core
{
    public class MapServiceConfig
    {
        public string? MapProviderName { get; set; }
    }

    public interface IMapService
    {
        long CalculateMapCacheSize();
        string MapCacheDirectory { get; }
        IRxEditableValue<GMapProvider> CurrentMapProvider { get; }
        IEnumerable<GMapProvider> AvailableProviders { get; }
    }
}