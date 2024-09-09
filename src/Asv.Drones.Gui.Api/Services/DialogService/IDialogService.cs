namespace Asv.Drones.Gui.Api;

public interface IDialogService
{
    public bool IsImplementedShowOpenFileDialog { get; }
    public bool IsImplementedShowSaveFileDialog { get; }
    public bool IsImplementedShowSelectFolderDialog { get; }
    public bool IsImplementedShowObserveFolderDialog { get; }
    
    /// <summary>
    /// Opens dialog to choose a file
    /// </summary>
    /// <param name="title">caption of the dialog</param>
    /// <param name="typeFilter">extension filter, example: "txt, *, nupkg"</param>
    /// <param name="initialDirectory">directory where to start search</param>
    /// <returns></returns>
    public Task<string?> ShowOpenFileDialog(string title, string? typeFilter = null, string? initialDirectory = null);

    /// <summary>
    /// Opens dialog to save a file
    /// </summary>
    /// <param name="title">caption of the dialog</param>
    /// <param name="defaultExt">default extension of the file</param>
    /// <param name="typeFilter">extension filter, example: "txt, *, nupkg"</param>
    /// <param name="initialDirectory">directory where to start search</param>
    /// <returns></returns>
    public Task<string?> ShowSaveFileDialog(string title, string? defaultExt = null, string? typeFilter = null, string? initialDirectory = null);

    /// <summary>
    /// Opens dialog to select a folder
    /// </summary>
    /// <param name="title">caption of the dialog</param>
    /// <param name="oldPath">default path</param>
    /// <returns></returns>
    public Task<string?> ShowSelectFolderDialog(string title, string? oldPath = null);

    /// <summary>
    /// Opens dialog to observe a folder
    /// </summary>
    /// <param name="title">caption of the dialog</param>
    /// <param name="defaultPath">default path</param>
    /// <returns></returns>
    public Task ShowObserveFolderDialog(string title, string? defaultPath);

}