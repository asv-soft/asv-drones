using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class TemplaterViewModelConfig
{
    public string TemplatePath { get; set; }
    public string ResultPath { get; set; }
}

public class TemplaterViewModel : ViewModelBaseWithValidation
{
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _loc;
    private readonly TemplaterViewModelConfig _config;
    
    private ReadOnlyObservableCollection<TagElement> _tags;
    private ObservableAsPropertyHelper<bool> _isRefreshing;
    private readonly SourceList<TagElement> _tagsList;
    
    public const string UriString = ShellViewModel.UriString + "/tools/templater";
    public static readonly Uri Uri = new(UriString);
    
    public TemplaterViewModel() : base(Uri)
    {
        
        ClearTagsList = ReactiveCommand.Create(ClearTagsListImpl);
        AddStringTag = ReactiveCommand.Create(AddStringTagImpl);
        AddImageTag = ReactiveCommand.Create(AddImageTagImpl);
    }

    [ImportingConstructor]
    public TemplaterViewModel(IConfiguration cfg) : this()
    {
        _cfg = cfg;
        _config = _cfg.Get<TemplaterViewModelConfig>();

        TemplatePath = _config.TemplatePath;
        ResultPath = _config.ResultPath;
        
        _tagsList = new SourceList<TagElement>().DisposeItWith(Disposable);
        _tagsList.Connect()
            .Bind(out _tags)
            .Subscribe()
            .DisposeItWith(Disposable);

        this.WhenPropertyChanged(_ => _.TemplatePath, false)
            .Subscribe(_ =>
            {
                _config.TemplatePath = TemplatePath;
                _cfg.Set(_config);
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.ResultPath, false)
            .Subscribe(_ =>
            {
                _config.ResultPath = ResultPath;
                _cfg.Set(_config);
            })
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public ICommand ClearTagsList { get; set; }
    
    [Reactive]
    public ICommand AddStringTag { get; set; }
    
    [Reactive]
    public ICommand AddImageTag { get; set; }
    
    [Reactive]
    public string TemplatePath { get; set; }
    
    [Reactive]
    public string ResultPath { get; set; }
    
    public ReadOnlyObservableCollection<TagElement> Tags => _tags;
    
    private void AddStringTagImpl()
    {
        var currentGuid = Guid.NewGuid();
        _tagsList.Add(new StringTagElement(currentGuid, ReactiveCommand.Create(() =>
        {
            _tagsList.Remove(_tags.FirstOrDefault(_ => _.Id == currentGuid));
        })){Tag = "", Value = ""});
    }
    
    private void AddImageTagImpl()
    {
        var currentGuid = Guid.NewGuid();
        _tagsList.Add(new ImageTagElement(currentGuid, ReactiveCommand.Create(() =>
        {
            _tagsList.Remove(_tags.FirstOrDefault(_ => _.Id == currentGuid));
        })){Tag = "", Path = ""});
    }
    
    private void ClearTagsListImpl()
    {
        _tagsList.Clear();
    }
    
    private void SaveDocImpl()
    {
        if(!File.Exists(TemplatePath)) return;

        var template = new DocxTemplate(TemplatePath);
        
        foreach (var tag in Tags)
        {
            if (tag is StringTagElement strTag)
            {
                template.Tag(strTag.Tag, strTag.Value);
            }
            else if (tag is ImageTagElement imgTag)
            {
                template.Image(imgTag.Tag, imgTag.Path, 1, 1);
            }
        }
        
        template.Save(ResultPath);
    }
    
    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));

        dialog.PrimaryButtonCommand = ReactiveCommand.Create(SaveDocImpl).DisposeItWith(Disposable);
    }
}

public abstract class TagElement
{
    public Guid Id { get; protected set; }
    public string Tag { get; set; }
    internal ICommand Remove { get; set; }
}

public class StringTagElement : TagElement
{
    public StringTagElement(Guid id, ICommand remove)
    {
        Id = id;
        Remove = remove;
    }
    
    public string Value { get; set; }
}

public class ImageTagElement : TagElement
{
    public ImageTagElement(Guid id, ICommand remove)
    {
        Id = id;
        Remove = remove;
    }
    
    public string Path { get; set; }
}