# CombExtLib

CombExtLib is a lightweight C# library designed for combining multiple files into a single file and extracting them back. This library is ideal for scenarios where you need to bundle multiple files together for easy distribution or need a convenient way to split combined files back into their original components.

## Features

- **File Combination**: Combine multiple files into a single binary file.
- **File Extraction**: Extract files from a combined binary file.
- **Metadata Handling**: Automatically generate and manage metadata to keep track of file information.
- **Directory Management**: Easily set and manage directories for combining and extracting files.
- **Error Handling**: Robust error handling to ensure smooth operation.

## Usage

```csharp
CombExt combExt = new CombExt();
combExt.SetDir("your-directory-path");
combExt.AddFile("file1.txt");
combExt.AddFile("file2.txt");
string combinedFilePath = combExt.Combine("combined.bin");
//Extract
CombExt combExt = new CombExt();
combExt.SetDir("your-directory-path");
string extractDir = combExt.Extract("combined-file-path");
