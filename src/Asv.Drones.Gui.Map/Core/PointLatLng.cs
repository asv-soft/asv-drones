namespace Asv.Avalonia.Map
{
    // /// <summary>
    // ///     the point of coordinates
    // /// </summary>
    // [Serializable]
    // public struct GeoPoint
    // {
    //     public static readonly GeoPoint Empty = new GeoPoint();
    //     private double _lat;
    //     private double _lng;
    //
    //     bool _notEmpty;
    //
    //     public GeoPoint(double lat, double lng)
    //     {
    //         _lat = lat;
    //         _lng = lng;
    //         _notEmpty = true;
    //     }
    //
    //     /// <summary>
    //     ///     returns true if coordinates wasn't assigned
    //     /// </summary>
    //     public bool IsEmpty
    //     {
    //         get
    //         {
    //             return !_notEmpty;
    //         }
    //     }
    //
    //     public double Lat
    //     {
    //         get
    //         {
    //             return _lat;
    //         }
    //         set
    //         {
    //             _lat = value;
    //             _notEmpty = true;
    //         }
    //     }
    //
    //     public double Lng
    //     {
    //         get
    //         {
    //             return _lng;
    //         }
    //         set
    //         {
    //             _lng = value;
    //             _notEmpty = true;
    //         }
    //     }
    //
    //     public static GeoPoint operator +(GeoPoint pt, SizeLatLng sz)
    //     {
    //         return Add(pt, sz);
    //     }
    //
    //     public static GeoPoint operator -(GeoPoint pt, SizeLatLng sz)
    //     {
    //         return Subtract(pt, sz);
    //     }
    //
    //     public static SizeLatLng operator -(GeoPoint pt1, GeoPoint pt2)
    //     {
    //         return new SizeLatLng(pt1.Lat - pt2.Lat, pt2.Lng - pt1.Lng);
    //     }
    //
    //     public static bool operator ==(GeoPoint left, GeoPoint right)
    //     {
    //         return left.Lng == right.Lng && left.Lat == right.Lat;
    //     }
    //
    //     public static bool operator !=(GeoPoint left, GeoPoint right)
    //     {
    //         return !(left == right);
    //     }
    //
    //     public static GeoPoint Add(GeoPoint pt, SizeLatLng sz)
    //     {
    //         return new GeoPoint(pt.Lat - sz.HeightLat, pt.Lng + sz.WidthLng);
    //     }
    //
    //     public static GeoPoint Subtract(GeoPoint pt, SizeLatLng sz)
    //     {
    //         return new GeoPoint(pt.Lat + sz.HeightLat, pt.Lng - sz.WidthLng);
    //     }
    //
    //     public override bool Equals(object obj)
    //     {
    //         if (!(obj is GeoPoint))
    //         {
    //             return false;
    //         }
    //
    //         GeoPoint tf = (GeoPoint)obj;
    //         return tf.Lng == Lng && tf.Lat == Lat && tf.GetType().Equals(GetType());
    //     }
    //
    //     public void Offset(GeoPoint pos)
    //     {
    //         Offset(pos.Lat, pos.Lng);
    //     }
    //
    //     public void Offset(double lat, double lng)
    //     {
    //         Lng += lng;
    //         Lat -= lat;
    //     }
    //
    //     public override int GetHashCode()
    //     {
    //         return Lng.GetHashCode() ^ Lat.GetHashCode();
    //     }
    //
    //     public override string ToString()
    //     {
    //         return string.Format(CultureInfo.CurrentCulture, "{{Lat={0}, Lng={1}}}", Lat, Lng);
    //     }
    // }
}
