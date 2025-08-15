using System;
using System.Threading;
using R3;

namespace Asv.Drones;

public interface ITransferController : IDisposable
{
    /// <summary>Gets true while a transfer is in progress.</summary>
    BindableReactiveProperty<bool> IsTransferInProgress { get; }

    /// <summary>Gets controls progress overlay visibility.</summary>
    BindableReactiveProperty<bool> IsProgressVisible { get; }

    /// <summary>Gets progress value in range [0..1].</summary>
    BindableReactiveProperty<double> Progress { get; }

    /// <summary>
    /// Starts a transfer session.
    /// </summary>
    /// <param name="ct">External token.</param>
    /// <returns>A linked CancellationToken.</returns>
    CancellationToken Begin(CancellationToken ct = default);

    /// <summary>
    /// Finish current transfer (hides progress, disposes CTS).
    /// Safe to call multiple times.
    /// </summary>
    void Complete();

    /// <summary>
    /// Update progress safely (0..1). Values outside the range are clamped.
    /// </summary>
    /// <param name="value">Progress value.</param>
    void Report(double value);

    /// <summary>
    /// Tries to cancel the current transfer if any.
    /// </summary>
    /// <returns>True if cancellation is successful.</returns>
    bool TryCancel();
}
