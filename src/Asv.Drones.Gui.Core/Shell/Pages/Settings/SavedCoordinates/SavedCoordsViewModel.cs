using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class FixedModeConfig
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public double Altitude { get; set; }
    public double Accuracy { get; set; }
    public string Name { get; set; }
}

public class FixedModeSavedCoords
{
    public ObservableCollection<FixedModeConfig> Coords { get; set; } = new();
}

[Export(typeof(ISettingsPart))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SavedCoordsViewModel : SettingsPartBase
{
    private static readonly Uri Uri = new(SettingsPartBase.Uri, "savedcoords");
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _cfg;
    
    public SavedCoordsViewModel() : base(Uri)
    {
    }

    [ImportingConstructor]
    public SavedCoordsViewModel(IConfiguration configuration, ILocalizationService loc) : this()
    {
        _loc = loc;
        _cfg = configuration;
        
        UpdateValues(configuration);

        AddNewItemCommand = ReactiveCommand.CreateFromTask(AddNewItem).DisposeItWith(Disposable);

        var canExecuteRemoveCommand = this.WhenAnyValue(
            _ => _.SelectedCoordsItem, (selected) => SavedCoordinates.Contains(selected));
        
        RemoveItemCommand = ReactiveCommand.CreateFromTask(RemoveItem, canExecuteRemoveCommand).DisposeItWith(Disposable);
    }

    private void UpdateValues(IConfiguration configuration)
    {
        var savedCoordsConfig = configuration.Get<FixedModeSavedCoords>();
        
        if (savedCoordsConfig.Coords != null)
        {
            SavedCoordinates = savedCoordsConfig.Coords;
        }
    }

    private async Task AddNewItem()
    {
        var dialog = new ContentDialog()
        {
            Title = RS.SavedCoordsViewModel_AddNewItem_Title,
            PrimaryButtonText = RS.SavedCoordsViewModel_AddNewItem_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.SavedCoordsViewModel_AddNewItem_CloseButtonText
        };

        var itemToAdd = SelectedCoordsItem != null ? SelectedCoordsItem : new FixedModeConfig();
        var vm = new AddNewMapPointViewModel(itemToAdd, _loc, _cfg);
        vm.ApplyDialog(dialog);
        dialog.Content = vm;
        var result = await dialog.ShowAsync();
        
        UpdateValues(_cfg);
    }

    private async Task RemoveItem()
    {
        var dialog = new ContentDialog()
        {
            Title = RS.SavedCoordsViewModel_RemoveItem_Title,
            PrimaryButtonText = RS.SavedCoordsViewModel_RemoveItem_PrimaryButtonText,
            IsSecondaryButtonEnabled = true,
            CloseButtonText = RS.SavedCoordsViewModel_RemoveItem_CloseButtonText
        };

        var vm = new RemoveMapPointViewModel(SelectedCoordsItem, SavedCoordinates.IndexOf(SelectedCoordsItem), _loc, _cfg);
        vm.ApplyDialog(dialog);
        dialog.Content = vm;
        var result = await dialog.ShowAsync();
        UpdateValues(_cfg);
    }

    public override int Order => 3;
    
    public ICommand AddNewItemCommand { get; set; }
    public ICommand? RemoveItemCommand { get; set; }
    
    [Reactive]
    public ObservableCollection<FixedModeConfig> SavedCoordinates { get; set; } = new();
    [Reactive] 
    public FixedModeConfig SelectedCoordsItem { get; set; } = new();
    public string MapIcon => MaterialIconDataProvider.GetData(MaterialIconKind.LocationOn);
}