using System.IO.Abstractions;

namespace Locking.App
{
    internal class Program
    {
        private static FileLocker locker;
        private static LockLocation lockLocation;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            lockLocation = new LockLocation()
            {
                LockDirectory = @"C:\temp",
                LockName = "lock",
                FileSystem = new FileSystem()
            };
            locker = new FileLocker();

            // Register the CancelKeyPress event handler
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPressHandler);

            try
            {
                if (locker.GetLock(lockLocation))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            Console.WriteLine("Operation cancelled.");
                            break;
                        }
                        Console.WriteLine("Doing work");
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    Console.WriteLine("Could not acquire lock");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Some exception was caught. Releasing lock...");
                locker.ReleaseLock(lockLocation);
            }
            finally
            {
                locker.ReleaseLock(lockLocation);
            }
        }

        private static void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Cancel key pressed. Releasing lock...");
            locker.ReleaseLock(lockLocation);
            cancellationTokenSource.Cancel();
        }
    }
}
