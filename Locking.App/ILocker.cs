using System.IO.Abstractions;

namespace Locking.App
{
    public interface ILocker
    {
        public bool GetLock(LockLocation lockLocation);
        public bool ReleaseLock(LockLocation lockLocation);
    }
}