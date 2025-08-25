using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

/// <inheritdoc />
public sealed class TransferController(ILoggerFactory? loggerFactory = null) : ITransferController
{
    private readonly ILogger _log = (
        loggerFactory ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance
    ).CreateLogger<TransferController>();
    private CancellationTokenSource? _cts;
    private readonly Lock _sync = new();

    public ReactiveProperty<bool> IsTransferInProgress { get; } = new(false);
    public ReactiveProperty<bool> IsProgressVisible { get; } = new(false);
    public ReactiveProperty<double> Progress { get; } = new(0);

    public CancellationToken Begin(CancellationToken externalToken = default)
    {
        lock (_sync)
        {
            _cts?.Dispose();
            _cts = externalToken.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(externalToken)
                : new CancellationTokenSource();

            IsTransferInProgress.OnNext(true);
            IsProgressVisible.OnNext(true);
            Progress.OnNext(0);

            _log.LogDebug("Transfer started");
            return _cts.Token;
        }
    }

    public void Complete()
    {
        lock (_sync)
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
