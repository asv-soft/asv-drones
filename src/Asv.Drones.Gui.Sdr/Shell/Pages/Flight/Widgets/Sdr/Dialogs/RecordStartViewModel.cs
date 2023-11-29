using System.Collections.ObjectModel;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using DocumentFormat.OpenXml.Presentation;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using AsvSdrHelper = Asv.Mavlink.AsvSdrHelper;

namespace Asv.Drones.Gui.Sdr;

public class RecordStartViewModel : ViewModelBaseWithValidation
{
    public const string UriString = FlightSdrViewModel.UriString + ".dialogs.startrecord";
    public static readonly Uri Uri = new(UriString);
    private readonly AsvSdrCustomMode _selectedMode;

    public RecordStartViewModel(ISdrClientDevice payload) : base(Uri)
    {
        if (Design.IsDesignMode)
        {
            var doubleTag = new DoubleTagViewModel(new TagId(Guid.NewGuid(), Guid.NewGuid()), "DoubleTag", 2.812);
            doubleTag.Remove = ReactiveCommand.Create(() => { Tags.Remove(doubleTag); });
            Tags.Add(doubleTag);
        }
        
        _selectedMode = payload.Sdr.CustomMode.Value;
        
        AddTag = ReactiveCommand.Create(() =>
        {
            if (TagName.IsNullOrWhiteSpace() | TagValue.IsNullOrWhiteSpace() | RecordName.IsNullOrWhiteSpace() | 
                Tags.Any(model => model.Name == TagName)) return;
            
            if (SelectedType == "String8")
            {
                var tag = new StringTagViewModel(new TagId(Guid.NewGuid(), Guid.NewGuid()), TagName, TagValue);
                tag.Remove = ReactiveCommand.Create(() => { Tags.Remove(tag); });
                Tags.Add(tag);
            }
            else if (SelectedType == "Int64")
            {
                var tag = new LongTagViewModel(new TagId(Guid.NewGuid(), Guid.NewGuid()), TagName, long.Parse(TagValue));
                tag.Remove = ReactiveCommand.Create(() => { Tags.Remove(tag); });
                Tags.Add(tag);
            }
            else if (SelectedType == "UInt64")
            {
                var tag = new ULongTagViewModel(new TagId(Guid.NewGuid(), Guid.NewGuid()), TagName, uint.Parse(TagValue));
                tag.Remove = ReactiveCommand.Create(() => { Tags.Remove(tag); });
                Tags.Add(tag);
            }
            else if (SelectedType == "Float64")
            {
                var tag = new DoubleTagViewModel(new TagId(Guid.NewGuid(), Guid.NewGuid()), TagName, double.Parse(TagValue));
                tag.Remove = ReactiveCommand.Create(() => { Tags.Remove(tag); });
                Tags.Add(tag);
            }
            
            TagName = "";
            TagValue = "";
        }, this.IsValid());
        
        SelectedType = Types.First();
        SelectedKit = Kits.First();
        SelectedMission = Missions.First();
        
        this.ValidationRule(x => x.TagName, _ =>
            {
                if (_.IsNullOrWhiteSpace()) return false;
                if (Tags.Any(__ => __.Name == _)) return false;
                bool isValid = true;
                try
                {
                    AsvSdrHelper.CheckTagName(_);
                }
                catch
                {
                    isValid = false;
                }
                return isValid;
            }, _ => RS.RecordStartViewModel_TagName_Validation_ErrorMessage)
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.TagValue, _ => CheckTagValue(_), _ => GetTagValueValidationMessage())
            .DisposeItWith(Disposable);
    }

    private bool CheckTagValue(string tagValue)
    {
        if (tagValue.IsNullOrWhiteSpace()) return false;
        
        bool isValid = true;
        
        if (SelectedType == "String8")
        {
            isValid = tagValue.Length <= 8 & tagValue.Length > 0 & tagValue.All(char.IsAscii);
        }
        else if (SelectedType == "Int64")
        {
            isValid = long.TryParse(tagValue, out var result);
        }
        else if (SelectedType == "UInt64")
        {
            isValid = ulong.TryParse(tagValue, out var result);
        }
        else if (SelectedType == "Float64")
        {
            isValid = double.TryParse(tagValue, out var result);
        }
        
        return isValid;
    }

    private string GetTagValueValidationMessage()
    {
        var message = "";
        
        if (SelectedType == "String8")
        {
            message = RS.RecordStartViewModel_TagValue_String8_ErrorMessage;
        }
        else if (SelectedType == "Int64")
        {
            message = RS.RecordStartViewModel_TagValue_Int64_ErrorMessage;
        }
        else if (SelectedType == "UInt64")
        {
            message = RS.RecordStartViewModel_TagValue_UInt64_ErrorMessage;
        }
        else if (SelectedType == "Float64")
        {
            message = RS.RecordStartViewModel_TagValue_Float64_ErrorMessage;
        }

        return message;
    }
    
    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
        
        this.ValidationRule(x => x.RecordName, _ =>
            {
                dialog.IsPrimaryButtonEnabled = false;
                
                if (_.IsNullOrWhiteSpace()) return dialog.IsPrimaryButtonEnabled;
                
                try
                {
                    AsvSdrHelper.CheckRecordName(_);
                    dialog.IsPrimaryButtonEnabled = true;
                }
                catch
                {
                }
                
                return dialog.IsPrimaryButtonEnabled;
            }, _ => RS.RecordStartViewModel_RecordName_Validation_ErrorMessage)
            .DisposeItWith(Disposable);
    }
    
    public ICommand AddTag { get; }

    [Reactive]
    public IEnumerable<int> Kits { get; set; } = Enumerable.Range(1, 16);

    [Reactive] 
    public int SelectedKit { get; set; }
    
    public string[] Missions => _selectedMode switch
    {
        AsvSdrCustomMode.AsvSdrCustomModeGp => new [] {"Zero", "Cross", "Upper", "Lower"},
        AsvSdrCustomMode.AsvSdrCustomModeLlz => new [] {"Zero", "Cross", "Left", "Right"},
        AsvSdrCustomMode.AsvSdrCustomModeVor => new [] {"Bearing", "Radial"},
        AsvSdrCustomMode.AsvSdrCustomModeIdle => Array.Empty<string>(),
        _ => Array.Empty<string>()
    };
    
    [Reactive]
    public string SelectedMission { get; set; }
    
    public string[] Types => new[]
    {
        "String8",
        "Int64",
        "UInt64",
        "Float64"
    };
    
    [Reactive]
    public string SelectedType { get; set; }
    
    [Reactive]
    public string RecordName { get; set; }
    
    [Reactive]
    public string TagName { get; set; }
    
    [Reactive]
    public string TagValue { get; set; }

    [Reactive] 
    public ObservableCollection<TagViewModel> Tags { get; set; } = new();
}