using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using Material.Icons.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

[ExportShellPage(WellKnownUri.ShellPageLogViewer)]
public class LogViewerViewModel : ShellPage
{
    private const int MinPageIndex = 1;

    private readonly ReadOnlyObservableCollection<LogItemViewModel> _logItems;
    private readonly ISourceCache<LogItemViewModel, int> _logSource;

    private readonly IApplicationHost _app;
    private readonly ILogService _log;

    private HashSet<LogMessageType> _logLevels;
    private HashSet<string> _logClasses;
    private HashSet<string> _logThreadIds;

    protected LogViewerViewModel() : base(WellKnownUri.Undefined)
    {
    }

    [ImportingConstructor]
    public LogViewerViewModel(IApplicationHost app, ILogService log) : base(WellKnownUri.ShellPageLogViewerUri)
    {
        _app = app;
        _log = log;

        Icon = MaterialIconKind.Journal;
        Title = RS.LogViewer_Name;

        _logSource = new SourceCache<LogItemViewModel, int>(item => item.Index);

        _logSource.Connect()
            .Filter(FilterItem)
            .SortBy(_ => _.Index, SortDirection.Descending)
            .Bind(out _logItems)
            .Subscribe(_ => FilteredItemsCount = _logItems.Count)
            .DisposeItWith(Disposable);

        PageSize = PageSizes.First();

        _logLevels = new HashSet<LogMessageType>();
        _logClasses = new HashSet<string>();
        _logThreadIds = new HashSet<string>();

        _logSource.AddOrUpdate(_log.LoadItemsFromLogFile());

        _logSource.Items.Select(_ => _.ThreadId).ForEach(tread => _logThreadIds.Add(tread));
        _logSource.Items.Select(_ => _.Level).ForEach(level => _logLevels.Add(level));
        _logSource.Items.Select(_ => _.Class).ForEach(logClass => _logClasses.Add(logClass));

        TotalItemsCount = _logSource.Items.Count();
        TotalPagesCount = (int)Math.Ceiling(TotalItemsCount / PageSize);

        AvailableLevels.ForEach(level => SelectedLevels.Add(level));
        AvailableClasses.ForEach(cls => SelectedClasses.Add(cls));
        AvailableThreadIds.ForEach(thread => SelectedThreadIds.Add(thread));

        this.WhenAnyValue(vm => vm.SearchText)
            .Subscribe(_ =>
            {
                CurrentPageIndex = 1;
                UpdatePage();
            })
            .DisposeItWith(Disposable);

        this.WhenAnyValue(vm => vm.PageSize,
                vm => vm.FilteredItemsCount)
            .Subscribe(tuple =>
            {
                var (pageSize, filteredCount) = tuple;
                TotalPagesCount = (int)Math.Ceiling(filteredCount / pageSize);
                if (CurrentPageIndex > TotalPagesCount) CurrentPageIndex = TotalPagesCount;
                if (CurrentPageIndex < MinPageIndex) CurrentPageIndex = MinPageIndex;
                UpdatePage();
            })
            .DisposeItWith(Disposable);

        MoveToPreviousPageCommand = ReactiveCommand.Create(MoveToPreviousPage,
            this.WhenAnyValue(vm => vm.CurrentPageIndex,
                    vm => vm.TotalPagesCount)
                .Select(tuple => tuple.Item1 > MinPageIndex));

        MoveToNextPageCommand = ReactiveCommand.Create(MoveToNextPage,
            this.WhenAnyValue(vm => vm.CurrentPageIndex,
                    vm => vm.TotalPagesCount)
                .Select(tuple => tuple.Item1 < tuple.Item2));

        ClearSearchTextCommand = ReactiveCommand.Create(() => SearchText = string.Empty,
            this.WhenAnyValue(vm => vm.SearchText)
                .Select(text => !string.IsNullOrEmpty(text)));

        SelectAllThreadIdsCommand =
            ReactiveCommand.Create(() => AvailableThreadIds.ForEach(thread => SelectedThreadIds.Add(thread)));
        DeselectAllThreadIdsCommand = ReactiveCommand.Create(() => SelectedThreadIds.Clear());
        SelectAllClassesCommand =
            ReactiveCommand.Create(() => AvailableClasses.ForEach(cls => SelectedClasses.Add(cls)));
        DeselectAllClassesCommand = ReactiveCommand.Create(() => SelectedClasses.Clear());
        SelectAllLevelsCommand =
            ReactiveCommand.Create(() => AvailableLevels.ForEach(level => SelectedLevels.Add(level)));
        DeselectAllLevelsCommand = ReactiveCommand.Create(() => SelectedLevels.Clear());

        ClearLogsCommand = ReactiveCommand.Create(ClearLogs);
    }

