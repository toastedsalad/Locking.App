using System.IO.Abstractions;

namespace Locking.App
{
    internal class Program
    {
        private static AzureBlobLocker locker;
        private static LockLocation lockLocation;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            lockLocation = new LockLocation()
            {
                ConnectionString = "connstring",
                ContainerName = "mycontainer",
                BlobName = "lock"
            };
            var helper = new AzureBlobStorageHelper();
            locker = new AzureBlobLocker(helper);

            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPressHandler);

            Action workToDo = () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        Console.WriteLine("Operation cancelled.");
                        break;
                    }
                    Console.WriteLine("Doing work");
                    Thread.Sleep(1000);
                }
            };

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (LockManager.TryAcquireLockAndExecute(locker, lockLocation, workToDo))
                {
                    break;
                }

                Console.WriteLine("Waiting for lock...");
                Thread.Sleep(1000);
            }
        }

        private static void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Cancel key pressed. Operation will be cancelled.");
            cancellationTokenSource.Cancel();

            if (locker != null && locker.IsLockAcquired)
            {
                Console.WriteLine("Releassing lock...");
                locker.ReleaseLock(lockLocation);
            }
        }
    }
}
