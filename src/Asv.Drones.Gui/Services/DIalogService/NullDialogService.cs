using System.Threading.Tasks;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class NullDialogService : IDialogService
{
    public bool IsImplementedShowOpenFileDialog { get; } = false;
    public bool IsImplementedShowSaveFileDialog { get; } = false;
    public bool IsImplementedShowSelectFolderDialog { get; } = false;
    public bool IsImplementedShowObserveFolderDialog { get; } = false;

    public Task<string?> ShowOpenFileDialog(string title, string? typeFilter = null, string? initialDirectory = null)
    {
        throw new System.NotImplementedException();
    }

    public Task<string?> ShowSaveFileDialog(string title, string? defaultExt = null, string? typeFilter = null,
        string? initialDirectory = null)
    {
        throw new System.NotImplementedException();
    }

    public Task<string?> ShowSelectFolderDialog(string title, string? oldPath = null)
    {
        throw new System.NotImplementedException();
    }

    public Task ShowObserveFolderDialog(string title, string? defaultPath = null)
    {
        throw new System.NotImplementedException();
    }
}