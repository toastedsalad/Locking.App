using System.IO.Abstractions;

namespace Locking.App
{
    public record LockLocation
    {
        public string? LockDirectory { get; set; }
        public string? LockName { get; set; }
        public IFileSystem? FileSystem { get; set; }
        // Example connection string: DefaultEndpointsProtocol=[http|https];AccountName=myAccountName;AccountKey=myAccountKey
        public string? ConnectionString { get; set; }
        public string? ContainerName { get; set; }
        public string? BlobName { get; set; }
    }
}
