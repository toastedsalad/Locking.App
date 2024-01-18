using System.Diagnostics;
using Azure.Storage.Blobs;


namespace Locking.App.Test
{
    public class AzureBlobLockerTests : IDisposable
    {
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string ContainerName = "testcontainer";
        private const string BlobName = "testlock";
        private const string AzuriteLocation = "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\Extensions\\Microsoft\\Azure Storage Emulator\\azurite.exe";
        private const string AzuriteArguments = "-l -s C:\\temp\\ --skipApiVersionCheck"; // Local folder for Azurite storage
        private BlobServiceClient blobServiceClient;
        private BlobContainerClient blobContainerClient;
        private Process azuriteProcess;

        public AzureBlobLockerTests()
        {
            StartAzurite();
            SetupBlobContainer();
        }

        private void StartAzurite()
        {
            azuriteProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = AzuriteLocation,
                    Arguments = AzuriteArguments,
                    UseShellExecute = true
                }
            };

            azuriteProcess.Start();
        }

        private void SetupBlobContainer()
        {
            blobServiceClient = new BlobServiceClient(ConnectionString);
            blobContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            blobContainerClient.CreateIfNotExists();
        }

        [Fact]
        public void AzureBlobLocker_GetLock_Should_Acquire_Lock_When_Blob_Is_Available()
        {
            var lockLocation = new LockLocation
            {
                ConnectionString = ConnectionString,
                ContainerName = ContainerName,
                BlobName = BlobName
            };
            var locker = new AzureBlobLocker();

            bool lockAcquired = locker.GetLock(lockLocation);

            Assert.True(lockAcquired);
            Assert.True(locker.IsLockAcquired);
        }

        [Fact]
        public void AzureBlobLocker_GetLock_Should_Return_False_When_Lock_Cant_Be_Acquired()
        {
            var lockLocation = new LockLocation
            {
                ConnectionString = ConnectionString,
                ContainerName = ContainerName,
                BlobName = BlobName
            };

            // First locker acquires the lock
            var firstLocker = new AzureBlobLocker();
            bool firstLockAcquired = firstLocker.GetLock(lockLocation);
            Assert.True(firstLockAcquired);

            // Second locker attempts to acquire the same lock
            var secondLocker = new AzureBlobLocker();
            bool secondLockAcquired = secondLocker.GetLock(lockLocation);

            // Assert that the second locker could not acquire the lock
            Assert.False(secondLockAcquired);

            // Cleanup: Release the lock
            if (firstLocker.IsLockAcquired)
            {
                firstLocker.ReleaseLock(lockLocation);
            }
        }

        [Fact]
        public void Locker_ReleaseLock_Should_Return_True_When_Lock_Is_Released()
        {
            var lockLocation = new LockLocation
            {
                ConnectionString = ConnectionString,
                ContainerName = ContainerName,
                BlobName = BlobName
            };

            // Acquire the lock
            var locker = new AzureBlobLocker();
            bool lockAcquired = locker.GetLock(lockLocation);
            Assert.True(lockAcquired);

            // Attempt to release the lock
            bool lockReleased = locker.ReleaseLock(lockLocation);

            // Assert that the lock was released
            Assert.True(lockReleased);
            Assert.False(locker.IsLockAcquired);
        }

        public void Dispose()
        {
            CleanupBlobContainer();
            StopAzurite();
        }

        private void CleanupBlobContainer()
        {
            blobContainerClient.DeleteIfExists();
        }

        private void StopAzurite()
        {
            if (!azuriteProcess.HasExited)
            {
                azuriteProcess.Kill();
                azuriteProcess.WaitForExit();
            }
        }
    }

}
