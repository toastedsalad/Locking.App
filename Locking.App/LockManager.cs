using System;

namespace Locking.App
{
    public static class LockManager
    {
        public static bool TryAcquireLockAndExecute(ILocker locker, LockLocation lockLocation, Action workToDo)
        {
            if (locker.GetLock(lockLocation))
            {
                try
                {
                    workToDo.Invoke();
                }
                finally
                {
                    locker.ReleaseLock(lockLocation);
                }
                return true; // Work is done, exit the loop
            }
            return false; // Lock not acquired, keep trying
        }
    }
}
