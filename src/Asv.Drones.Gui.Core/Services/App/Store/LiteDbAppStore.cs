using System;
using Asv.Store;
using LiteDB;

namespace Asv.Drones.Core
{
    
    public class LiteDbAppStore : LiteDbFileStore, IAppStore
    {
        private readonly LiteDatabase _db;

        public LiteDbAppStore(LiteDatabase db, string sourceName) : base(db, sourceName)
        {
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
    }
}