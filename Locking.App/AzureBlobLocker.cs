using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Locking.App
{
    public class AzureBlobLocker : ILocker
    {
        private BlobLeaseClient leaseClient;
        private string leaseId;
        public bool IsLockAcquired { get; private set; }

        public AzureBlobLocker()
        {
            IsLockAcquired = false;
        }

        public bool GetLock(LockLocation lockLocation)
        {
            var blobServiceClient = new BlobServiceClient(lockLocation.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(lockLocation.ContainerName);
            var blobClient = blobContainerClient.GetBlobClient(lockLocation.BlobName);

            // Create the container if it doesn't exist
            blobContainerClient.CreateIfNotExists();

            // Check if the blob is there and if it is don't try to create it
            if (!blobClient.Exists())
            {
                blobClient.Upload(new BinaryData(Array.Empty<byte>()));
            }

            leaseClient = blobClient.GetBlobLeaseClient();
            try
            {
                var leaseDuration = new TimeSpan(-1); // Infinite lease
                var lease = leaseClient.Acquire(leaseDuration);
                leaseId = lease.Value.LeaseId;
                IsLockAcquired = true;
                return true;
            }
            catch
            {
                IsLockAcquired = false;
                return false;
            }
        }

        public bool ReleaseLock(LockLocation lockLocation)
        {
            if (!IsLockAcquired) return true;

            if (leaseClient != null && !string.IsNullOrEmpty(leaseId))
            {
                leaseClient.Release();
                leaseId = null;
                IsLockAcquired = false;
                return true;
            }

            IsLockAcquired = false;
            return true;
        }
    }
}
