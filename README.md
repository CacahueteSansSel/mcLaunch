![The mcLaunch banner](mcLaunch/resources/banner.png)

> This project is in a very early stage, some things just don't work or aren't implemented yet !

<h3 align="center">mcLaunch</h3>

<p align="center">The Minecraft launcher that puts your Minecraft in a box 📦</p>

**mcLaunch** is a new and modern Minecraft launcher focused on design, compatibility, and ease of use.

![Main Screenshot](res/screenshot.png)
![Screenshot Collection](res/screenshots.png)

# Features

+ Launches every Minecraft version 🚀
+ Supports Fabric, Forge and Quilt 📜
+ Installs mods, and import modpacks from both CurseForge and Modrinth 🧩
+ Exports modpacks to a custom really small file format 🛫

# Install

## Windows x64
Download the installer [here](https://github.com/CacahueteSansSel/mcLaunch/releases/download/v0.1.1/mcLaunch.Installer.win64.exe) and follow the instructions. If you have any warning about "Windows protected your PC", ignore it.

# Build

Make sure to have the **.NET 7.0 SDK** installed, then clone and build the project :

```shell
$ cd mcLaunch
$ dotnet build
```

Then, you can run the project :
```shell
$ dotnet run
```

# Credits

Libraries used by the project :
+ Avalonia (UI Library)
+ ReactiveUI (UI Library)
+ Modrinth.Net (for Modrinth)
+ CurseForge.NET (for CurseForge)
+ K4os.Compression.LZ4 (compression, used by the Box Binary format)
+ ReverseMarkdown (to render mod pages)
+ Markdown.Avalonia (to render mod pages)
+ SharpNBT (for reading world's level.dat files)

This project takes huge inspiration of those other projects :
+ [portablemc](https://github.com/mindstorm38/portablemc) (general Minecraft launcher stuff + Forge installer wrapper code)
