using LiteDB;

namespace Asv.Drones.Core
{
    public interface IAppStore:IDisposable
    {
        int GetFileSizeInBytes();
        ILiteDatabase Db { get; }
        string SourceName { get; }
    }

    
}