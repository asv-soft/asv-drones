using System;
using Asv.Store;

namespace Asv.Drones.Core
{
    /// <summary>
    /// This interface is used for copy data from one store to another
    /// </summary>
    public interface IAppStoreCopyProvider
    {
        void TryCopy(IAppStore from,IAppStore to);
    }
    
    public interface IAppStore:IStore
    {
        int GetFileSizeInBytes();
    }

    
}