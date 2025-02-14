# EasySave - User Manual

## Introduction
EasySave is a software designed to facilitate file and directory backups. This guide provides instructions for both the console and graphical versions of the application.

---

## Installation and Launch

### Console Version:
1. Open File Explorer and navigate to the EasySave directory.
2. Follow this path:
EasySaveConsole/bin/Debug/net9.0
3. Right-click inside the directory and select **Display more options**.
4. Choose **Open in terminal**.
5. In the terminal, execute:
./EasySave.exe
6. Type `help` to display the list of available commands.

### Graphical Version:
1. Open File Explorer and navigate to the EasySave directory.
2. Follow this path:
EasySaveDesktop/bin/Debug/net9.0
3. Double-click the executable file to launch the application.

---

## Using the Console Version

### Available Commands:
| Command | Description |
|---------|------------|
| `./easysave create <name> <sourceDirectory> <targetDirectory> <type>` | Creates a new backup configuration. |
| `./easysave remove <id>` | Deletes a backup configuration using its ID. |
| `./easysave <id1;id2;id3>` | Executes multiple specific backups. |
| `./easysave <id1-id3>` | Executes a sequence of consecutive backups. |
| `./easysave help` | Displays the help message. |

---

## Explanation of Parameters:
- `<name>` : Name of the backup job (e.g., `daily_backup`).
- `<sourceDirectory>` : Path of the source directory (e.g., `C:\Users\Images`).
- `<targetDirectory>` : Path of the destination directory (e.g., `D:\Backup\Images`).
- `<type>` : Backup type (`Full` for complete backup, `Differential` for incremental backup).
- `<id>` : Identifier of the backup job to execute.
- `<id1;id2;id3>` : List of backup job IDs to run sequentially (non-consecutive).
- `<id1-id3>` : Range of backup job IDs to execute consecutively.

---

## Example Commands:
- Create a full backup job:
./easysave create backupExample C:\Users\Example D:\Backup\Example Full
- Execute a specific backup job:
./easysave 1
- Execute multiple non-sequential backup jobs:
./easysave 1;3;5
- Execute multiple consecutive backup jobs:
./easysave 2-4

---

## Credits
**Developed by:**  
**Groupe 3 - ProSoft**

---

This README provides basic usage instructions for EasySave. For more details, please refer to the complete documentation.