using System.Collections.ObjectModel;
using System.Reactive;
using Asv.Common;
using Asv.Mavlink;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public enum HierarchicalStoreEntryAction
{
    Rename,
    Delete
}

public class HierarchicalStoreEntryViewModel : DisposableReactiveObject
{
    public HierarchicalStoreEntryViewModel()
    {
        BeginEditName = ReactiveCommand.Create(() => { IsInEditNameMode = true; }).DisposeItWith(Disposable);
        ;
        EndEditName = ReactiveCommand.Create(() =>
        {
            IsInEditNameMode = false;
            Rename(Name);
        }).DisposeItWith(Disposable);
        EndEditName.ThrownExceptions
            .Subscribe(ex => OnError(HierarchicalStoreEntryAction.Rename, ex))
            .DisposeItWith(Disposable);
        DeleteEntry = ReactiveCommand.Create(Delete)
            .DisposeItWith(Disposable);
        DeleteEntry.ThrownExceptions
            .Subscribe(ex => OnError(HierarchicalStoreEntryAction.Delete, ex))
            .DisposeItWith(Disposable);

        BeginMove = ReactiveCommand.Create(() => { IsInMoveMode = true; }).DisposeItWith(Disposable);
        CancelMove = ReactiveCommand.Create(() => { IsInMoveMode = false; }).DisposeItWith(Disposable);
        EndMove = ReactiveCommand.Create(() =>
        {
            IsInMoveMode = false;
            Move();
        }).DisposeItWith(Disposable);

        this.WhenValueChanged(x => x.IsInMoveMode)
            .Subscribe(x => IsNotInMoveMode = !x)
            .DisposeItWith(Disposable);
        this.WhenValueChanged(x => x.IsNotInMoveMode)
            .Subscribe(x => IsInMoveMode = !x)
            .DisposeItWith(Disposable);
    }


    [Reactive] public bool IsInMoveMode { get; set; }
    [Reactive] public bool IsNotInMoveMode { get; set; }
    public object Id { get; set; }
    public object ParentId { get; set; }
    public bool IsFolder { get; set; }
    public bool IsFile { get; set; }
    public FolderStoreEntryType Type { get; set; }
    [Reactive] public string Name { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool IsSelected { get; set; }

    public virtual ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items { get; set; }
    [Reactive] public bool IsInEditNameMode { get; set; } = false;

    public ReactiveCommand<Unit, Unit> DeleteEntry { get; }
    public ReactiveCommand<Unit, Unit> BeginEditName { get; }
    public ReactiveCommand<Unit, Unit> EndEditName { get; }
    public ReactiveCommand<Unit, Unit> BeginMove { get; }
    public ReactiveCommand<Unit, Unit> EndMove { get; }
    public ReactiveCommand<Unit, Unit> CancelMove { get; }

    public virtual ReadOnlyObservableCollection<HierarchicalStoreEntryTagViewModel> Tags { get; set; }

    [Reactive] public string Description { get; set; }


    protected virtual void OnError(HierarchicalStoreEntryAction action, Exception? exception)
    {
    }

    protected virtual void Rename(string? name)
    {
    }

    protected virtual void Delete()
    {
    }

    protected virtual void Move()
    {
    }

    public virtual void Refresh()
    {
    }

    public HierarchicalStoreEntryViewModel? FindAndSelect(object id)
    {
        if (id.Equals(Id))
        {
            IsExpanded = true;
            IsSelected = true;
            return this;
        }
        else
        {
            foreach (var item in Items)
            {
                var find = item.FindAndSelect(id);
                if (find == null) continue;
                IsExpanded = true;
                return find;
            }
        }

        return null;
    }
}

public class HierarchicalStoreEntryViewModel<TKey, TFile> : HierarchicalStoreEntryViewModel
    where TKey : notnull
    where TFile : IDisposable
{
    private readonly Node<IHierarchicalStoreEntry<TKey>, TKey> _node;
    private readonly HierarchicalStoreViewModel<TKey, TFile> _context;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> _items;
    private ReadOnlyObservableCollection<HierarchicalStoreEntryTagViewModel> _tags;
    private readonly SourceCache<HierarchicalStoreEntryTagViewModel, string> _tagsSource;
    private bool _isAlreadyFillTags;

    public HierarchicalStoreEntryViewModel(Node<IHierarchicalStoreEntry<TKey>, TKey> node,
        HierarchicalStoreViewModel<TKey, TFile> context, ILogService log)
    {
        _node = node;
        _context = context;
        _log = log;
        node.Children.Connect()
            .Transform(x =>
                (HierarchicalStoreEntryViewModel)new HierarchicalStoreEntryViewModel<TKey, TFile>(x, context, log))
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        _tagsSource = new SourceCache<HierarchicalStoreEntryTagViewModel, string>(x => x.Name)
            .DisposeItWith(Disposable);
        _tagsSource.Connect()
            .Bind(out _tags)
            .Subscribe()
            .DisposeItWith(Disposable);
        IsFolder = node.Item.Type == FolderStoreEntryType.Folder;
        IsFile = node.Item.Type == FolderStoreEntryType.File;
        Name = node.Item.Name ?? "";
        Type = node.Item.Type;
        Id = node.Item.Id;
        Description = context.GetEntryDescription(node.Item);
        ParentId = node.Item.ParentId;
    }

    protected override void OnError(HierarchicalStoreEntryAction action, Exception? ex)
    {
        base.OnError(action, ex);
        _log.Error("Store", $"Error to '{action}' entry", ex);
    }

    protected override void Rename(string? name)
    {
        base.Rename(name);
        if (name != null)
        {
            _context.RenameEntryImpl(_node.Item.Id, name);
        }
    }

    protected override void Delete()
    {
        base.Delete();
        _context.DeleteEntryImpl(_node.Item.Id);
    }

    protected override void Move()
    {
        base.Move();
        if (_context.SelectedItemMoveTo == null) return;
        if (_context.SelectedItem == null) return;
        _context.MoveEntryImpl((TKey)_context.SelectedItem.Id, (TKey)_context.SelectedItemMoveTo.Id);
    }

    public override void Refresh()
    {
        base.Refresh();
        if (_isAlreadyFillTags == false)
        {
            _tagsSource.Clear();
            _tagsSource.AddOrUpdate(_context.GetEntryTags((TKey)Id));
            _isAlreadyFillTags = true;
        }
    }

    public override ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items => _items;

    public override ReadOnlyObservableCollection<HierarchicalStoreEntryTagViewModel> Tags => _tags;
}