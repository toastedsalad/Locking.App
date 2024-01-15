using System.IO.Abstractions;
using System;
using System.Threading;
using System.Net.Sockets;

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

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (TryAcquireLockAndWork())
                {
                    break;
                }

                Console.WriteLine("Waiting for lock...");
                Thread.Sleep(1000); // Wait for some time before trying again
            }
        }

        private static bool TryAcquireLockAndWork()
        {
            if (locker.GetLock(lockLocation))
            {
                try
                {
                    for (int i = 0; i < 100; i++)
                    {
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            Console.WriteLine("Operation cancelled.");
                            return true; // Exit the method as the operation is cancelled
                        }
                        Console.WriteLine("Doing work");
                        Thread.Sleep(1000);
                    }
                }
                finally
                {
                    locker.ReleaseLock(lockLocation);
                }
                return true; // Work is done, exit the loop
            }
            return false; // Lock not acquired, keep trying
        }


        private static void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Cancel key pressed. Operation will be cancelled.");
            cancellationTokenSource.Cancel();

            // Release the lock if it's acquired
            if (locker != null && locker.IsLockAcquired)
            {
                locker.ReleaseLock(lockLocation);
            }
        }
    }
}