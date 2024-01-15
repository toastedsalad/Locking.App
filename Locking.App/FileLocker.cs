using System.IO;

namespace Locking.App
{
    public class FileLocker : ILocker
    {
        public bool IsLockAcquired { get; set; }

        public FileLocker()
        {
            IsLockAcquired = false;
        }

        public bool GetLock(LockLocation lockLocation)
        {
            string filePath = GetFilePath(lockLocation);
            if (lockLocation.FileSystem.File.Exists(filePath))
            {
                IsLockAcquired = false;
                return false;
            }

            lockLocation.FileSystem.Directory.CreateDirectory(lockLocation.LockDirectory);
            using (var stream = lockLocation.FileSystem.File.Create(filePath))
            {
                // File creation logic here
            }
            IsLockAcquired = true;
            return true;
        }

        public bool ReleaseLock(LockLocation lockLocation)
        {
            if (!IsLockAcquired) return true;

            string filePath = GetFilePath(lockLocation);
            if (lockLocation.FileSystem.File.Exists(filePath))
            {
                lockLocation.FileSystem.File.Delete(filePath);
                IsLockAcquired = false;
                return true;
            }
            IsLockAcquired = false;
            return true;
        }

        public static string GetFilePath(LockLocation lockLocation)
        {
            return Path.Combine(lockLocation.LockDirectory, lockLocation.LockName);
        }
    }
}
