using System;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Composition;
using System.IO;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MavlinkHelper = Asv.Mavlink.MavlinkHelper;

namespace Asv.Drones.Gui;

[ExportShellPage(WellKnownUri.ShellPageVehicleFileBrowser)]
public class VehicleFileBrowserViewModel : ShellPage
{
    private readonly ILogService _log;
    private readonly IMavlinkDevicesService _svc;
    
    public VehicleFileBrowserViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        Store =
        [
            new FileSystemItemViewModel
            {
                Name = "Folder1",
                Path = @"C:\path\to\file",
                IsDirectory = true,
                Children =
                [
                    new FileSystemItemViewModel
                    {
                        Name = "Folder1_File1",
                        Path = @"C:\path\to\another\file",
                        IsFile = true,
                        Size = "123,4 KB"
                    }
                ]
            },
            new FileSystemItemViewModel
            {
                Name = "File1",
                Path = @"C:\path\to\file1",
                IsFile = true,
                Size = "12 GB"
            },
            new FileSystemItemViewModel
            {
                Name = "File2",
                Path = @"C:\path\to\file2",       
                IsFile = true,
                Size = "123456 TB"
            }
        ];
        
        Device = 
        [
            new FileSystemItemViewModel
            {
                Name = "RemoteFolder1",
                Path = @"C:\path\to\Remote_file",
                IsDirectory = true,
                Children =
                [
                    new FileSystemItemViewModel
                    {
                        Name = "RemoteFolder1_RemoteFile1",
                        Path = @"C:\path\to\another\Remote_file",
                        IsFile = true,
                        Size = "1234 KB"
                    }
                ]
            },
            new FileSystemItemViewModel
            {
                Name = "RemoteFile1",
                Path = @"C:\path\to\Remote_file1",
                IsFile = true,
                Size = "1,3 GB"
            },
            new FileSystemItemViewModel
            {
                Name = "RemoteFile2",
                Path = @"C:\path\to\Remote_file2",       
                IsFile = true,
                Size = "654321 TB"
            },
            new FileSystemItemViewModel
            {
                Name = "RemoteFile3",
                Path = @"C:\path\to\Remote_file3",       
                IsFile = true,
                Size = "2 B"
            }
        ];
    }
    
    [ImportingConstructor]
    public VehicleFileBrowserViewModel(IMavlinkDevicesService svc, IApplicationHost appService, ILogService log) 
        : base(WellKnownUri.ShellPageVehicleFileBrowser)
    {
        _log = log;
        _svc = svc;

        Store = LoadLocalFiles(appService.Paths.AppDataFolder);
        Device = LoadLocalFiles(appService.Paths.AppDataFolder);
    }

    public override async void SetArgs(NameValueCollection args)
    {
        base.SetArgs(args);
        
        if (ushort.TryParse(args["id"], out var id) == false) return;
        if (Enum.TryParse<DeviceClass>(args["class"], true, out var deviceClass) == false) return;
        
        Icon = Api.MavlinkHelper.GetIcon(deviceClass);
        
        var vehicle = _svc.GetVehicleByFullId(id);
        if (vehicle == null) return;

        
        Title = $"{vehicle.Class}: {vehicle.Name.Value}";
        
        var ftpClient = vehicle.Ftp;
        //Device = await LoadRemoteFiles(ftpClient, "/");
        
    }

    public static Uri GenerateUri(string baseUri, ushort deviceFullId, DeviceClass @class) =>
        new($"{baseUri}?id={deviceFullId}&class={@class:G}");

    private static ObservableCollection<FileSystemItemViewModel> LoadLocalFiles(string path)
    {
        var items = new ObservableCollection<FileSystemItemViewModel>();
    
        foreach (var directory in Directory.GetDirectories(path))
        {
            items.Add(new FileSystemItemViewModel
            {
                Name = Path.GetFileName(directory),
                Path = directory,
                IsDirectory = true,
                Children = LoadLocalFiles(directory)
            });
        }

        foreach (var file in Directory.GetFiles(path))
        {
            items.Add(new FileSystemItemViewModel
            {
                Name = Path.GetFileName(file),
                Path = file,
                IsFile = true,
                Size = ConvertBytesToReadableSize(new FileInfo(file).Length)
            });
        }

        return items;
    }

    /*private static async Task<ObservableCollection<FileSystemItemViewModel>> LoadRemoteFiles(IFtpClient ftpClient, string path)
    {
        var items = new ObservableCollection<FileSystemItemViewModel>();
        var offset = 0u;
    
        // Чтение данных о директории до тех пор, пока не придет пакет с концом данных (EOF)
        while (true)
        {
            // Получаем следующий пакет данных
            var result = await ftpClient.ListDirectory(path, offset);
        
            // Получаем данные из пакета
            var data = result.ReadDataAsString();
            
            // Используем SequenceReader для чтения записей о файлах/папках
            var reader = new SequenceReader<char>(new ReadOnlySequence<char>(data.AsMemory()));
        
            // Разбираем каждую запись в директории
            while (MavlinkFtpHelper.ParseFtpEntry(ref reader, path, out var entry))
            {
                switch (entry)
                {
                    case FtpDirectory directory:
                        items.Add(new FileSystemItemViewModel
                        {
                            Name = directory.Name,
                            Path = directory.Path,
                            IsDirectory = true
                        });
                        break;
                    case FtpFile file:
                        items.Add(new FileSystemItemViewModel
                        {
                            Name = file.Name,
                            Path = file.Path,
                            IsDirectory = false
                        });
                        break;
                }
            }
        
            // Проверяем, достигнут ли конец файла (EOF)
            if (result.ReadOpcode() == FtpOpcode.Nak && result.ReadBurstComplete())
            {
                break;
            }
        }

        return items;
    }*/
    
    private static string ConvertBytesToReadableSize(long fileSizeInBytes)
    {
        string[] sizes = [
            RS.Unit_Byte_Abbreviation, 
            RS.Unit_Kilobyte_Abbreviation, 
            RS.Unit_Megabyte_Abbreviation,
            RS.Unit_Gigabyte_Abbreviation,
            RS.Unit_Terabyte_Abbreviation
        ];
        double len = fileSizeInBytes;
        var order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
    
    public ReactiveCommand<Unit, Unit> UploadCommand { get; set; }
    public ReactiveCommand<Unit, Unit> DownloadCommand { get; set; }
    
    [Reactive] public FileSystemItemViewModel? LeftSelectedItem { get; set; }
    [Reactive] public FileSystemItemViewModel? RightSelectedItem { get; set; }
    public ObservableCollection<FileSystemItemViewModel> Device { get; set; }
    public ObservableCollection<FileSystemItemViewModel> Store { get; set; }
}

public class FileSystemItemViewModel : DisposableReactiveObject
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public bool IsDirectory { get; set; }
    public bool IsFile { get; set; }
    public ObservableCollection<FileSystemItemViewModel> Children { get; set; } = [];
    public string? Size { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool IsSelected { get; set; }
}