using Asv.Common;
using LiteDB;

namespace Asv.Drones.Core
{
    
    public class LiteDbAppStore : DisposableOnceWithCancel, IAppStore
    {
        private readonly LiteDatabase _db;

        public LiteDbAppStore(LiteDatabase db, string sourceName)
        {
            SourceName = sourceName;
            _db = db;
        }
        
        public int GetFileSizeInBytes()
        {
            var result = _db.GetCollection("$dump")
                .Query()
                .Select("{used: SUM(*.usedBytes)}")
                .ToList();

            return result[0]["used"].AsInt32;
        }

        public ILiteDatabase Db => _db;
        public string SourceName { get; }
    }
}