using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

[ExportSetup(PageId)]
public sealed class SetupFrameTypeViewModel : SetupSubpage
{
    public const string PageId = "frame-type";
    private readonly ReactiveProperty<bool> _isLoading;
    private readonly ILoggerFactory _loggerFactory;

    private readonly YesOrNoDialogPrefab _yesOrNoDialog;

    private IFrameClient? _frameClient;

    private ISynchronizedView<
        KeyValuePair<string, IMotorFrame>,
        MotorFrameItemViewModel
    >? _frameView;

    public SetupFrameTypeViewModel()
        : this(NullLoggerFactory.Instance, NullDialogService.Instance)
    {
        var frames = new ObservableList<MotorFrameItemViewModel>(
            Enumerable
                .Range(1, 10)
                .Select(n => new MotorFrameItemViewModel(
                    new FakeMotorFrame { Id = $"frame-id-{n}" },
                    _loggerFactory
                ))
        );

        CurrentFrame = new BindableReactiveProperty<IMotorFrame?>(frames[0].Model).DisposeItWith(
            Disposable
        );
        CurrentFrameLabel = new BindableReactiveProperty<string>(
            CurrentFrame?.Value?.Id ?? RS.SetupFrameTypeViewModel_CurrentFrame_Unknown
        ).DisposeItWith(Disposable);

        Frames = frames.ToNotifyCollectionChangedSlim();

        RefreshCommand = new ReactiveCommand<Unit>(
            async (_, cancel) =>
            {
                _isLoading.Value = true;
                await Task.Delay(500, cancel);
                _isLoading.Value = false;
            }
        ).DisposeItWith(Disposable);

        ApplyCommand = new ReactiveCommand<MotorFrameItemViewModel>(
            async (frameItem, cancel) =>
            {
                var frameId = frameItem.Model.Id;

                if (frameId != CurrentFrame?.Value?.Id)
                {
                    _isLoading.Value = true;
                    await Task.Delay(1000, cancel);
                    _isLoading.Value = false;
                }
            }
        ).DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public SetupFrameTypeViewModel(ILoggerFactory loggerFactory, IDialogService dialogService)
        : base(PageId, loggerFactory)
    {
        _yesOrNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _loggerFactory = loggerFactory;

        _isLoading = new ReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsLoading = _isLoading.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);

        SelectedFrame = new BindableReactiveProperty<MotorFrameItemViewModel?>(null).DisposeItWith(
            Disposable
        );

        RefreshCommand = new ReactiveCommand(
            RefreshCurrentFrame,
            AwaitOperation.Drop
        ).DisposeItWith(Disposable);

        ApplyCommand = new ReactiveCommand<MotorFrameItemViewModel>(
            async (frameItem, cancel) =>
            {
                var frameId = frameItem.Model.Id;

                if (frameId != CurrentFrame?.Value?.Id)
                {
                    await this.ExecuteCommand(
                        ChangeFrameTypeCommand.Id,
                        CommandArg.CreateString(frameId),
                        cancel
                    );
                }
            }
        ).DisposeItWith(Disposable);
    }

    public IReadOnlyBindableReactiveProperty<bool> IsLoading { get; }
    public IReadOnlyBindableReactiveProperty<string>? CurrentFrameLabel { get; private set; }
    public IReadOnlyBindableReactiveProperty<IMotorFrame?>? CurrentFrame { get; private set; }

    public INotifyCollectionChangedSynchronizedViewList<MotorFrameItemViewModel>? Frames
    {
        get;
        private set;
    }

    public BindableReactiveProperty<MotorFrameItemViewModel?> SelectedFrame { get; }

    public ReactiveCommand<MotorFrameItemViewModel> ApplyCommand { get; }
    public ReactiveCommand<Unit> RefreshCommand { get; }

    public override IExportInfo Source => SystemModule.Instance;

    protected override ValueTask OnDeviceConnected(IClientDevice device, CancellationToken cancel)
    {
        _frameClient = device.GetMicroservice<IFrameClient>();

        if (_frameClient is null)
        {
            Logger.LogWarning("Frame configuration is not available for the current device");
            return ValueTask.CompletedTask;
        }

        CurrentFrame = _frameClient
            .CurrentMotorFrame.ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);
        CurrentFrameLabel = _frameClient
            .CurrentMotorFrame.Select(motorFrame =>
                string.IsNullOrWhiteSpace(motorFrame?.Id)
                    ? RS.SetupFrameTypeViewModel_CurrentFrame_Unknown
                    : motorFrame.Id
            )
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        _frameView = _frameClient
            .MotorFrames.CreateView(motorFrame => new MotorFrameItemViewModel(
                motorFrame.Value,
                _loggerFactory
            ))
            .DisposeItWith(Disposable);
        _frameView.SetRoutableParent(this).DisposeItWith(Disposable);
        _frameView.DisposeMany().DisposeItWith(Disposable);

        Frames = _frameView.ToNotifyCollectionChanged().DisposeItWith(Disposable);

        _frameClient
            .CurrentMotorFrame.Subscribe(currentMotorFrame =>
            {
                if (currentMotorFrame is null)
                {
                    return;
                }

                foreach (var motorFrame in Frames)
                {
                    motorFrame.IsCurrent.Value = motorFrame.Model.Id == currentMotorFrame.Id;
                }
            })
            .DisposeItWith(Disposable);

        return ValueTask.CompletedTask;
    }

    internal async Task ChangeFrameType(string frameId, CancellationToken cancel = default)
    {
        if (_frameClient is null)
        {
            return;
        }

        if (_frameClient.MotorFrames.TryGetValue(frameId, out var motorFrame))
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

                _isLoading.Value = true;

                await _frameClient.SetFrame(motorFrame, cancel);

                Logger.LogInformation("Frame set to: {FrameId}", motorFrame.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to set frame");
            }
            finally
            {
                _isLoading.Value = false;
            }
        }
    }

    private async ValueTask RefreshCurrentFrame(Unit unit, CancellationToken cancel = default)
    {
        if (_frameClient is null)
        {
            return;
        }

        try
        {
            _isLoading.Value = true;

            await _frameClient.RefreshCurrentFrame(cancel);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to refresh frame");
        }
        finally
        {
            _isLoading.Value = false;
        }
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var childRoutable in base.GetRoutableChildren())
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
}
