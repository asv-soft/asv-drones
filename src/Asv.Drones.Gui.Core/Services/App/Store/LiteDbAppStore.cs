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

        
    }
}