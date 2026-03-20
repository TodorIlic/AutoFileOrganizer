# AutoFileOrganizer

A lightweight service built in C# .NET that automatically monitors a designated drop-zone and categorizes incoming files into organized subdirectories. 

```
/AutoFileOrganizer (Executable Location)
│
└─── /Sorting (Drop your files here!)
     │
     └─── /Sorted (Automatically generated and populated)
          ├── /Documents  (.pdf, .docx, .txt)
          ├── /Images     (.jpg, .png, .gif)
          ├── /Installers (.exe, .msi)
          └── /Duplicates (Identical SHA256 hashes)
```
## Features

* **Event-Driven Architecture:** Utilizes `FileSystemWatcher` to react to OS-level file creation events asynchronously without polling.
* **Smart Duplicate Detection:** Computes **SHA256** hashes of file contents to detect true duplicates, regardless of the file name, routing them to a safe `Duplicates` folder instead of deleting them.
* **Collision Handling:** Automatically appends timestamps to incoming files that share a name with an existing file but have different byte contents.
* **Stateful Memory Bank:** Indexes existing files on startup into an $O(1)$ `HashSet` to ensure duplicates aren't processed across application restarts.
* **Portable Execution:** Dynamically resolves the base directory (`AppDomain.CurrentDomain.BaseDirectory`), meaning the app can be dropped anywhere on a system and function without hardcoded paths.

## Tech Stack & Concepts Demonstrated

* **Language:** C# / .NET
