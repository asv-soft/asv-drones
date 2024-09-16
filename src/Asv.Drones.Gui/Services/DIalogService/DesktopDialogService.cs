using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Asv.Drones.Gui;

public sealed class DesktopDialogService : IDialogService
{
    private readonly TopLevel _topLevel;
    
    public DesktopDialogService(TopLevel topLevel)
    {
        _topLevel = topLevel;
        IsImplementedShowOpenFileDialog = true;
        IsImplementedShowSaveFileDialog = true;
        IsImplementedShowSelectFolderDialog = true;
        IsImplementedShowObserveFolderDialog = true;
    }

    public bool IsImplementedShowOpenFileDialog { get; }
    public bool IsImplementedShowSaveFileDialog { get; }
    public bool IsImplementedShowSelectFolderDialog { get; }
    public bool IsImplementedShowObserveFolderDialog { get; }
    
    public async Task<string?> ShowOpenFileDialog(string title, string? typeFilter = null, string? initialDirectory = null)
    {
        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };
        
        if (!string.IsNullOrEmpty(typeFilter))
        {
            var fileTypes = typeFilter
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext =>
                {
                    if (ext == "*")
                    {
                        return new FilePickerFileType(ext.Trim())
                        {
                            Patterns = ["*"], // linux, windows, web
                            AppleUniformTypeIdentifiers = null, // for apple
                            MimeTypes = null, // for web only
                        };
                    }

                    return new FilePickerFileType(ext.Trim())
                    {
                        Patterns = [$"*.{ext.Trim()}"], // linux, windows, web
                        AppleUniformTypeIdentifiers = null, // for apple
                        MimeTypes = null, // for web only
                    };
                })
                .ToList();

            options.FileTypeFilter = fileTypes;
        }

        if (!string.IsNullOrEmpty(initialDirectory))
        {
            options.SuggestedStartLocation = await _topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
        }

        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(options);

        return files.Count == 1 ? 
            files[0].Path.AbsolutePath 
            : null;
    }

    public async Task<string?> ShowSaveFileDialog(string title, string? defaultExt = null, string? typeFilter = null,
        string? initialDirectory = null)
    {
        var options = new FilePickerSaveOptions
        {
            Title = title,
        };

        if (!string.IsNullOrEmpty(defaultExt))
        {
            options.DefaultExtension = defaultExt;
        }
        
        if (!string.IsNullOrEmpty(typeFilter))
        {
            var fileTypes = typeFilter
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => new FilePickerFileType(ext.Trim())
                {
                    Patterns = [$"*.{ext.Trim()}"], // linux, windows, web
                    AppleUniformTypeIdentifiers = null, // for apple
                    MimeTypes = null, // for web only
                })
                .ToList();

            options.FileTypeChoices = fileTypes;
        }

        if (!string.IsNullOrEmpty(initialDirectory))
        {
            options.SuggestedStartLocation = await _topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
        }

        var result = await _topLevel.StorageProvider.SaveFilePickerAsync(options);
        return result?.Path.AbsolutePath;
    }

    public async Task<string?> ShowSelectFolderDialog(string title, string? oldPath = null)
    {
        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };

        if (!string.IsNullOrEmpty(oldPath))
        {
            options.SuggestedStartLocation = await _topLevel.StorageProvider.TryGetFolderFromPathAsync(oldPath);
        }
        
        var folders = await _topLevel.StorageProvider.OpenFolderPickerAsync(options);
        
        var folder = folders.FirstOrDefault()?.Path.AbsolutePath;

        return folder;
    }

    public Task ShowObserveFolderDialog(string title, string? defaultPath = null)
    {
        if (defaultPath is null)
        {
            return Task.CompletedTask;
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            OpenFolderInWindowsExplorer(defaultPath);
            return Task.CompletedTask;
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            OpenFolderInMacFinder(defaultPath);
            return Task.CompletedTask;
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            OpenFolderInLinuxFileManager(defaultPath);
            return Task.CompletedTask;
        }
        
        return Task.CompletedTask;
    }
    
    private static void OpenFolderInWindowsExplorer(string folderPath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{folderPath}\"",
            UseShellExecute = true
        });
    }

    private static void OpenFolderInMacFinder(string folderPath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "open",
            Arguments = $"\"{folderPath}\"",
            UseShellExecute = true
        });
    }

    private static void OpenFolderInLinuxFileManager(string folderPath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "xdg-open",
            Arguments = $"\"{folderPath}\"",
            UseShellExecute = true
        });
    }
}