    private bool FilterItem(LogItemViewModel item)
    {
        var containsMessage = string.IsNullOrEmpty(SearchText) ||
                              item.Message.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
        var containsLevel = SelectedLevels.Contains(item.Level);
        var containsClass = SelectedClasses.Contains(item.Class);
        var containsThreadId = SelectedThreadIds.Contains(item.ThreadId);

        return containsMessage && containsLevel && containsClass && containsThreadId;
    }

    private void MoveToPreviousPage()
    {
        CurrentPageIndex--;
        UpdatePage();
    }

    private void MoveToNextPage()
    {
        CurrentPageIndex++;
        UpdatePage();
    }

    public void UpdatePage()
    {
        _logSource.Refresh();

        var startLineIndex = (int)((CurrentPageIndex - 1) * PageSize);
        var endLineIndex = Math.Min(startLineIndex + (int)PageSize, _logItems.Count);

        LogItems.Clear();

        for (int i = startLineIndex; i < endLineIndex; i++)
        {
            LogItems.Add(_logItems[i]);
        }
    }

    private async Task ClearLogs()
    {
        var titlePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal, Spacing = 8,
            Children =
            {
                new MaterialIcon
                {
                    Foreground = new SolidColorBrush(Colors.Yellow), Height = 20, Width = 20,
                    Kind = MaterialIconKind.Warning
                },
                new TextBlock { Text = RS.LogViewerViewModel_ClearDialog_Title }
            }
        };

        var confirmDialog = new ContentDialog
        {
            Title = titlePanel,
            Content = RS.LogViewerViewModel_ClearDialog_Content,
            PrimaryButtonText = RS.LogViewerViewModel_ClearDialog_PrimaryButton,
            CloseButtonText = RS.LogViewerViewModel_ClearDialog_CloseButton
        };

        var result = await confirmDialog.ShowAsync();

        if (result is ContentDialogResult.Primary)
        {
            LogItems.Clear();
            _log.DeleteLogFile();
            _log.Warning(nameof(LogViewerViewModel), "Log have been cleared!");
        }
    }

    public ICommand SelectAllThreadIdsCommand { get; }
    public ICommand DeselectAllThreadIdsCommand { get; }
    public ICommand SelectAllClassesCommand { get; }
    public ICommand DeselectAllClassesCommand { get; }
    public ICommand SelectAllLevelsCommand { get; }
    public ICommand DeselectAllLevelsCommand { get; }

    public ICommand MoveToPreviousPageCommand { get; }
    public ICommand MoveToNextPageCommand { get; }
    public ICommand ClearSearchTextCommand { get; }
    public ICommand ClearLogsCommand { get; }

    public List<double> PageSizes => [25, 50, 100, 200];

    public IEnumerable<LogMessageType> AvailableLevels => _logLevels;
    public IEnumerable<string> AvailableClasses => _logClasses;
    public IEnumerable<string> AvailableThreadIds => _logThreadIds;
    [Reactive] public ObservableCollection<LogItemViewModel> LogItems { get; set; } = [];

    [Reactive] public int TotalItemsCount { get; set; }
    [Reactive] public int FilteredItemsCount { get; set; }
    [Reactive] public string SearchText { get; set; }

    [Reactive] public int CurrentPageIndex { get; set; }
    [Reactive] public int TotalPagesCount { get; set; }
    [Reactive] public double PageSize { get; set; }

    [Reactive] public ObservableCollection<LogMessageType> SelectedLevels { get; set; } = [];
    [Reactive] public ObservableCollection<string> SelectedClasses { get; set; } = [];
    [Reactive] public ObservableCollection<string> SelectedThreadIds { get; set; } = [];

    [Reactive] public SortDirection SortDirectionType { get; set; }
}