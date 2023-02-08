#if !MONO
#define UseFastResourceLock
#endif
namespace Asv.Avalonia.Map
{

    public sealed class FastReaderWriterLock : IDisposable
    {
        private ReaderWriterLockSlim _rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public FastReaderWriterLock()
        {
            
        }

        public void Dispose()
        {
            _rwlock.Dispose();
        }

        public void AcquireWriterLock()
        {
            _rwlock.EnterWriteLock();
        }

        public void ReleaseReaderLock()
        {
            _rwlock.ExitReadLock();
        }

        public void ReleaseWriterLock()
        {
            _rwlock.ExitWriteLock();
        }

        public void AcquireReaderLock()
        {
            _rwlock.EnterReadLock();
        }
    }
}
