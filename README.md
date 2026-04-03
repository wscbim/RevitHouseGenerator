# RevitHouseGenerator

A plugin for **Autodesk Revit 2025** that procedurally generates modular house structures on a 120 x 120 cm grid.

## Features

- 4-level building generation: Ground Floor, 1st Floor, 2nd Floor, Attic
- Parametric modular grid (120 x 120 cm modules)
- Automatic exterior wall creation (Concept wall types: 50, 25, 12 cm)
- Automatic floor/slab creation (Concept floor types: 60, 35 cm)
- Interactive WPF dialog for configuring building dimensions
- Real-time metric size preview

## Installation

### Quick install (recommended)

1. Download the latest `.zip` from [Releases](../../releases)
2. Extract the archive
3. Right-click `install.ps1` > **Run with PowerShell**  
   _(or run `powershell -ExecutionPolicy Bypass -File install.ps1`)_
4. Restart Revit 2025

### Manual install

1. Download the latest `.zip` from [Releases](../../releases)
2. Extract `RevitHouseGenerator.dll` to:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2025\RevitHouseGenerator\
   ```
3. Copy `RevitHouseGenerator.addin` to:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2025\
   ```
4. In the `.addin` file, update the `<Assembly>` path to the full path of the DLL
5. Restart Revit 2025

## Usage

1. Open any Revit project
2. Go to the **DevWSC** tab in the ribbon
3. Click **Solid** in the Generator panel
4. Enter the number of modules (X and Y) in the dialog
5. Click OK to generate the house structure

## Uninstall

Run `uninstall.ps1` or manually delete:
- `%APPDATA%\Autodesk\Revit\Addins\2025\RevitHouseGenerator\`
- `%APPDATA%\Autodesk\Revit\Addins\2025\RevitHouseGenerator.addin`

## Building from source

### Requirements
- Visual Studio 2022 or .NET SDK
- Autodesk Revit 2025 installed (or build via `dotnet build` which uses NuGet API stubs)

```bash
dotnet build RevitHouseGenerator.csproj -c Release
```

Output: `bin/Release/net48/RevitHouseGenerator.dll`

## License

[MIT](LICENSE)

## Author

**Wojciech Cieplucha**  
Chair of Architectural Design, Cracow University of Technology  
wojciech.cieplucha@pk.edu.pl
