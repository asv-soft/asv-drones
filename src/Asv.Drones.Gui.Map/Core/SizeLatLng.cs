using System.Globalization;
using Asv.Common;

namespace Asv.Avalonia.Map
{
    /// <summary>
    ///     the size of coordinates
    /// </summary>
    public struct SizeLatLng
    {
        public static readonly SizeLatLng Empty;

        public SizeLatLng(SizeLatLng size)
        {
            WidthLng = size.WidthLng;
            HeightLat = size.HeightLat;
        }

        public SizeLatLng(GeoPoint pt)
        {
            HeightLat = pt.Latitude;
            WidthLng = pt.Longitude;
        }

        public SizeLatLng(double heightLat, double widthLng)
        {
            HeightLat = heightLat;
            WidthLng = widthLng;
        }

        public static SizeLatLng operator +(SizeLatLng sz1, SizeLatLng sz2)
        {
            return Add(sz1, sz2);
        }

        public static SizeLatLng operator -(SizeLatLng sz1, SizeLatLng sz2)
        {
            return Subtract(sz1, sz2);
        }

        public static bool operator ==(SizeLatLng sz1, SizeLatLng sz2)
        {
            return sz1.WidthLng == sz2.WidthLng && sz1.HeightLat == sz2.HeightLat;
        }

        public static bool operator !=(SizeLatLng sz1, SizeLatLng sz2)
        {
            return !(sz1 == sz2);
        }

        public static explicit operator GeoPoint(SizeLatLng size)
        {
            return new GeoPoint(size.HeightLat, size.WidthLng,0);
        }

        public bool IsEmpty => WidthLng == 0d && HeightLat == 0d;

        public double WidthLng
        {
            get;
            set;
        }

        public double HeightLat
        {
            get;
            set;
        }

        public static SizeLatLng Add(SizeLatLng sz1, SizeLatLng sz2)
        {
            return new SizeLatLng(sz1.HeightLat + sz2.HeightLat, sz1.WidthLng + sz2.WidthLng);
        }

        public static SizeLatLng Subtract(SizeLatLng sz1, SizeLatLng sz2)
        {
            return new SizeLatLng(sz1.HeightLat - sz2.HeightLat, sz1.WidthLng - sz2.WidthLng);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SizeLatLng))
            {
                return false;
            }

            SizeLatLng ef = (SizeLatLng)obj;
            return ef.WidthLng == WidthLng && ef.HeightLat == HeightLat &&
                   ef.GetType().Equals(GetType());
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
            {
                return 0;
            }

            return WidthLng.GetHashCode() ^ HeightLat.GetHashCode();
        }

        public GeoPoint ToGeoPoint()
        {
            return (GeoPoint)this;
        }

        public override string ToString()
        {
            return "{WidthLng=" + WidthLng.ToString(CultureInfo.CurrentCulture) + ", HeightLng=" +
                   HeightLat.ToString(CultureInfo.CurrentCulture) + "}";
        }

        static SizeLatLng()
        {
            Empty = new SizeLatLng();
        }
    }
}
