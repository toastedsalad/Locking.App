using Azure.Storage.Blobs.Specialized;
using System;

namespace Locking.App
{
    public class AzureBlobLocker : ILocker
    {
        private readonly IAzureBlobStorageHelper blobStorageHelper;
        private BlobLeaseClient leaseClient;
        private string leaseId;
        public bool IsLockAcquired { get; set; }

        public AzureBlobLocker(IAzureBlobStorageHelper blobStorageHelper)
        {
            IsLockAcquired = false;
            this.blobStorageHelper = blobStorageHelper;
        }

        public bool GetLock(LockLocation lockLocation)
        {
            leaseClient = blobStorageHelper.CreateLeaseClient(lockLocation.ConnectionString, lockLocation.ContainerName, lockLocation.BlobName);
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
