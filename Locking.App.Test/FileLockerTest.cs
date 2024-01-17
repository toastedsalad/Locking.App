using System.IO.Abstractions.TestingHelpers;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure;
using Moq;

namespace Locking.App.Test
{
    public class FileLockerTest
    {
        [Fact]
        public void FileLocker_GetLock_Should_Return_True_When_Lock_Is_Acquired()
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
        public void FileLocker_GetLock_Should_Return_False_When_Lock_Cant_Be_Acquired()
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
        public void FileLocker_ReleaseLock_Should_Return_True_When_Lock_Is_Released()
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
        public void AzureBlobLocker_GetLock_Should_Call_Acquire_Once()
        {
            // Arrange
            var mockBlobStorageHelper = new Mock<IAzureBlobStorageHelper>();
            var mockLeaseClient = new Mock<BlobLeaseClient>();
            var locker = new AzureBlobLocker(mockBlobStorageHelper.Object);
            var lockLocation = new LockLocation { };

            mockBlobStorageHelper.Setup(m => m.CreateLeaseClient(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockLeaseClient.Object);

            mockLeaseClient.Setup(m => m.Acquire(It.IsAny<TimeSpan>(), null, CancellationToken.None))
                .Returns(Response.FromValue((BlobLease)null, null));

            // Act
            locker.GetLock(lockLocation);

            // Assert
            mockLeaseClient.Verify(m => m.Acquire(It.IsAny<TimeSpan>(), null, CancellationToken.None), Times.Once);
        }

        [Fact]
        public void AzureBlobLocker_GetLock_Should_Return_False_WhenLockNotAcquired()
        {
            // Arrange
            var mockBlobStorageHelper = new Mock<IAzureBlobStorageHelper>();
            var mockLeaseClient = new Mock<BlobLeaseClient>();
            var locker = new AzureBlobLocker(mockBlobStorageHelper.Object);
            var lockLocation = new LockLocation { };

            mockBlobStorageHelper.Setup(m => m.CreateLeaseClient(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockLeaseClient.Object);

            mockLeaseClient.Setup(m => m.Acquire(It.IsAny<TimeSpan>(), null, CancellationToken.None))
                .Throws(new Exception());

            // Act
            bool result = locker.GetLock(lockLocation);

            // Assert
            Assert.False(result);
        }


    }
}