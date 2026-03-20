using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace AutoFileOrganizer
{
    public class FileOrganizer
    {
        private readonly string _watchDirectory;
        private readonly string _sortedDirectory;
        private FileSystemWatcher _watcher;
        private readonly HashSet<string> _seenHashes;

        public Action<string> OnLogMessage { get; set; }

        public FileOrganizer(string watchDirectory)
        {
            _watchDirectory = watchDirectory;
            _sortedDirectory = Path.Combine(_watchDirectory, "Sorted");

            _seenHashes = new HashSet<string>();

            EnsureDirectoriesExist();
            IndexExistingFiles();
        }

        public void StartWatching()
        {
            _watcher = new FileSystemWatcher(_watchDirectory)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;

            Log($"Started watching: {_watchDirectory}");
        }

        public void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                Log("Stopped watching.");
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            System.Threading.Thread.Sleep(500);

            try
            {
                string fileHash = ComputeFileHash(e.FullPath);
                string fileExtension = Path.GetExtension(e.FullPath).ToLower();
                string fileName = Path.GetFileName(e.FullPath);

                if (_seenHashes.Contains(fileHash))
                {
                    // Route duplicates into the Sorted/Duplicates folder
                    string dupPath = Path.Combine(_sortedDirectory, "Duplicates", fileName);
                    dupPath = EnsureUniqueFileName(dupPath);
                    File.Move(e.FullPath, dupPath);
                    Log($"DUPLICATE DETECTED: Moved {fileName} to Sorted/Duplicates.");
                    return;
                }

                string targetFolder = DetermineTargetFolder(fileExtension);
                if (targetFolder == null) return;

                // Route new files into the Sorted/[Category] folder
                string destinationPath = Path.Combine(_sortedDirectory, targetFolder, fileName);
                destinationPath = EnsureUniqueFileName(destinationPath);

                File.Move(e.FullPath, destinationPath);
                _seenHashes.Add(fileHash);

                Log($"Moved: {fileName} -> Sorted/{targetFolder}.");
            }
            catch (Exception ex)
            {
                Log($"Error processing {e.Name}: {ex.Message}");
            }
        }

        private string ComputeFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private void IndexExistingFiles()
        {
            Log("Indexing existing files for duplicates...");
            string[] folders = { "Images", "Documents", "Installers", "Duplicates" };

            foreach (var folder in folders)
            {
                // Look inside the Sorted folder during startup
                string folderPath = Path.Combine(_sortedDirectory, folder);
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    try
                    {
                        string hash = ComputeFileHash(file);
                        _seenHashes.Add(hash);
                    }
                    catch { /* Skip unreadable files */ }
                }
            }
            Log($"Indexing complete. {_seenHashes.Count} unique files memorized.");
        }

        private string DetermineTargetFolder(string extension)
        {
            return extension switch
            {
                ".jpg" or ".png" or ".gif" => "Images",
                ".pdf" or ".docx" or ".txt" => "Documents",
                ".exe" or ".msi" => "Installers",
                _ => null
            };
        }

        private void EnsureDirectoriesExist()
        {
            // Ensure the main directory exists
            Directory.CreateDirectory(_sortedDirectory);

            // create the subfolders
            string[] folders = { "Images", "Documents", "Installers", "Duplicates" };
            foreach (var folder in folders)
            {
                Directory.CreateDirectory(Path.Combine(_sortedDirectory, folder));
            }
        }

        private string EnsureUniqueFileName(string destinationPath)
        {
            if (!File.Exists(destinationPath)) return destinationPath;

            string directory = Path.GetDirectoryName(destinationPath);
            string extension = Path.GetExtension(destinationPath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(destinationPath);
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            return Path.Combine(directory, $"{fileNameWithoutExt}_{timestamp}{extension}");
        }

        private void Log(string message)
        {
            OnLogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
