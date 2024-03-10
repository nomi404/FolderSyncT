# FolderSyncT

This repository contains a C# program designed for a synchronization task. The program facilitates the one-way synchronization of two folders, maintaining an identical copy of the source folder in the replica folder.

## Usage

### Command-line Arguments:

- `sourceFolderPath`: The path to the source folder.
- `replicaFolderPath`: The path to the replica folder.
- `logFilePath`: The path to the log file.
- `syncIntervalSeconds`: The synchronization interval in seconds.

### Example Command:

```bash
FodlerSyncT.exe "C:\Path\To\Source" "D:\Path\To\Replica" "C:\Path\To\Log" 60
