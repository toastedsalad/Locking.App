using System.IO.Abstractions;

namespace Locking.App
{
    public record LockLocation
    {
        public string? LockDirectory { get; set; }
        public string? LockName { get; set; }
        public IFileSystem? FileSystem { get; set; }
    }
}
