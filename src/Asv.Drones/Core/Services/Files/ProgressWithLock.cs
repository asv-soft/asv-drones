using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public sealed class ProgressWithLock(ILoggerFactory? loggerFactory = null) : IDisposable
{
    private readonly ILogger _log = (
        loggerFactory ?? NullLoggerFactory.Instance
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
    /// Disposable scope that represents a single transfer session.
    /// When disposed, it finalizes the transfer by calling Complete().
    /// </summary>
    public readonly struct TransferScope : IDisposable
    {
        private readonly ProgressWithLock _owner;

        /// <summary>Gets a linked cancellation token for the current transfer.</summary>
        public CancellationToken Token { get; }

        /// <summary>
        /// Gets a progress reporter bound to the owner's Report method.
        /// Prefer to reuse if you report often.
        /// </summary>
        public IProgress<double> Reporter { get; }

        internal TransferScope(ProgressWithLock owner, CancellationToken token)
        {
            _owner = owner;
            Token = token;
            Reporter = new Progress<double>(owner.Report);
        }

        public void Dispose()
        {
            _owner.Complete();
        }
    }

    /// <summary>
    /// Starts a transfer session.
    /// Use <c>using var t = _transfer.BeginScope(ct);</c> and pass <c>t.Token</c> to async operations.
    /// </summary>
    /// <returns>A disposable scope.</returns>
    public TransferScope BeginScope(CancellationToken ct = default)
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
            return new TransferScope(this, _cts.Token);
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
    public bool TryCancel()
    {
        using (_sync.EnterScope())
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
