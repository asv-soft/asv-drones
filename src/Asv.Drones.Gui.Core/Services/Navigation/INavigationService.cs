using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Asv.Drones.Gui.Core
{
    public class NavigationServiceHelper
    {

    }

    public interface INavigationService
    {
        void InitStorageProvider(IStorageProvider windowStorageProvider);
        void Init(IShell shell);
        /// <summary>
        /// Navigate to IShell page by URI
        /// </summary>
        /// <param name="uri"></param>    
        void GoTo(Uri uri);

        /// <summary>
        /// Shows an open file dialog
        /// </summary>
        /// <param name="title">The title for the dialog</param>
        /// <param name="suggestedStartLocation">Sets the initial location where the folder open picker looks for files to present to the user.</param>
        /// <param name="fileTypes">Sets the collection of file types that the file open picker displays</param>
        /// <returns>The path of the file that was opened. Null if no file was opened</returns>
        Task<string?> ShowOpenFileDialogAsync(string title, string? suggestedStartLocation, params FilePickerFileType[] fileTypes);

        /// <summary>
        /// Shows an open folder dialog
        /// </summary>
        /// <param name="title">The title for the dialog</param>
        /// <param name="suggestedStartLocation">Sets the initial location where the folder open picker looks for files to present to the user.</param>
        /// <returns>The path of the folder that was opened. Null if no folder was opened</returns>
        Task<string?> ShowOpenFolderDialogAsync(string title, string? suggestedStartLocation);

        /// <summary>
        /// Shows a save file dialog
        /// </summary>
        /// <param name="title">Sets the text that appears in the title bar of a folder dialog.</param>
        /// <param name="suggestedStartLocation">Sets the initial location where the file open picker looks for files to present to the user</param>
        /// <param name="suggestedFileName">Sets the file name that the file save picker suggests to the user</param>
        /// <param name="defaultExtension">Sets the default extension to be used to save the file</param>
        /// <param name="fileTypes"></param>
        /// <returns>The path of the file that was saved. Null if no file was saved</returns>
        Task<string?> ShowSaveFileDialogAsync(string title, string? suggestedStartLocation, string? suggestedFileName, string? defaultExtension, params FilePickerFileType[] fileTypes);

    }
}
