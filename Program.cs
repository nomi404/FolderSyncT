using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TestAssignment
{
    class FolderSync : IDisposable
    {
        private readonly object syncLock = new object();
        private readonly string sourceFolderPath;
        private readonly string replicaFolderPath;
        private readonly string logFilePath;
        private readonly int syncIntervalSeconds;
        private bool disposed = false;

        public FolderSync(string sourceFolderPath, string replicaFolderPath, string logFilePath, int syncIntervalSeconds)
        {
            this.sourceFolderPath = sourceFolderPath;
            this.replicaFolderPath = replicaFolderPath;
            this.logFilePath = logFilePath;
            this.syncIntervalSeconds = syncIntervalSeconds;
        }
        public void Start()
        {
            Console.WriteLine($"Synchronization initiated. Source: {sourceFolderPath}, Replica: {replicaFolderPath}");

            // Message to show when you press Ctrl+C.
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Synchronization stopped by user.");
                Dispose();
                Environment.Exit(0);
            };

            while (true)
            {
                try
                {
                    SynchronizeFolders();
                    Console.WriteLine($"Synchronization completed at {DateTime.Now}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during synchronization: {ex.Message}");
                }

                Thread.Sleep(syncIntervalSeconds * 1000);
            }
        }

        private void SynchronizeFolders()
        {
            string[] sourceFiles = Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories);

            // Synchronize files in parallel
            Parallel.ForEach(sourceFiles, sourceFilePath =>
            {
                string relativePath = sourceFilePath.Substring(sourceFolderPath.Length + 1);
                string replicaFilePath = Path.Combine(replicaFolderPath, relativePath);

                if (!File.Exists(replicaFilePath) || File.GetLastWriteTimeUtc(sourceFilePath) > File.GetLastWriteTimeUtc(replicaFilePath))
                {
                    lock (syncLock) // Lock to ensure thread safety when writing to the log file
                    {
                        File.Copy(sourceFilePath, replicaFilePath, true);
                        LogAction($"Copied/Updated: {relativePath}");
                    }
                }
            });

            string[] replicaFiles = Directory.GetFiles(replicaFolderPath, "*", SearchOption.AllDirectories);

            // Synchronize replica files in parallel
            Parallel.ForEach(replicaFiles, replicaFilePath =>
            {
                string relativePath = replicaFilePath.Substring(replicaFolderPath.Length + 1);
                string sourceFilePath = Path.Combine(sourceFolderPath, relativePath);

                if (!File.Exists(sourceFilePath))
                {
                    lock (syncLock) // Lock to ensure thread safety when writing to the log file
                    {
                        File.Delete(replicaFilePath);
                        LogAction($"Removed: {relativePath}");
                    }
                }
            });
        }

        private void LogAction(string action)
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    using (FileStream fs = File.Create(logFilePath))
                    {
                        //Creates the file
                    }
                }
                string logEntry = $"{DateTime.Now} - {action}";
                // Log to console
                Console.WriteLine(logEntry);
                // Log to file using File.AppendAllText
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Make sure you have the necessary permissions to write to the log file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                }

                disposed = true;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: TestAssignment.exe <sourceFolderPath> <replicaFolderPath> <logFilePath> <syncIntervalSeconds>");
                return;
            }

            using (FolderSync fileSyncInstance = new FolderSync(args[0], args[1], args[2], int.Parse(args[3])))
            {
                fileSyncInstance.Start();
            }
        }
    }
}
