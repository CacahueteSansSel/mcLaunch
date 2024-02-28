![The mcLaunch banner](mcLaunch/resources/banner.png)
[![.NET](https://github.com/CacahueteSansSel/mcLaunch/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/CacahueteSansSel/mcLaunch/actions/workflows/dotnet.yml)
![GitHub License](https://img.shields.io/github/license/CacahueteSansSel/mcLaunch)
![GitHub Release](https://img.shields.io/github/v/release/CacahueteSansSel/mcLaunch)

> This project is in beta, some things just don't work or aren't implemented yet !

<h3 align="center">mcLaunch</h3>

<p align="center">The Minecraft launcher that puts your Minecraft in a box 📦</p>

**mcLaunch** (pronounced **m-c-launch**, **[ɛm si lɔntʃ]**) is a new and modern Minecraft launcher focused on design, compatibility, and ease of use.

![Main Screenshot](res/screenshot.png)

# Features

+ Launches every Minecraft version 🚀
+ Supports Fabric, Forge, NeoForge and Quilt 📜
+ Installs mods, and import modpacks from both CurseForge and Modrinth 🧩
+ Exports modpacks to a custom really small file format 🛫

# Install

## Windows x64
Download the installer [here](https://github.com/CacahueteSansSel/mcLaunch/releases/download/v0.1.1/mcLaunch.Installer.win64.exe) and follow the instructions. If you have any warning about "Windows protected your PC", ignore it. If you don't want to use the installer, download `mcLaunch-windows.zip` from [the latest release](https://github.com/CacahueteSansSel/mcLaunch/releases/latest), extract it in a folder, and run mcLaunch.exe from there.

## Linux x64
Download `mcLaunch-linux.zip` from [the latest release](https://github.com/CacahueteSansSel/mcLaunch/releases/latest) and extract it in a folder. You may need to mark the `mcLaunch` and the `mcLaunch.MinecraftGuard` files as executable before running mcLaunch.

# Build

Make sure to have the [**.NET 8.0 SDK**](https://dotnet.microsoft.com/en-us/download) installed, then clone the project.

Then, you can build and run the project :
```shell
$ cd mcLaunch
$ dotnet build
$ dotnet run
```

# About forking mcLaunch
If you fork mcLaunch and plan to create a derivative work out of it, you will need to do some changes :
+ You will need to remove every logos of mcLaunch and mentions of the mcLaunch name to replace with your own
+ You will need to replace the CurseForge API key and the Microsoft Azure App ID with your own created specifically for your derivative work
+ This derivative work will need to be open-source too, and with the same license, [according to it](LICENSE).

# Credits

Libraries used by the project :
+ [Avalonia](https://github.com/AvaloniaUI/Avalonia) (UI Library)
+ [ReactiveUI](https://github.com/reactiveui/ReactiveUI) (UI Library)
+ [Modrinth.Net](https://github.com/Zechiax/Modrinth.Net) (for Modrinth support in the launcher)
+ [CurseForge.NET](https://github.com/Raxdiam/CurseForge.NET) (for CurseForge support in the launcher)
+ [K4os.Compression.LZ4](https://github.com/MiloszKrajewski/K4os.Compression.LZ4) (compression, used by the Box Binary format)
+ [ReverseMarkdown](https://github.com/mysticmind/reversemarkdown-net) (to render mod pages)
+ [Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia) (to render mod pages)
+ [SharpNBT](https://github.com/ForeverZer0/SharpNBT) (for reading world's level.dat files)
+ [Markdig](https://github.com/xoofx/markdig) (to render mod pages)
+ [Jdenticon-net](https://github.com/dmester/jdenticon-net) (for generating anonymized box icons)
+ [DiscordRichPresence](https://github.com/Lachee/discord-rpc-csharp) (for Discord Rich Presence)

This project takes huge inspiration of :
+ [portablemc](https://github.com/mindstorm38/portablemc) (general Minecraft launcher stuff + Forge installer wrapper code)
