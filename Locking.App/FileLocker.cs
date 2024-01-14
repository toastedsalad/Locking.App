
namespace Locking.App
{
    public class FileLocker : ILocker
    {
        public bool GetLock(LockLocation lockLocation)
        {
            string filePath = GetFilePath(lockLocation);
            if (lockLocation.FileSystem.File.Exists(filePath))
            {
                return false;
            }

            lockLocation.FileSystem.Directory.CreateDirectory(lockLocation.LockDirectory);
            // The using statement ensures that the stream is closed and the file is released after creation.
            using var stream = lockLocation.FileSystem.File.Create(filePath);
            return true;
        }

        public bool ReleaseLock(LockLocation lockLocation)
        {
            string filePath = GetFilePath(lockLocation);
            if (lockLocation.FileSystem.File.Exists(filePath))
            {
                lockLocation.FileSystem.File.Delete(filePath);
                return true;
            }
            return true;
        }

        public static string GetFilePath(LockLocation lockLocation)
        {
            return $"{lockLocation.LockDirectory}/{lockLocation.LockName}";
        }
    }
}
