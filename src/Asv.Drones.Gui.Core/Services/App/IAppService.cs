using Asv.Common;
using Asv.Drones.Core;
using Avalonia.Platform.Storage;

namespace Asv.Drones.Gui.Core
{


    public interface IAppService
    {
        IAppInfo Info { get; }
        IAppPathInfo Paths { get; }
        
        
        #region Store
        /// <summary>
        /// Application store
        /// </summary>
        IRxValue<IAppStore> Store { get; }

        /// <summary>
        /// File filter for store file
        /// </summary>
        FilePickerFileType StoreFileFilter { get; }
        string GetSuggestedFileNameForStore();
        string DefaultStoreFileExtension { get; }
        
       
        /// <summary>
        /// Create new store
        /// </summary>
        /// <param name="filePath"></param>
        void CreateStore(string filePath, bool copyFromCurrent);
        /// <summary>
        /// Open store
        /// </summary>
        /// <param name="filePath">file path</param>
        void OpenStore(string filePath);
        
        #endregion
        
    }

    
}