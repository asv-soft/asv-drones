using System.Collections.Immutable;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class SetupFrameTypeViewModel : SetupSubpage
{
    public const string PageId = "frameType";
    public const MaterialIconKind Icon = MaterialIconKind.ThemeLightDark;

    private readonly SynchronizedReactiveProperty<bool> _isRefreshing;
    private readonly SynchronizedReactiveProperty<bool> _isChangingFrame;
    private readonly YesOrNoDialogPrefab _yesOrNoDialog;
    private readonly IFrameClient? _frameClient;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoSink;
    private readonly ISynchronizedView<
        KeyValuePair<string, IDroneFrame>,
        DroneFrameItemViewModel
    >? _framesView;
    private readonly ISynchronizedViewFilter<
        KeyValuePair<string, IDroneFrame>,
        DroneFrameItemViewModel
    > _framesViewFilter;

    public SetupFrameTypeViewModel()
        : this(
            DesignTimeSetupSubPageContext.Instance,
            NullLoggerFactory.Instance,
            NullDialogService.Instance.GetDialogPrefab<YesOrNoDialogPrefab>()
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        var frames = new ObservableList<DroneFrameItemViewModel>(
            Enumerable
                .Range(1, 10)
                .Select(n => new DroneFrameItemViewModel(
                    new NullDroneFrame { Id = $"frame-id-{n}" }
                ))
        );

        CurrentFrame = new BindableReactiveProperty<IDroneFrame?>(frames[0].Model).DisposeItWith(
            Disposable
        );
        CurrentFrameLabel = new BindableReactiveProperty<string>(
            CurrentFrame?.Value?.Id ?? RS.SetupFrameTypeViewModel_CurrentFrame_Unknown
        ).DisposeItWith(Disposable);

        Frames = frames.ToNotifyCollectionChangedSlim();
    }

    public SetupFrameTypeViewModel(
        ITreeSubPageContext<ISetupPage> context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : this(context, loggerFactory, dialogService.GetDialogPrefab<YesOrNoDialogPrefab>())
    {
        _frameClient =
            context.Context.Target.CurrentValue?.Device.GetMicroservice<IFrameClient>()
            ?? throw new Exception($"{nameof(IFrameClient)} should not be null");

        CurrentFrame = _frameClient
            .CurrentFrame.ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty()
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
            .Frames.CreateView(droneFrame => new DroneFrameItemViewModel(droneFrame.Value))
            .DisposeItWith(Disposable);
        _framesView.SetParent(this).DisposeItWith(Disposable);
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
    }

    private SetupFrameTypeViewModel(
        ITreeSubPageContext<ISetupPage> context,
        ILoggerFactory loggerFactory,
        YesOrNoDialogPrefab yesOrNoDialog
    )
        : base(PageId, context, loggerFactory)
    {
        _yesOrNoDialog = yesOrNoDialog;

        _isRefreshing = new SynchronizedReactiveProperty<bool>(false).DisposeItWith(Disposable);
        _isChangingFrame = new SynchronizedReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsUpdating = _isRefreshing
            .CombineLatest(_isChangingFrame, (r, c) => r || c)
            .ObserveOnUIThreadDispatcher()
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

        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);

        _undoSink = Undo.RegisterValue<string>("default", ApplyFrameById, ApplyFrameById)
            .DisposeItWith(Disposable);
    }

    public IReadOnlyBindableReactiveProperty<bool> IsUpdating { get; }
    public IReadOnlyBindableReactiveProperty<string>? CurrentFrameLabel { get; private set; }
    public IReadOnlyBindableReactiveProperty<IDroneFrame?>? CurrentFrame { get; private set; }
    public SearchBoxViewModel Search { get; }
    public IReadOnlyDictionary<string, string> MetaFallBack =>
        ImmutableDictionary<string, string>.Empty;

    public INotifyCollectionChangedSynchronizedViewList<DroneFrameItemViewModel>? Frames { get; }

    public BindableReactiveProperty<DroneFrameItemViewModel?> SelectedFrame { get; }

    public ReactiveCommand<Unit> RefreshCommand { get; }

    public override IEnumerable<IViewModel> GetChildren()
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

    private async Task ChangeFrameType(
        string frameId,
        bool record,
        CancellationToken cancel = default
    )
    {
        if (_frameClient is null)
        {
            return;
        }

        var oldFrameId = CurrentFrame?.Value?.Id;

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

                if (record && oldFrameId is not null && oldFrameId != frameId)
                {
                    _undoSink.PublishUpdate(oldFrameId, frameId);
                }
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

    private void ApplyFrameById(string frameId)
    {
        ChangeFrameType(frameId, record: false).SafeFireAndForget();
    }

    private async ValueTask InternalCatchEvent(
        IViewModel src,
        AsyncRoutedEvent<IViewModel> e,
        CancellationToken cancel
    )
    {
        switch (e)
        {
            case CurrentDroneFrameChangeEvent { Sender: DroneFrameItemViewModel param }:
            {
                await ChangeFrameType(param.Model.Id, record: true, cancel);

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
}
