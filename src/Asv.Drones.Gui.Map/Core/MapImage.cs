using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Asv.Avalonia.Map
{

    public enum ScaleModes
    {
        /// <summary>
        ///     no scaling
        /// </summary>
        Integer,

        /// <summary>
        ///     scales to fractional level using a stretched tiles, any issues -> http://greatmaps.codeplex.com/workitem/16046
        /// </summary>
        ScaleUp,

        /// <summary>
        ///     scales to fractional level using a narrowed tiles, any issues -> http://greatmaps.codeplex.com/workitem/16046
        /// </summary>
        ScaleDown,

        /// <summary>
        ///     scales to fractional level using a combination both stretched and narrowed tiles, any issues ->
        ///     http://greatmaps.codeplex.com/workitem/16046
        /// </summary>
        Dynamic
    }
    /// <summary>
    ///     image abstraction
    /// </summary>
    public class MapImage : PureImage
    {
        
        public IImage Img;

        public override void Dispose()
        {
            if (Img != null)
            {
                Img = null;
            }

            if (Data != null)
            {
                Data.Dispose();
                Data = null;
            }
        }
    }

    /// <summary>
    ///     image abstraction proxy
    /// </summary>
    public class MapImageProxy : PureImageProxy
    {
        
        private MapImageProxy()
        {
        }

        public static void Enable()
        {
            GMapProvider.TileImageProxy = Instance;
        }

        public static readonly MapImageProxy Instance = new MapImageProxy();

        public override PureImage FromStream(Stream stream)
        {
            if (stream != null)
            {
                try
                {
                    var m = new Bitmap(stream);
                   
                    var ret = new MapImage { Img = m};
                    return ret;
                }
                catch
                {
                    stream.Position = 0;
                    
                    int type = stream.Length > 0 ? stream.ReadByte() : 0;
                    Debug.WriteLine("WindowsPresentationImageProxy: unknown image format: " + type);
                }
            }

            return null;
        }

        public override bool Save(Stream stream, PureImage image)
        {
            var ret = (MapImage)image;
            if (ret.Img != null)
            {
                try
                {
                    (ret.Img as Bitmap)?.Save(stream);
                    return true;
                }
                catch
                {
                    // ignore
                }
            }

            return false;
        }
    }

}
