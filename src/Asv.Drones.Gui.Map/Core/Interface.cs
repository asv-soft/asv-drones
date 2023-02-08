using Asv.Common;

namespace Asv.Avalonia.Map
{
    public interface Interface
    {
        GeoPoint Position
        {
            get;
            set;
        }

        GPoint PositionPixel
        {
            get;
        }

        string CacheLocation
        {
            get;
            set;
        }

        bool IsDragging
        {
            get;
        }

        RectLatLng ViewArea
        {
            get;
        }

        GMapProvider MapProvider
        {
            get;
            set;
        }

        bool CanDragMap
        {
            get;
            set;
        }

        RenderMode RenderMode
        {
            get;
        }

        // events
        event PositionChanged OnPositionChanged;
        event TileLoadComplete OnTileLoadComplete;
        event TileLoadStart OnTileLoadStart;
        event MapDrag OnMapDrag;
        event MapZoomChanged OnMapZoomChanged;
        event MapTypeChanged OnMapTypeChanged;

        void ReloadMap();

        GeoPoint FromLocalToLatLng(int x, int y);
        GPoint FromLatLngToLocal(GeoPoint point);

#if SQLite
        bool ShowExportDialog();
        bool ShowImportDialog();
#endif
    }
}
