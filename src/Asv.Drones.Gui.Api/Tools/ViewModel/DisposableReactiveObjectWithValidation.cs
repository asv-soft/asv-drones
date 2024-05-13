using System.Reactive.Disposables;
using ReactiveUI.Validation.Helpers;

namespace Asv.Drones.Gui.Api
{
    public class DisposableReactiveObjectWithValidation : ReactiveValidationObject, IDisposable
    {
        private const int Disposed = 1;
        private const int NotDisposed = 0;
        private int _disposeFlag;
        private CancellationTokenSource? _cancel;
        private CompositeDisposable? _dispose;
        private readonly object _sync1 = new();
        private readonly object _sync2 = new();

        #region Disposing

        protected bool IsDisposed => Thread.VolatileRead(ref _disposeFlag) > 0;

        protected void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
        }

        protected CancellationToken DisposeCancel
        {
            get
            {
                if (_cancel != null)
                {
                    return IsDisposed ? CancellationToken.None : _cancel.Token;
                }

                lock (_sync2)
                {
                    if (_cancel != null)
                    {
                        return IsDisposed ? CancellationToken.None : _cancel.Token;
                    }

                    _cancel = new();
                    return _cancel.Token;
                }
            }
        }

        protected CompositeDisposable Disposable
        {
            get
            {
                if (_dispose != null) return _dispose;
                lock (_sync1)
                {
                    return _dispose ??= new CompositeDisposable();
                }
            }
        }

        protected virtual void InternalDisposeOnce()
        {
            if (_cancel?.Token.CanBeCanceled == true)
                _cancel.Cancel(false);
            _cancel?.Dispose();
            _dispose?.Dispose();
        }

        #endregion

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposeFlag, Disposed, NotDisposed) != NotDisposed) return;
            InternalDisposeOnce();
            GC.SuppressFinalize(this);
        }
    }
}