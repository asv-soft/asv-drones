using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Asv.Avalonia.Map
{
    

    /// <summary>
    ///     cache system for tiles, geocoding, etc...
    /// </summary>
    public class Cache
    {
        private static Cache? _cacheInstance;

        /// <summary>
        ///     abstract image cache
        /// </summary>
        public PureImageCache ImageCache;

        /// <summary>
        ///     second level abstract image cache
        /// </summary>
        public PureImageCache ImageCacheSecond;

        string _cache;

        public static string CacheFolder { get; set; } = "map";

        public static Cache Instance => _cacheInstance ??= new Cache();

        private Cache()
        {
            ImageCache = new FolderDbCache(CacheFolder);
        }

        #region -- etc cache --

        static readonly SHA1 HashProvider = SHA1.Create();
        

        void ConvertToHash(ref string s)
        {
            s = BitConverter.ToString(HashProvider.ComputeHash(Encoding.Unicode.GetBytes(s)));
        }

        public void SaveContent(string url, CacheType type, string content)
        {
            try
            {
                ConvertToHash(ref url);

                string dir = Path.Combine(_cache, type.ToString()) + Path.DirectorySeparatorChar;

                // precrete dir
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string file = dir + url + ".txt";

                using (var writer = new StreamWriter(file, false, Encoding.UTF8))
                {
                    writer.Write(content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveContent: " + ex);
            }
        }

        public string GetContent(string url, CacheType type, TimeSpan stayInCache)
        {
            string ret = null;

            try
            {
                ConvertToHash(ref url);

                string dir = Path.Combine(_cache, type.ToString()) + Path.DirectorySeparatorChar;
                string file = dir + url + ".txt";

                if (File.Exists(file))
                {
                    var writeTime = File.GetLastWriteTime(file);
                    if (DateTime.Now - writeTime < stayInCache)
                    {
                        using (var r = new StreamReader(file, Encoding.UTF8))
                        {
                            ret = r.ReadToEnd();
                        }
                    }
                    else
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                ret = null;
                Debug.WriteLine("GetContent: " + ex);
            }

            return ret;
        }

        public string GetContent(string url, CacheType type)
        {
            return GetContent(url, type, TimeSpan.FromDays(100));
        }

        #endregion
    }

    public enum CacheType
    {
        GeocoderCache,
        PlacemarkCache,
        RouteCache,
        UrlCache,
        DirectionsCache,
    }
}
