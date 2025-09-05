using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class ProgressWithLock(ILoggerFactory? loggerFactory = null) : IDisposable
{
    private readonly ILogger _log = (
        loggerFactory ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance
    ).CreateLogger<ProgressWithLock>();
    private CancellationTokenSource? _cts;
    private readonly Lock _sync = new();

    /// <summary>Gets true while a transfer is in progress.</summary>
    public ReactiveProperty<bool> IsTransferInProgress { get; } = new(false);

    /// <summary>Gets controls progress overlay visibility.</summary>
    public ReactiveProperty<bool> IsProgressVisible { get; } = new(false);

    /// <summary>Gets progress value in range [0..1].</summary>
    public ReactiveProperty<double> Progress { get; } = new(0);

    /// <summary>
    /// Starts a transfer session.
    /// </summary>
    /// <param name="ct">External token.</param>
    /// <returns>A linked CancellationToken.</returns>
    public CancellationToken Begin(CancellationToken ct = default)
    {
        using (_sync.EnterScope())
        {
            _cts?.Dispose();
            _cts = ct.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(ct)
                : new CancellationTokenSource();

            IsTransferInProgress.OnNext(true);
            IsProgressVisible.OnNext(true);
            Progress.OnNext(0);

            _log.LogDebug("Transfer started");
            return _cts.Token;
        }
    }

    /// <summary>
    /// Finish current transfer (hides progress, disposes CTS).
    /// Safe to call multiple times.
    /// </summary>
    public void Complete()
    {
        using (_sync.EnterScope())
        {
            if (IsTransferInProgress.Value || IsProgressVisible.Value)
            {
                IsTransferInProgress.OnNext(false);
                IsProgressVisible.OnNext(false);
                Progress.OnNext(0);
                _log.LogDebug("Transfer completed");
            }

            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Update progress safely (0..1). Values outside the range are clamped.
    /// </summary>
    /// <param name="value">Progress value.</param>
    public void Report(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            value = 0;
        }

        if (value < 0)
        {
            value = 0;
        }

        if (value > 1)
        {
            value = 1;
        }

        Progress.OnNext(value);
    }

    /// <summary>
    /// Tries to cancel the current transfer if any.
    /// </summary>
    /// <returns>True if cancellation is successful.</returns>
    public bool TryCancel()
    {
        lock (_sync)
        {
            if (_cts == null)
            {
                return false;
            }

            try
            {
                _cts.Cancel();
                _log.LogInformation("Transfer cancellation requested");
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }

    public void Dispose()
    {
        Complete();
        IsTransferInProgress.Dispose();
        IsProgressVisible.Dispose();
        Progress.Dispose();
    }
}
