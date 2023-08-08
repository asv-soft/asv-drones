using System.Collections.ObjectModel;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Sdr;

public class RecordStartViewModel : ViewModelBaseWithValidation
{
    public const string UriString = FlightSdrViewModel.UriString + ".dialogs.startrecord";
    public static readonly Uri Uri = new(UriString);
    
    public RecordStartViewModel() : base(Uri)
    {
        if (Design.IsDesignMode)
        {
            var doubleTag = new DoubleTagViewModel()
            {
                Name = "DoubleTag",
                Value = 2.812
            };
            doubleTag.Remove = ReactiveCommand.Create(() => { Tags.Remove(doubleTag); });
            Tags.Add(doubleTag);
            
            var longTag = new LongTagViewModel()
            {
                Name = "LongTag",
                Value = 13
            };
            longTag.Remove = ReactiveCommand.Create(() => { Tags.Remove(longTag); });
            Tags.Add(longTag);

            var ulongTag = new ULongTagViewModel()
            {
                Name = "ULongTag",
                Value = 13
            };
            ulongTag.Remove = ReactiveCommand.Create(() => { Tags.Remove(ulongTag); });
            Tags.Add(ulongTag);

            var stringTag = new StringTagViewModel()
            {
                Name = "StringTag",
                Value = "TagInfo"
            };
            stringTag.Remove = ReactiveCommand.Create(() => { Tags.Remove(stringTag); });
            Tags.Add(stringTag);

        }

        AddTag = ReactiveCommand.Create(() =>
        {
            if (TagName.IsNullOrWhiteSpace() | TagValue.IsNullOrWhiteSpace() | RecordName.IsNullOrWhiteSpace() | 
                Tags.Where(__ => __.Name == TagName).Count() > 0) return;
            
            if (SelectedType == "String8")
            {
                var tag = new StringTagViewModel()
                {
                    Name = TagName,
                    Value = TagValue
                };
                tag.Remove = ReactiveCommand.Create(() => { Tags.Remove(tag); });
                Tags.Add(tag);
            }
            else if (SelectedType == "Int64")
            {
                var tag = new LongTagViewModel()
                {
                    Name = TagName,
                    Value = long.Parse(TagValue) 
                };
                tag.Remove = ReactiveCommand.Create(() => { Tags.Remove(tag); });
                Tags.Add(tag);
            }
            else if (SelectedType == "UInt64")
            {
                var tag = new ULongTagViewModel()
                {
                    Name = TagName,
                    Value = ulong.Parse(TagValue) 
                };
                tag.Remove = ReactiveCommand.Create(() => { Tags.Remove(tag); });
                Tags.Add(tag);
            }
            else if (SelectedType == "Float64")
            {
                var tag = new DoubleTagViewModel()
                {
                    Name = TagName,
                    Value = double.Parse(TagValue) 
                };
                tag.Remove = ReactiveCommand.Create(() => { Tags.Remove(tag); });
                Tags.Add(tag);
            }
            
            TagName = "";
            TagValue = "";
        });
        
        SelectedType = Types.First();
        
        this.ValidationRule(x => x.TagName, _ =>
            {
                if (_.IsNullOrWhiteSpace()) return false;
                if (Tags.Where(__ => __.Name == _).Count() > 0) return false;
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

    [Reactive] public ObservableCollection<TagViewModel> Tags { get; set; } = new ObservableCollection<TagViewModel>();
}