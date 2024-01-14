using System.IO.Abstractions.TestingHelpers;

namespace Locking.App.Test
{
    public class FileLockerTest
    {
        [Fact]
        public void GetLock_Should_Return_True_When_Lock_Is_Acquired()
        {
            var lockLocation = new LockLocation()
            {
                LockDirectory = "/lock",
                LockName = "lockname",
                FileSystem = new MockFileSystem()
            };

            string filePath = FileLocker.GetFilePath(lockLocation);
            var locker = new FileLocker();
            bool lockAcquired = locker.GetLock(lockLocation);

            Assert.True(lockAcquired);
            Assert.True(lockLocation.FileSystem.File.Exists(filePath));
        }

        [Fact]
        public void GetLock_Should_Return_False_When_Lock_Cant_Be_Acquired()
        {
            var lockLocation = new LockLocation()
            {
                LockDirectory = "/lock",
                LockName = "lockname",
                FileSystem = new MockFileSystem()
            };

            string filePath = FileLocker.GetFilePath(lockLocation);
            lockLocation.FileSystem.Directory.CreateDirectory(lockLocation.LockDirectory);
            lockLocation.FileSystem.File.Create(filePath);

            var locker = new FileLocker();
            bool lockAcquired = locker.GetLock(lockLocation);

            Assert.False(lockAcquired);
            Assert.True(lockLocation.FileSystem.File.Exists(filePath));
        }

        [Fact]
        public void ReleaseLock_Should_Return_Truen_When_Lock_Is_Released()
        {
            var lockLocation = new LockLocation()
            {
                LockDirectory = "/lock",
                LockName = "lockname",
                FileSystem = new MockFileSystem()
            };

            string filePath = FileLocker.GetFilePath(lockLocation);
            lockLocation.FileSystem.Directory.CreateDirectory(lockLocation.LockDirectory);
            lockLocation.FileSystem.File.Create(filePath);

            var locker = new FileLocker();
            bool releasedLock = locker.ReleaseLock(lockLocation);

            Assert.True(releasedLock);
            Assert.False(lockLocation.FileSystem.File.Exists(filePath));
        }
    }
}