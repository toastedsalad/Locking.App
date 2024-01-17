using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;

public interface IAzureBlobStorageHelper
{
    BlobLeaseClient CreateLeaseClient(string connectionString, string containerName, string blobName);
}

public class AzureBlobStorageHelper : IAzureBlobStorageHelper
{
    public BlobLeaseClient CreateLeaseClient(string connectionString, string containerName, string blobName)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        blobContainerClient.CreateIfNotExists();
        try
        {
            blobClient.Upload(new BinaryData(Array.Empty<byte>()), overwrite: true);
        }
        catch
        {
            return blobClient.GetBlobLeaseClient();
        }
        return blobClient.GetBlobLeaseClient();
    }
}
