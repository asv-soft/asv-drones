using System.Diagnostics;
using Avalonia.Media.Imaging;

namespace Asv.Avalonia.Map
{
    

    public class FolderDbCache : PureImageCache
    {
        private readonly string _rootFolder;
        public FolderDbCache(string rootFolder)
        {
            _rootFolder = rootFolder;
            if (Directory.Exists(_rootFolder) == false) Directory.CreateDirectory(_rootFolder);
        }

        public bool PutImageToCache(byte[] tile, int type, GPoint pos, int zoom)
        {
            var fileName = GetFileName(type, zoom, pos.X,pos.Y, out var dir);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
                // if directory not exist, file not exist too
            }
            else
            {
                if (File.Exists(fileName)) File.Delete(fileName);
            }
            
            
            File.WriteAllBytes(fileName,tile);
            return true;
        }

        private string GetFileName(int type, int zoom, long posX, long posY, out string dir)
        {
            dir = Path.Combine(_rootFolder, $"T_{type}", $"Z_{zoom:000}");
            return Path.Combine(dir, $"X_{posX}_Y_{posY}.jpg");
        }

        public PureImage GetImageFromCache(int type, GPoint pos, int zoom)
        {
            var fileName = GetFileName(type, zoom, pos.X, pos.Y, out var dir);
            if (File.Exists(fileName) == false) return null;
            using var file = File.OpenRead(fileName);
            var m = new MemoryStream();
            file.CopyTo(m);
            m.Seek(0, SeekOrigin.Begin);
            try
            {
                var b = new Bitmap(m);
                var ret = new MapImage { Img = b,Data = m};
                return ret;
            }
            catch
            {
                Debug.WriteLine("WindowsPresentationImageProxy: unknown image format: " + type);
                return null;
            }
        }

        public int DeleteOlderThan(DateTime date, int? type)
        {
            //TODO: delete files by creation time
            return 0;
        }
    }

}
