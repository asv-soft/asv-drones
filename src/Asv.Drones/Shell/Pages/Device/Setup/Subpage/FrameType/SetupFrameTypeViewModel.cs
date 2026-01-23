using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

[ExportSetup(PageId)]
public sealed class SetupFrameTypeViewModel : SetupSubpage
{
    public const string PageId = "frame-type";
    private readonly SynchronizedReactiveProperty<bool> _isRefreshing;
    private readonly SynchronizedReactiveProperty<bool> _isChangingFrame;
    private readonly ILoggerFactory _loggerFactory;

    private readonly YesOrNoDialogPrefab _yesOrNoDialog;

    private IFrameClient? _frameClient;

    private ISynchronizedView<
        KeyValuePair<string, IDroneFrame>,
        DroneFrameItemViewModel
    >? _framesView;
    private readonly ISynchronizedViewFilter<
        KeyValuePair<string, IDroneFrame>,
        DroneFrameItemViewModel
    > _framesViewFilter;

    public SetupFrameTypeViewModel()
        : this(NullLoggerFactory.Instance, NullDialogService.Instance)
    {
        var frames = new ObservableList<DroneFrameItemViewModel>(
            Enumerable
                .Range(1, 10)
                .Select(n => new DroneFrameItemViewModel(
                    new NullDroneFrame { Id = $"frame-id-{n}" },
                    _loggerFactory
                ))
        );

        CurrentFrame = new BindableReactiveProperty<IDroneFrame?>(frames[0].Model).DisposeItWith(
            Disposable
        );
        CurrentFrameLabel = new BindableReactiveProperty<string>(
            CurrentFrame?.Value?.Id ?? RS.SetupFrameTypeViewModel_CurrentFrame_Unknown
        ).DisposeItWith(Disposable);

        Frames = frames.ToNotifyCollectionChangedSlim();

        RefreshCommand = new ReactiveCommand<Unit>(
            async (_, cancel) =>
            {
                _isRefreshing.Value = true;
                await Task.Delay(500, cancel);
                _isRefreshing.Value = false;
            }
        ).DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public SetupFrameTypeViewModel(ILoggerFactory loggerFactory, IDialogService dialogService)
        : base(PageId, loggerFactory)
    {
        _yesOrNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _loggerFactory = loggerFactory;

        _isRefreshing = new SynchronizedReactiveProperty<bool>(false).DisposeItWith(Disposable);
        _isChangingFrame = new SynchronizedReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsUpdating = _isRefreshing
            .ObserveOnUIThreadDispatcher()
            .CombineLatest(_isChangingFrame, (r, c) => r || c)
            .ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);

        SelectedFrame = new BindableReactiveProperty<DroneFrameItemViewModel?>(null).DisposeItWith(
            Disposable
        );

        RefreshCommand = new ReactiveCommand(Refresh, AwaitOperation.Drop).DisposeItWith(
            Disposable
        );

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        _framesViewFilter = new SynchronizedViewFilter<
            KeyValuePair<string, IDroneFrame>,
            DroneFrameItemViewModel
        >((_, model) => model.Filter(Search.Text.ViewValue.Value));

        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public IReadOnlyBindableReactiveProperty<bool> IsUpdating { get; }
    public IReadOnlyBindableReactiveProperty<string>? CurrentFrameLabel { get; private set; }
    public IReadOnlyBindableReactiveProperty<IDroneFrame?>? CurrentFrame { get; private set; }
    public SearchBoxViewModel Search { get; }
    public IReadOnlyDictionary<string, string> MetaFallBack =>
        ImmutableDictionary<string, string>.Empty;

    public INotifyCollectionChangedSynchronizedViewList<DroneFrameItemViewModel>? Frames
    {
        get;
        private set;
    }

    public BindableReactiveProperty<DroneFrameItemViewModel?> SelectedFrame { get; }

    public ReactiveCommand<Unit> RefreshCommand { get; }

    public override ValueTask Init(ISetupPage context)
    {
        _frameClient =
            context.Target.CurrentValue?.Device.GetMicroservice<IFrameClient>()
            ?? throw new Exception($"{nameof(IFrameClient)} should not be null");

        CurrentFrame = _frameClient
            .CurrentFrame.ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);
        CurrentFrameLabel = _frameClient
            .CurrentFrame.ObserveOnUIThreadDispatcher()
            .Select(droneFrame =>
                string.IsNullOrWhiteSpace(droneFrame?.Id)
                    ? RS.SetupFrameTypeViewModel_CurrentFrame_Unknown
                    : droneFrame.Id
            )
            .ToReadOnlyBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        _framesView = _frameClient
            .Frames.CreateView(droneFrame => new DroneFrameItemViewModel(
                droneFrame.Value,
                _loggerFactory
            ))
            .DisposeItWith(Disposable);
        _framesView.SetRoutableParent(this).DisposeItWith(Disposable);
        _framesView.DisposeMany().DisposeItWith(Disposable);

        Frames = _framesView
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        _frameClient
            .CurrentFrame.ObserveOnUIThreadDispatcher()
            .Subscribe(currentFrame =>
            {
                if (currentFrame is null)
                {
                    return;
                }

                foreach (var frame in Frames)
                {
                    frame.IsCurrent.Value = frame.Model.Id == currentFrame.Id;
                }
            })
            .DisposeItWith(Disposable);

        return ValueTask.CompletedTask;
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return Search;

        foreach (var childRoutable in base.GetChildren())
        {
            yield return childRoutable;
        }

        if (Frames is not null)
        {
            foreach (var vm in Frames)
            {
                yield return vm;
            }
        }
    }

    internal async Task ChangeFrameType(string frameId, CancellationToken cancel = default)
    {
        if (_frameClient is null)
        {
            return;
        }

        if (_frameClient.Frames.TryGetValue(frameId, out var droneFrame))
        {
            try
            {
                var payload = new YesOrNoDialogPayload
                {
                    Title = RS.SetupFrameTypeViewModel_ApplyConfirmation_Title,
                    Message = RS.SetupFrameTypeViewModel_ApplyConfirmation_Message,
                };
                var saveChanges = await _yesOrNoDialog.ShowDialogAsync(payload);

                if (!saveChanges)
                {
                    return;
                }

                _isChangingFrame.Value = true;

                await _frameClient.SetFrame(droneFrame, cancel);

                Logger.LogInformation("Frame set to: {FrameId}", droneFrame.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to set frame");
            }
            finally
            {
                _isChangingFrame.Value = false;
            }
        }
    }

    private async ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
    {
        switch (e)
        {
            case CurrentDroneFrameChangeEvent { Sender: DroneFrameItemViewModel param }:
            {
                await this.ExecuteCommand(
                    ChangeFrameTypeCommand.Id,
                    CommandArg.CreateString(param.Model.Id)
                );

                e.IsHandled = true;
                break;
            }
        }
    }

    private Task UpdateImpl(string? query, IProgress<double> progress, CancellationToken cancel)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _framesView?.ResetFilter();
            return Task.CompletedTask;
        }

        _framesView?.AttachFilter(_framesViewFilter);

        return Task.CompletedTask;
    }

    private async ValueTask Refresh(Unit unit, CancellationToken cancel = default)
    {
        if (_frameClient is null)
        {
            return;
        }

        try
        {
            _isRefreshing.Value = true;

            await _frameClient.RefreshCurrentFrame(cancel);
            await _frameClient.RefreshAvailableFrames(cancel);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to refresh frame");
        }
        finally
        {
            _isRefreshing.Value = false;
        }
    }

    public override IExportInfo Source => SystemModule.Instance;
}
