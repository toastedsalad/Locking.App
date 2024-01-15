using System.IO.Abstractions.TestingHelpers;
using Moq;

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
        public void ReleaseLock_Should_Return_True_When_Lock_Is_Released()
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
            locker.IsLockAcquired = true;

            bool releasedLock = locker.ReleaseLock(lockLocation);

            Assert.True(releasedLock);
            Assert.False(lockLocation.FileSystem.File.Exists(filePath));
        }

        [Fact]
        public void TryAcquireLockAndExecute_Should_Execute_Action_When_Lock_Acquired()
        {
            // Arrange
            var mockLocker = new Mock<ILocker>();
            var lockLocation = new LockLocation { };
            bool actionExecuted = false;
            // Here we setup the action that will be executed
            // The action set var to true
            Action mockAction = () => actionExecuted = true;

            mockLocker.Setup(m => m.GetLock(lockLocation)).Returns(true);
            mockLocker.Setup(m => m.ReleaseLock(lockLocation));

            // Act
            var result = LockManager.TryAcquireLockAndExecute(mockLocker.Object, lockLocation, mockAction);

            // Assert
            Assert.True(result);
            Assert.True(actionExecuted);
        }
    }
}