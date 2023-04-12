![The ddLaunch banner](ddLaunch/resources/banner.png)

<h3 align="center">ddLaunch</h3>

<p align="center">The Minecraft launcher that puts your Minecraft in a box 📦</p>

![Main Screenshot](res/screenshot.png)
![Screenshot Collection](res/screenshots.png)

# Features

+ Launches Minecraft 1.12.2 up to 1.19.4 🚀
+ Supports Fabric and Forge 📜
+ Installs mods from CurseForge and Modrinth 🧩
+ Imports modpacks from CurseForge 🛬
+ Exports modpacks to a really small file format 🛫

# Install

Work in progress

# Build

Make sure to have the **.NET 6.0 SDK** installed, then clone and build the project :

```shell
$ cd ddLaunch
$ dotnet build
```

Then, you can run the project :
```shell
$ dotnet run
```

# Credits

Libraries used by the project :
+ Avalonia (UI Library)
+ Modrinth.Net (for Modrinth)
+ CurseForge.NET (for CurseForge)
+ K4os.Compression.LZ4 (compression, used by the Box Binary format)
+ ReverseMarkdown (to render mod pages)
+ CmlLib.Core.Auth.Microsoft.MsalClient (for logging in with Microsoft)
+ Markdown.Avalonia (to render mod pages)