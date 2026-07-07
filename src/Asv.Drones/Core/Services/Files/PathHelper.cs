namespace Asv.Drones;

public static class PathHelper
{
    public static string EncodePathToId(string path)
    {
        var utf8 = System.Text.Encoding.UTF8.GetBytes(path);
        var id = Convert.ToHexString(utf8);
        return id;
    }
}
