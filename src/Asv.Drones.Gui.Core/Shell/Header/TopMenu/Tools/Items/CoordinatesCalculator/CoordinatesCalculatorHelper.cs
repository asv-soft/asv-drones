using Asv.Common;

namespace Asv.Drones.Gui.Core;

public static class CoordinatesCalculatorHelper
{
    // Parameters of ellipsoids WGS 84 and PZ-90
    private const double WGS84_A = 6378137;           // Semi-major axis WGS 84 (in meters)
    private const double WGS84_ECCENTRICITY_SQUARED = 0.00669437999014; // Square of eccentricity WGS 84
    private const double PZ90_A = 6378136;            // Semi-major axis PZ-90 (in meters)
    private const double PZ90_ECCENTRICITY_SQUARED = 0.00669436619; // Square of eccentricity PZ-90

    public static GeoPoint ConvertPz90ToWgs84(GeoPoint point)
    {
        // Convert degrees to radians
        double phiPz90 = point.Latitude * Math.PI / 180.0;
        double lambdaPz90 = point.Longitude * Math.PI / 180.0;

        // Curvature radii of meridian and parallel for PZ-90
        double nuPz90 = PZ90_A / Math.Sqrt(1 - PZ90_ECCENTRICITY_SQUARED * Math.Sin(phiPz90) * Math.Sin(phiPz90));
        double lambdaSinPz90 = Math.Sin(lambdaPz90);
        double lambdaCosPz90 = Math.Cos(lambdaPz90);
        double hPz90 = point.Altitude;

        // Geocentric coordinates PZ-90
        double xPz90 = (nuPz90 + hPz90) * Math.Cos(phiPz90) * lambdaCosPz90;
        double yPz90 = (nuPz90 + hPz90) * Math.Cos(phiPz90) * lambdaSinPz90;
        double zPz90 = ((1 - PZ90_ECCENTRICITY_SQUARED) * nuPz90 + hPz90) * Math.Sin(phiPz90);

        // Parameters for reverse transformation of coordinates from PZ-90 to WGS 84
        double dx = 1.11;
        double dy = -0.87;
        double dz = 1.08;
        double wx = 0.000000000189;
        double wy = 0.000000000369;
        double wz = 0.000000000890;
        double m = 1 - 0.000001137;

        // Reverse transformation of coordinates from PZ-90 to WGS 84
        double xWgs84 = dx + m * (xPz90 - wz * yPz90 + wy * zPz90);
        double yWgs84 = dy + m * (wz * xPz90 + yPz90 - wx * zPz90);
        double zWgs84 = dz + m * (-wy * xPz90 + wx * yPz90 + zPz90);

        // Radians to degrees
        double latitudeWgs84 = Math.Atan2(zWgs84, Math.Sqrt(xWgs84 * xWgs84 + yWgs84 * yWgs84)) * 180.0 / Math.PI;
        double longitudeWgs84 = Math.Atan2(yWgs84, xWgs84) * 180.0 / Math.PI;
        double altitudeWgs84 = Math.Sqrt(xWgs84 * xWgs84 + yWgs84 * yWgs84 + zWgs84 * zWgs84) - WGS84_A;

        return new GeoPoint(latitudeWgs84, longitudeWgs84, altitudeWgs84);
    }
    
    public static GeoPoint ConvertWgs84ToPz90(GeoPoint point)
    {
        // Convert degrees to radians
        double phi = point.Latitude * Math.PI / 180.0;
        double lambda = point.Longitude * Math.PI / 180.0;

        // Curvature radii of meridian and parallel for WGS 84
        double nu = WGS84_A / Math.Sqrt(1 - WGS84_ECCENTRICITY_SQUARED * Math.Sin(phi) * Math.Sin(phi));
        double lambdaSin = Math.Sin(lambda);
        double lambdaCos = Math.Cos(lambda);
        double h = point.Altitude;

        // Geocentric coordinates WGS 84
        double x = (nu + h) * Math.Cos(phi) * lambdaCos;
        double y = (nu + h) * Math.Cos(phi) * lambdaSin;
        double z = ((1 - WGS84_ECCENTRICITY_SQUARED) * nu + h) * Math.Sin(phi);

        // Parameters for reverse transformation of coordinates from WGS 84 to ПЗ-90
        double dx = -1.11;
        double dy = 0.87;
        double dz = -1.08;
        double wx = -0.000000000189;
        double wy = -0.000000000369;
        double wz = -0.000000000890;
        double m = 1 + 0.000001137;

        // Reverse transformation of coordinates from WGS 84 to ПЗ-90
        double xPz90 = dx + m * (x + wz * y - wy * z);
        double yPz90 = dy + m * (-wz * x + y + wx * z);
        double zPz90 = dz + m * (wy * x - wx * y + z);

        // Radians to degrees
        double latitudePz90 = Math.Atan2(zPz90, Math.Sqrt(xPz90 * xPz90 + yPz90 * yPz90)) * 180.0 / Math.PI;
        double longitudePz90 = Math.Atan2(yPz90, xPz90) * 180.0 / Math.PI;
        double altitudePz90 = Math.Sqrt(xPz90 * xPz90 + yPz90 * yPz90 + zPz90 * zPz90) - PZ90_A;

        return new GeoPoint(latitudePz90, longitudePz90, altitudePz90);
    }
}