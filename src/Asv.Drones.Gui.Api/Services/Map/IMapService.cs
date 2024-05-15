using Asv.Avalonia.Map;
using Asv.Common;

namespace Asv.Drones.Gui.Api
{
    public interface IMapService
    {
        long CalculateMapCacheSize();
        void SetMapCacheDirectory(string path);
        string MapCacheDirectory { get; }
        IRxEditableValue<GMapProvider> CurrentMapProvider { get; }
        IEnumerable<GMapProvider> AvailableProviders { get; }
        IRxEditableValue<AccessMode> CurrentMapAccessMode { get; }
        IEnumerable<AccessMode> AvailableAccessModes { get; }
    }
}